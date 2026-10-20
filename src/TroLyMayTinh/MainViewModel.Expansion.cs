using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;

namespace TroLyMayTinh;

public sealed partial class MainViewModel
{
    public ObservableCollection<ToolItem> WindowsSettings { get; } = [];
    public ObservableCollection<ToolItem> Utilities { get; } = [];
    public ObservableCollection<HardwareGroup> HardwareGroups { get; } = [];
    public ObservableCollection<CategoryOption> Categories { get; } = [];
    public ICollectionView ToolsView { get; private set; } = null!;
    public ICollectionView SettingsView { get; private set; } = null!;
    public ICollectionView UtilitiesView { get; private set; } = null!;
    private HardwareSnapshot? hardware;
    private bool collectingHardware;
    private string hardwareStatus = "";
    public string HardwareStatus { get => hardwareStatus; private set => Set(ref hardwareStatus, value); }
    private string quickSearch = "";
    public string QuickSearch { get => quickSearch; set { Set(ref quickSearch, value); RefreshQuickViews(); } }
    private bool favoritesOnly;
    public bool FavoritesOnly { get => favoritesOnly; set { Set(ref favoritesOnly, value); RefreshQuickViews(); } }
    private string category = "all";
    public string Category { get => category; set { Set(ref category, value ?? "all"); SoftwareView.Refresh(); Changed(nameof(NoApps)); } }
    private string utilityOutput = "";
    public string UtilityOutput { get => utilityOutput; private set => Set(ref utilityOutput, value); }
    public bool NoTools => ToolsView.IsEmpty;
    public bool NoSettings => SettingsView.IsEmpty;
    public bool NoUtilities => UtilitiesView.IsEmpty;
    public ICommand ExportHardwareCommand { get; private set; } = null!;
    public ICommand OfficePresetCommand { get; private set; } = null!;
    public ICommand DeveloperPresetCommand { get; private set; } = null!;
    public ICommand ClearSoftwareCommand { get; private set; } = null!;
    public ICommand CopyUtilityCommand { get; private set; } = null!;
    public ICommand ChooseSetupCommand { get; private set; } = null!;
    public ICommand DownloadWindowsCommand { get; private set; } = null!;
    public ICommand WindowsSettingsPageCommand { get; private set; } = null!;
    public ICommand UtilitiesPageCommand { get; private set; } = null!;
    public ICommand SetupPageCommand { get; private set; } = null!;

    private void InitializeExpansion()
    {
        ToolsView = CollectionViewSource.GetDefaultView(Tools);
        SettingsView = CollectionViewSource.GetDefaultView(WindowsSettings);
        UtilitiesView = CollectionViewSource.GetDefaultView(Utilities);
        foreach (var view in new[] { ToolsView, SettingsView, UtilitiesView })
            view.Filter = x => x is ToolItem item && (!FavoritesOnly || item.Favorite) && (item.Name + " " + item.Description + " " + item.Group + " " + item.Id).Contains(QuickSearch.Trim(), StringComparison.CurrentCultureIgnoreCase);
        ExportHardwareCommand = new AsyncCommand(() => Run(ExportHardware));
        OfficePresetCommand = new ActionCommand(() => SelectPreset(["UniKey.UniKey", "7zip.7zip", "Google.Chrome", "TheDocumentFoundation.LibreOffice", "SumatraPDF.SumatraPDF"]));
        DeveloperPresetCommand = new ActionCommand(() => SelectPreset(["7zip.7zip", "Microsoft.VisualStudioCode", "Git.Git", "GitHub.GitHubDesktop", "OpenJS.NodeJS.LTS", "Python.Python.3.13"]));
        ClearSoftwareCommand = new ActionCommand(() => { if (!Busy) foreach (var app in Software) app.Selected = false; });
        CopyUtilityCommand = new ActionCommand(() => { try { if (!string.IsNullOrEmpty(UtilityOutput)) Clipboard.SetText(UtilityOutput); } catch (Exception ex) { Fail(ex); } });
        ChooseSetupCommand = new AsyncCommand(() => Run(ChooseSetup));
        DownloadWindowsCommand = new ActionCommand(() => SafeOpen("https://www.microsoft.com/software-download/windows11"));
        WindowsSettingsPageCommand = new ActionCommand(() => Page = 9);
        UtilitiesPageCommand = new ActionCommand(() => Page = 10);
        SetupPageCommand = new ActionCommand(() => Page = 11);
    }

    private bool MatchesSoftware(SoftwareItem app) => (Category == "all" || app.Category == Category) && (app.Name + " " + app.Id + " " + app.Description).Contains(Search.Trim(), StringComparison.CurrentCultureIgnoreCase);
    private void SelectPreset(string[] ids)
    {
        if (Busy) return;
        foreach (var app in Software) app.Selected = ids.Contains(app.Id, StringComparer.Ordinal);
        Search = ""; Category = "all";
        Status = T("Đã chọn bộ phần mềm. Anh có thể thay đổi trước khi bấm Cài.", "Preset selected. Review your selection before installing.");
    }
    private ToolItem MakeTool(string name, string description, string icon, string id, string group, ICommand command)
    {
        var item = new ToolItem(name, description, icon, command, id, group) { Favorite = preferences.FavoriteTools.Contains(id) };
        item.FavoriteCommand = new ActionCommand(() =>
        {
            bool desired = !item.Favorite;
            foreach (var copy in Tools.Concat(WindowsSettings).Concat(Utilities).Where(x => x.Id == id)) copy.Favorite = desired;
            if (desired) preferences.FavoriteTools.Add(id); else preferences.FavoriteTools.Remove(id);
            SavePreferences(); RefreshQuickViews();
        });
        return item;
    }
    private void RefreshQuickViews()
    {
        ToolsView?.Refresh(); SettingsView?.Refresh(); UtilitiesView?.Refresh();
        Changed(nameof(NoTools)); Changed(nameof(NoSettings)); Changed(nameof(NoUtilities));
    }
    private void TranslateExpansion()
    {
        string chosen = Category;
        Categories.Clear(); foreach (var item in SoftwareCatalog.Categories(English)) Categories.Add(item);
        Category = chosen;
        WindowsSettings.Clear();
        foreach (var entry in SettingCatalog.Entries)
        {
            string group = T(entry.GroupVi, entry.GroupEn);
            WindowsSettings.Add(MakeTool(T(entry.NameVi, entry.NameEn), entry.Uri, "⚙", entry.Uri, group, new ActionCommand(() => OpenTool(entry.Uri))));
        }
        Utilities.Clear();
        AddUtility(T("Thư mục Tải xuống", "Downloads folder"), T("Mở thư mục Downloads của Windows", "Open the Windows Downloads folder"), "↓", "downloads", () => OpenKnownFolder("downloads"));
        AddUtility(T("Tài liệu", "Documents"), T("Mở thư mục tài liệu cá nhân", "Open your documents folder"), "▤", "documents", () => OpenKnownFolder("documents"));
        AddUtility(T("File tạm cá nhân", "User temporary files"), T("Mở thư mục đang được công cụ dọn dẹp quét", "Open the folder used by Cleanup"), "▣", "temp", () => { Directory.CreateDirectory(Cleaner.Root); SafeOpen(Cleaner.Root); });
        AddUtility(T("Chụp màn hình", "Screen capture"), T("Mở công cụ chụp vùng màn hình", "Open screen snipping"), "▧", "capture", () => OpenTool("ms-screenclip:"));
        AddUtility(T("Máy tính", "Calculator"), T("Mở Calculator", "Open Calculator"), "⊞", "calc", () => OpenTool("calc"));
        AddUtility("Notepad", T("Soạn ghi chú nhanh", "Create a quick note"), "≡", "notepad", () => OpenTool("notepad"));
        AddUtility(T("Bàn phím ảo", "On-screen keyboard"), T("Hỗ trợ nhập bằng chuột", "Mouse-based keyboard input"), "⌨", "osk", () => OpenTool("osk"));
        AddUtility(T("Kính lúp", "Magnifier"), T("Phóng to nội dung màn hình", "Magnify the display"), "⊕", "magnify", () => OpenTool("magnify"));
        AddUtility(T("Bảng ký tự", "Character Map"), T("Tra và sao chép ký tự", "Find and copy characters"), "Ω", "charmap", () => OpenTool("charmap"));
        Utilities.Add(MakeTool(T("Thông tin mạng", "Network details"), T("Xem IP, gateway và DNS", "View IP, gateway and DNS"), "◎", "network-report", T("Báo cáo", "Reports"), new AsyncCommand(() => Run(() => { UtilityOutput = HardwareService.GetNetwork(); return Task.CompletedTask; }))));
        Utilities.Add(MakeTool(T("Báo cáo pin", "Battery report"), T("Tạo báo cáo HTML bằng powercfg", "Create an HTML powercfg report"), "▰", "battery-report", T("Báo cáo", "Reports"), new AsyncCommand(() => Run(BatteryReport))));
        Utilities.Add(MakeTool(T("Kiểm tra SHA-256", "SHA-256 file hash"), T("Tính mã kiểm tra của một tệp", "Calculate a file's checksum"), "#", "file-hash", T("Tệp", "Files"), new AsyncCommand(() => Run(HashFile))));
        AddTool(T("Lập lịch tác vụ", "Task Scheduler"), T("Xem các tác vụ theo lịch", "View scheduled tasks"), "◷", "taskschd");
        AddTool(T("Chẩn đoán DirectX", "DirectX Diagnostic"), T("Thông tin đồ họa và âm thanh", "Graphics and sound information"), "◇", "dxdiag");
        AddTool(T("Tính năng Windows", "Windows Features"), T("Mở trình quản lý tính năng tùy chọn", "Open optional feature management"), "⊞", "optionalfeatures");
        AddTool(T("Dọn ổ đĩa Windows", "Disk Cleanup"), T("Mở công cụ dọn dẹp có sẵn", "Open the built-in cleanup tool"), "▣", "cleanmgr");
        AddTool(T("Bộ điều hợp mạng", "Network adapters"), T("Mở bảng kết nối mạng", "Open network connections"), "◎", "ncpa");
        AddTool(T("Tùy chọn nguồn", "Power Options"), T("Mở các chế độ nguồn", "Open power plans"), "ϟ", "power");
        AddTool(T("Thuộc tính hệ thống", "System Properties"), T("Thiết lập hệ thống nâng cao", "Advanced system settings"), "⚙", "sysdm");
        AddTool(T("Phiên bản Windows", "Windows version"), T("Mở hộp thoại winver", "Open the winver dialog"), "ⓘ", "winver");
        AddTool(T("Chứng chỉ cá nhân", "User certificates"), T("Mở kho chứng chỉ tài khoản hiện tại", "Open current-user certificates"), "◇", "certmgr");
        PresentHardware(); RefreshQuickViews();
    }
    private void AddUtility(string name, string description, string icon, string id, Action action) => Utilities.Add(MakeTool(name, description, icon, id, T("Tiện ích", "Utilities"), new ActionCommand(() => { try { action(); } catch (Exception ex) { Fail(ex); } })));
    private void OpenKnownFolder(string id)
    {
        if (id == "documents") { SafeOpen(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)); return; }
        // Shell known-folder identity respects redirected/localized Downloads paths.
        SafeOpen("shell:::{374DE290-123F-4565-9164-39C4925E467B}");
    }
    private async Task RefreshHardware()
    {
        if (collectingHardware) return;
        collectingHardware = true;
        HardwareStatus = T("Đang đọc thông số phần cứng…", "Reading hardware information…");
        try
        {
            RefreshSystem();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); timeout.CancelAfter(TimeSpan.FromSeconds(45));
            hardware = await Task.Run(() => HardwareService.Collect(timeout.Token), timeout.Token);
            PresentHardware();
        }
        catch (OperationCanceledException) { HardwareStatus = T("Truy vấn đã dừng hoặc quá thời gian. Bấm Làm mới để thử lại.", "Query cancelled or timed out. Refresh to retry."); }
        catch (Exception ex) { HardwareStatus = ex.Message; AppLog.Write("Hardware query failed", ex); }
        finally { collectingHardware = false; }
    }
    private void PresentHardware()
    {
        if (hardware is null) { HardwareStatus = T("Chưa có dữ liệu. Bấm Làm mới để đọc thông số.", "No snapshot yet. Refresh to read hardware information."); return; }
        HardwareGroups.Clear(); foreach (var group in HardwareService.Present(hardware, English)) HardwareGroups.Add(group);
        HardwareStatus = T("Lần đọc gần nhất: ", "Last collected: ") + hardware.CollectedAt.ToString("dd/MM/yyyy HH:mm:ss");
    }
    private async Task ExportHardware()
    {
        if (hardware is null) await RefreshHardware();
        if (hardware is null) return;
        var dialog = new SaveFileDialog { Filter = "Text report (*.txt)|*.txt", FileName = "ThongSoMayTinh-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".txt" };
        if (dialog.ShowDialog() != true) return;
        string report = "Trợ Lý Máy Tính " + Version + "\n" + HardwareStatus + "\n\n" + string.Join("\n\n", HardwareGroups.Select(x => x.Title + "\n" + string.Join("\n", x.Fields.Select(f => f.Label + ": " + f.Value))));
        await File.WriteAllTextAsync(dialog.FileName, report, new UTF8Encoding(true), lifetime.Token);
        Status = T("Đã lưu báo cáo: ", "Report saved: ") + dialog.FileName;
    }
    private async Task BatteryReport()
    {
        string folder = Path.Combine(Preferences.DataFolder, "Reports"); Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, "battery-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".html");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); timeout.CancelAfter(TimeSpan.FromSeconds(30));
        var result = await ProcessRunner.Run(Path.Combine(Environment.SystemDirectory, "powercfg.exe"), ["/batteryreport", "/output", path], timeout.Token);
        if (result.Code != 0 || !File.Exists(path)) throw new InvalidOperationException(T("Không tạo được báo cáo. Máy có thể không có pin hoặc không đủ quyền. ", "Could not create report. No battery or insufficient permission. ") + result.Error + result.Output);
        UtilityOutput = T("Đã lưu báo cáo pin: ", "Battery report saved: ") + path; SafeOpen(path);
    }
    private async Task HashFile()
    {
        var dialog = new OpenFileDialog { Title = T("Chọn tệp để tính SHA-256", "Choose a file to hash"), CheckFileExists = true };
        if (dialog.ShowDialog() != true) return;
        await using var file = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        UtilityOutput = T("Đang tính SHA-256…", "Calculating SHA-256…");
        string hash = Convert.ToHexString(await SHA256.HashDataAsync(file, lifetime.Token));
        UtilityOutput = dialog.FileName + "\nSHA-256: " + hash;
    }
    private async Task ChooseSetup()
    {
        var dialog = new OpenFileDialog { Filter = "Windows Setup (setup.exe)|setup.exe", Title = T("Chọn setup.exe trong bộ cài Windows", "Select setup.exe from Windows installation media"), CheckFileExists = true };
        if (dialog.ShowDialog() != true) return;
        string path = Path.GetFullPath(dialog.FileName);
        if (!Path.GetFileName(path).Equals("setup.exe", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Select setup.exe.");
        string media = Path.GetDirectoryName(path)!;
        if (!new[] { "install.wim", "install.esd", "install.swm" }.Any(name => File.Exists(Path.Combine(media, "sources", name))))
            throw new InvalidDataException(T("Không tìm thấy ảnh cài Windows trong thư mục sources. Hãy chọn setup.exe ở thư mục gốc bộ cài.", "No Windows installation image in sources. Select setup.exe at the installation media root."));
        var original = new FileInfo(path); long originalSize = original.Length; DateTime originalWrite = original.LastWriteTimeUtc;
        string escaped = path.Replace("'", "''");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); timeout.CancelAfter(TimeSpan.FromSeconds(30));
        var signature = await ProcessRunner.Script("$ErrorActionPreference='Stop'; $s=Get-AuthenticodeSignature -LiteralPath '" + escaped + "'; [pscustomobject]@{Status=$s.Status.ToString();Signer=$(if($s.SignerCertificate){$s.SignerCertificate.GetNameInfo([System.Security.Cryptography.X509Certificates.X509NameType]::SimpleName,$false)}else{''});Subject=$(if($s.SignerCertificate){$s.SignerCertificate.Subject}else{''})} | ConvertTo-Json -Compress", timeout.Token);
        if (signature.Code != 0) throw new InvalidOperationException("Cannot validate Windows Setup signature: " + signature.Error);
        using var doc = JsonDocument.Parse(signature.Output);
        string signer = doc.RootElement.GetProperty("Signer").GetString() ?? "";
        string subject = doc.RootElement.GetProperty("Subject").GetString() ?? "";
        if (doc.RootElement.GetProperty("Status").GetString() != "Valid" || !new[] { "Microsoft Corporation", "Microsoft Windows", "Microsoft Windows Publisher" }.Contains(signer, StringComparer.Ordinal) || !System.Text.RegularExpressions.Regex.IsMatch(subject, "(?:^|,\\s*)O=\"?Microsoft Corporation\"?(?:,|$)"))
            throw new InvalidDataException(T("Chưa xác minh được chữ ký Microsoft hợp lệ. Không mở bộ cài này.", "A valid Microsoft signature could not be verified. Setup will not be launched."));
        if (!Confirm("Mở bộ cài Windows của Microsoft?\n\n" + path + "\n\nAnh sẽ tự chọn các bước tiếp theo trong Windows Setup. Hãy kiểm tra bản sao lưu trước khi chọn cài mới hoặc thay đổi dữ liệu.", "Open Microsoft Windows Setup?\n\n" + path + "\n\nChoose subsequent actions in Windows Setup. Verify backups before a clean installation or changing data.")) return;
        original.Refresh();
        if (!original.Exists || original.Length != originalSize || original.LastWriteTimeUtc != originalWrite) throw new InvalidDataException("Setup changed after verification. Select it again.");
        using var launched = Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, WorkingDirectory = Path.GetDirectoryName(path)! }) ?? throw new InvalidOperationException("Windows Setup did not start.");
        Status = T("Đã mở Windows Setup. Tiếp tục trong cửa sổ bộ cài.", "Windows Setup opened. Continue in its window."); AppLog.Write(Status);
    }
}
