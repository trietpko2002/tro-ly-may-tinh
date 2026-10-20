using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace TroLyMayTinh;

public sealed partial class MainViewModel : Observable
{
    private readonly Preferences preferences = Preferences.Load();
    private readonly CancellationTokenSource lifetime = new();
    private CancellationTokenSource? cleanup;
    private ReleaseInfo? pendingRelease;
    private bool checkingUpdate;
    private bool busy;
    public bool Busy { get => busy; private set { if (Set(ref busy, value)) Changed(nameof(NotBusy)); } }
    public bool NotBusy => !Busy;
    public bool IsRestarting { get; private set; }
    public Dictionary<string, string> Labels { get; private set; } = [];
    public ObservableCollection<string> Navigation { get; } = [];
    public ObservableCollection<SoftwareItem> Software { get; } = [];
    public ICollectionView SoftwareView { get; }
    public ObservableCollection<ToolItem> Tools { get; } = [];
    public ObservableCollection<CleanupItem> CleanupFiles { get; } = [];
    private readonly List<string> activity = [];
    private string logText = "";
    public string LogText { get => logText; private set => Set(ref logText, value); }
    private string status = "";
    public string Status { get => status; private set => Set(ref status, value); }
    private int page;
    public int Page { get => page; set { Set(ref page, value); Changed(nameof(Title)); Changed(nameof(Subtitle)); } }
    private static readonly string[] Pages = ["home", "apps", "clean", "tweaks", "tools", "updates", "logs", "settings", "about", "winsettings", "utilities", "setup"];
    public string Title => Labels[Pages[Math.Clamp(Page, 0, Pages.Length - 1)]];
    public string Subtitle => Labels[Pages[Math.Clamp(Page, 0, Pages.Length - 1)] + "Sub"];
    private string search = "";
    public string Search { get => search; set { Set(ref search, value); SoftwareView.Refresh(); Changed(nameof(NoApps)); } }
    public bool NoApps => SoftwareView.IsEmpty;
    public bool English { get => preferences.Language == "en"; set { preferences.Language = value ? "en" : "vi"; Translate(); SavePreferences(); Changed(); } }
    public bool Dark { get => preferences.Dark; set { preferences.Dark = value; ApplyTheme(); SavePreferences(); Changed(); } }
    public bool AutoCheck { get => preferences.CheckUpdates; set { preferences.CheckUpdates = value; SavePreferences(); Changed(); } }
    private bool extensions;
    public bool Extensions { get => extensions; set => Set(ref extensions, value); }
    private bool hidden;
    public bool Hidden { get => hidden; set => Set(ref hidden, value); }
    private string cleanSummary = "";
    public string CleanSummary { get => cleanSummary; private set => Set(ref cleanSummary, value); }
    public bool EmptyScan => CleanupFiles.Count == 0;
    private string updateStatus = "";
    public string UpdateStatus { get => updateStatus; private set => Set(ref updateStatus, value); }
    private string releaseNotes = "";
    public string ReleaseNotes { get => releaseNotes; private set => Set(ref releaseNotes, value); }
    public bool CanUpdate => pendingRelease is not null && UpdateService.CanApply(pendingRelease);
    public string Device => Environment.MachineName;
    public string Architecture => RuntimeInformation.ProcessArchitecture.ToString();
    public string WindowsVersion { get; private set; } = "Windows";
    public string FreeSpace { get; private set; } = "—";
    public string Version => UpdateService.CurrentVersion;
    public ICommand InstallCommand { get; }
    public ICommand DetectCommand { get; }
    public ICommand ScanCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SelectAllCommand { get; }
    public ICommand ClearSelectionCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ApplyTweaksCommand { get; }
    public ICommand RestoreTweaksCommand { get; }
    public ICommand CheckUpdateCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand ReleasesCommand { get; }
    public ICommand OpenLogsCommand { get; }
    public ICommand CopyLogsCommand { get; }
    public ICommand GithubCommand { get; }
    public ICommand AppsPageCommand { get; }
    public ICommand CleanupPageCommand { get; }
    public ICommand ToolsPageCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand StartupCommand { get; }
    public ICommand RecoveryCommand { get; }

    public MainViewModel()
    {
        SoftwareView = CollectionViewSource.GetDefaultView(Software);
        SoftwareView.Filter = x => x is SoftwareItem a && MatchesSoftware(a);
        foreach (var app in SoftwareCatalog.Load()) Software.Add(app);
        InitializeExpansion();
        InstallCommand = new AsyncCommand(() => Run(InstallSelected));
        DetectCommand = new AsyncCommand(() => Run(DetectInstalled));
        ScanCommand = new AsyncCommand(() => Run(Scan));
        DeleteCommand = new AsyncCommand(() => Run(DeleteSelected));
        SelectAllCommand = new ActionCommand(() => { if (!Busy) foreach (var item in CleanupFiles) item.Selected = true; });
        ClearSelectionCommand = new ActionCommand(() => { if (!Busy) foreach (var item in CleanupFiles) item.Selected = false; });
        CancelCommand = new ActionCommand(() => cleanup?.Cancel());
        ApplyTweaksCommand = new AsyncCommand(() => Run(ApplyTweaks));
        RestoreTweaksCommand = new AsyncCommand(() => Run(RestoreTweaks));
        CheckUpdateCommand = new AsyncCommand(() => Run(CheckUpdate));
        UpdateCommand = new AsyncCommand(() => Run(ApplyUpdate));
        ReleasesCommand = new ActionCommand(() => SafeOpen($"https://github.com/{UpdateService.Owner}/{UpdateService.Repository}/releases"));
        GithubCommand = new ActionCommand(() => SafeOpen("https://github.com/trietpko2002"));
        OpenLogsCommand = new ActionCommand(() => { Directory.CreateDirectory(Preferences.DataFolder); SafeOpen(Preferences.DataFolder); });
        CopyLogsCommand = new ActionCommand(() => { try { Clipboard.SetText(LogText); } catch (Exception ex) { Fail(ex); } });
        AppsPageCommand = new ActionCommand(() => Page = 1);
        CleanupPageCommand = new ActionCommand(() => Page = 2);
        ToolsPageCommand = new ActionCommand(() => Page = 4);
        RefreshCommand = new AsyncCommand(() => Run(RefreshHardware));
        StartupCommand = new ActionCommand(() => OpenTool("ms-settings:startupapps"));
        RecoveryCommand = new ActionCommand(() => OpenTool("ms-settings:recovery"));
        Translate(); ApplyTheme(); RefreshSystem(); ReadTweaks();
        AppLog.Written += OnLog;
        AppLog.Write("Application started. Version " + Version);
    }

    private string T(string vi, string en) => English ? en : vi;
    private void AddSoftware(string name, string id, string publisher, string vi, string en, string[] detection) => Software.Add(new() { Name = name, Id = id, Publisher = publisher, DescriptionVi = vi, DescriptionEn = en, DetectionNames = detection });
    private void Translate()
    {
        Labels = Localization.Create(English);
        Labels["version"] = T("Bản thử nghiệm ", "Preview ") + Version;
        Changed(nameof(Labels));
        int selected = Page;
        Navigation.Clear(); foreach (string key in Pages) Navigation.Add(Labels[key]);
        Page = selected;
        foreach (var app in Software) app.Translate(English);
        SoftwareView.Refresh(); Changed(nameof(NoApps));
        Tools.Clear();
        AddTool(T("Trình quản lý tác vụ", "Task Manager"), T("Xem tiến trình và hiệu suất", "Processes and performance"), "◈", "taskmgr");
        AddTool(T("Quản lý ổ đĩa", "Disk Management"), T("Xem ổ đĩa và phân vùng", "Drives and partitions"), "▣", "diskmgmt");
        AddTool(T("Quản lý thiết bị", "Device Manager"), T("Kiểm tra thiết bị và driver", "Devices and drivers"), "⌘", "devmgmt");
        AddTool(T("Dịch vụ hệ thống", "Services"), T("Mở bảng dịch vụ Windows", "Windows services console"), "⚙", "services");
        AddTool(T("Thông tin hệ thống", "System Information"), T("Cấu hình máy chi tiết", "Detailed hardware information"), "ⓘ", "msinfo");
        AddTool(T("Giám sát tài nguyên", "Resource Monitor"), T("CPU, bộ nhớ, ổ đĩa và mạng", "CPU, memory, disk and network"), "↗", "resmon");
        AddTool("Windows Update", T("Kiểm tra cập nhật Windows", "Windows update settings"), "↻", "ms-settings:windowsupdate");
        AddTool(T("Mạng và Internet", "Network and Internet"), T("Kết nối và cài đặt mạng", "Connections and network settings"), "◎", "ms-settings:network-status");
        AddTool(T("Ứng dụng đã cài", "Installed apps"), T("Quản lý và gỡ ứng dụng", "Manage and uninstall apps"), "▦", "ms-settings:appsfeatures");
        AddTool("Windows Security", T("Mở bảng bảo vệ Windows", "Open Windows protection"), "◇", "ms-settings:windowsdefender");
        AddTool(T("Nhật ký sự kiện", "Event Viewer"), T("Tra cứu sự kiện hệ thống", "System event logs"), "≡", "eventvwr");
        AddTool("Control Panel", T("Các thiết lập hệ thống cổ điển", "Classic system settings"), "⊞", "control");
        TranslateExpansion();
        Status = Labels["ready"];
    }
    private void AddTool(string name, string description, string icon, string command) => Tools.Add(MakeTool(name, description, icon, command, T("Công cụ hệ thống", "System tools"), new ActionCommand(() => OpenTool(command))));
    private void OpenTool(string command) { try { WindowsTools.Open(command); } catch (Exception ex) { Fail(ex); } }
    private void SafeOpen(string target) { try { AppLog.Open(target); } catch (Exception ex) { Fail(ex); } }
    private void SavePreferences() { try { preferences.Save(); } catch (Exception ex) { Fail(ex); } }
    private void Fail(Exception ex) { Status = T("Không hoàn tất: ", "Could not complete: ") + ex.Message; AppLog.Write(Status, ex); }
    private async Task Run(Func<Task> action)
    {
        if (Busy) return;
        Busy = true;
        try { await action(); }
        catch (OperationCanceledException) { Status = T("Đã dừng tác vụ. Thao tác đã hoàn tất vẫn được giữ lại.", "Task cancelled. Completed changes are retained."); AppLog.Write(Status); }
        catch (Exception ex) { Fail(ex); MessageBox.Show(Status, "Trợ Lý Máy Tính", MessageBoxButton.OK, MessageBoxImage.Warning); }
        finally { Busy = false; }
    }
    private bool Confirm(string vi, string en) => MessageBox.Show(T(vi, en), "Trợ Lý Máy Tính", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
    private void OnLog(string message) => Application.Current.Dispatcher.InvokeAsync(() => { activity.Add(message); if (activity.Count > 500) activity.RemoveAt(0); LogText = string.Join(Environment.NewLine, activity); });
    public void Dispose() { lifetime.Cancel(); cleanup?.Cancel(); AppLog.Written -= OnLog; lifetime.Dispose(); }
    public async Task Startup() { await Task.WhenAll(RefreshHardware(), AutoCheck ? CheckUpdate() : Task.CompletedTask); }

    private void RefreshSystem()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            var build = key?.GetValue("CurrentBuildNumber")?.ToString() ?? Environment.OSVersion.Version.Build.ToString();
            var display = key?.GetValue("DisplayVersion")?.ToString() ?? "";
            WindowsVersion = $"Windows {(int.TryParse(build, out int b) && b >= 22000 ? "11" : "10")} {display} · {build}";
            var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory)!);
            FreeSpace = FormatBytes(drive.AvailableFreeSpace);
        }
        catch (Exception ex) { AppLog.Write("System info partially unavailable", ex); }
        Changed(nameof(WindowsVersion)); Changed(nameof(FreeSpace));
    }
    public static string FormatBytes(long value) => value >= 1073741824 ? $"{value / 1073741824.0:F1} GB" : value >= 1048576 ? $"{value / 1048576.0:F1} MB" : value >= 1024 ? $"{value / 1024.0:F1} KB" : $"{value} B";
    private async Task DetectInstalled()
    {
        Status = T("Đang kiểm tra phần mềm…", "Detecting apps…");
        var snapshot = Software.ToArray();
        var results = await Task.Run(() => snapshot.Select(app => (App: app, Version: SoftwareInstaller.FindInstalledVersion(app.DetectionNames))).ToArray());
        foreach (var item in results) item.App.State = item.Version is null ? T("Chưa nhận diện trong danh sách cài đặt", "Not detected in installed-app registry") : T("Đã cài · ", "Installed · ") + item.Version;
        Status = T("Đã kiểm tra. Bản portable có thể không được nhận diện.", "Detection complete. Portable apps may not be detected.");
        AppLog.Write(Status);
    }
    private async Task InstallSelected()
    {
        var selected = Software.Where(x => x.Selected).ToArray();
        if (selected.Length == 0) { Status = T("Hãy chọn ít nhất một phần mềm.", "Select at least one app."); return; }
        if (!SoftwareInstaller.Available) { SafeOpen("ms-windows-store://pdp/?ProductId=9NBLGGH4NNS1"); throw new InvalidOperationException(T("Cài App Installer từ Microsoft Store rồi thử lại.", "Install App Installer from Microsoft Store, then retry.")); }
        if (!Confirm("Cài các phần mềm đã chọn?\n\n" + string.Join("\n", selected.Select(x => x.Name)) + "\n\nTiếp tục đồng nghĩa với chấp nhận điều khoản nguồn WinGet và giấy phép các gói. Bộ cài có thể hỏi quyền quản trị.", "Install selected apps?\n\n" + string.Join("\n", selected.Select(x => x.Name)) + "\n\nContinuing accepts WinGet source and package agreements. Installers may request administrator permission.")) return;
        int succeeded = 0;
        foreach (var app in selected)
        {
            Status = T("Đang xử lý: ", "Processing: ") + app.Name; app.State = Status;
            try
            {
                int code = await SoftwareInstaller.Install(app.Id, new Progress<string>(line => { AppLog.Write(app.Name + ": " + line); Status = app.Name + " · " + (line.Length > 180 ? line[..180] : line); }));
                if (code == 0) { succeeded++; app.Selected = false; app.State = T("Hoàn tất", "Completed"); }
                else app.State = T("WinGet trả mã ", "WinGet exit code ") + $"{code} (0x{unchecked((uint)code):X8})";
                AppLog.Write(app.Name + ": " + app.State);
            }
            catch (Exception ex) { app.State = ex.Message; AppLog.Write("Install failed: " + app.Name, ex); }
        }
        Status = T($"Hoàn tất {succeeded}/{selected.Length} tác vụ. Xem nhật ký cho các mục còn lại.", $"Completed {succeeded}/{selected.Length} tasks. Check activity for the remaining items.");
    }
    private async Task Scan()
    {
        cleanup?.Dispose(); cleanup = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        CleanupFiles.Clear(); Changed(nameof(EmptyScan));
        Status = T("Đang quét file tạm…", "Scanning temporary files…");
        var files = await Task.Run(() => Cleaner.Scan(cleanup.Token), cleanup.Token);
        foreach (var file in files) CleanupFiles.Add(new(file));
        CleanSummary = T($"{files.Count} file có thể chọn · {FormatBytes(files.Sum(x => x.Length))}", $"{files.Count} selectable files · {FormatBytes(files.Sum(x => x.Length))}");
        Changed(nameof(EmptyScan)); Status = CleanSummary; AppLog.Write(Status);
    }
    private async Task DeleteSelected()
    {
        var selected = CleanupFiles.Where(x => x.Selected).Select(x => x.Entry).ToArray();
        if (selected.Length == 0) { Status = T("Hãy chọn file muốn xóa.", "Select files to delete."); return; }
        if (!Confirm($"Xóa vĩnh viễn {selected.Length} file tạm đã chọn ({FormatBytes(selected.Sum(x => x.Length))})?\nKhông thể hoàn tác từ Thùng rác.", $"Permanently delete {selected.Length} selected temporary files ({FormatBytes(selected.Sum(x => x.Length))})?\nThis cannot be undone through the Recycle Bin.")) return;
        cleanup?.Dispose(); cleanup = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        try
        {
            var result = await Task.Run(() => Cleaner.Delete(selected, cleanup.Token), cleanup.Token);
            Status = T($"Đã xóa {result.Deleted} file · {FormatBytes(result.Bytes)} · Bỏ qua {result.Skipped}.", $"Deleted {result.Deleted} files · {FormatBytes(result.Bytes)} · Skipped {result.Skipped}.");
            AppLog.Write(Status);
        }
        finally
        {
            foreach (var item in CleanupFiles.Where(x => !File.Exists(x.Entry.Path)).ToArray()) CleanupFiles.Remove(item);
            Changed(nameof(EmptyScan));
            CleanSummary = T($"Còn {CleanupFiles.Count} file trong kết quả quét.", $"{CleanupFiles.Count} files remain in the scan results.");
        }
    }
    private void ReadTweaks() { try { Extensions = RegistryTweaks.Read("HideFileExt") == 0; Hidden = RegistryTweaks.Read("Hidden") == 1; } catch (Exception ex) { Fail(ex); } }
    private Task ApplyTweaks()
    {
        if (Confirm("Áp dụng hai tùy chọn Explorer đang hiển thị? Giá trị cũ được lưu để khôi phục.", "Apply both displayed Explorer settings? Previous values will be saved."))
        { RegistryTweaks.Apply("HideFileExt", Extensions ? 0 : 1, preferences); RegistryTweaks.Apply("Hidden", Hidden ? 1 : 2, preferences); Status = Labels["tweakHint"]; }
        return Task.CompletedTask;
    }
    private Task RestoreTweaks()
    {
        if (preferences.RegistryOriginals.Count == 0) { Status = T("Chưa có thay đổi đã lưu để khôi phục.", "No saved changes to restore."); return Task.CompletedTask; }
        if (Confirm("Khôi phục các giá trị Explorer trước khi app chỉnh sửa lần đầu?", "Restore Explorer values from before this app first changed them?"))
        { RegistryTweaks.Restore(preferences); ReadTweaks(); Status = Labels["tweakHint"]; }
        return Task.CompletedTask;
    }
    private async Task CheckUpdate()
    {
        if (checkingUpdate) return;
        checkingUpdate = true;
        UpdateStatus = T("Đang kết nối GitHub…", "Connecting to GitHub…");
        pendingRelease = null; Changed(nameof(CanUpdate)); ReleaseNotes = "";
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); timeout.CancelAfter(TimeSpan.FromSeconds(25));
            pendingRelease = await UpdateService.Check(timeout.Token);
            UpdateStatus = pendingRelease is null ? T($"Không tìm thấy bản ổn định mới hơn {Version}.", $"No stable version newer than {Version} found.") : T("Có phiên bản mới: ", "New version: ") + pendingRelease.Version;
            if (pendingRelease is not null && !CanUpdate) UpdateStatus += T(" · Chưa có gói tương thích kèm SHA-256.", " · No compatible package with SHA-256 available.");
            ReleaseNotes = pendingRelease?.Notes ?? "";
        }
        catch (Exception ex) { UpdateStatus = T("Chưa thể kiểm tra: ", "Could not check: ") + ex.Message; }
        finally { checkingUpdate = false; }
        Changed(nameof(CanUpdate)); AppLog.Write(UpdateStatus);
    }
    private async Task ApplyUpdate()
    {
        if (pendingRelease is null || !CanUpdate) return;
        if (!Confirm("Tải bản cập nhật đã chọn? Sau khi xác minh SHA-256, ứng dụng sẽ đóng, thay file EXE và mở lại. File cũ được lưu với đuôi .previous.", "Download the selected update? After SHA-256 verification, the app will close, replace its EXE and restart. The previous EXE is kept with a .previous extension.")) return;
        await UpdateService.DownloadAndSchedule(pendingRelease, new Progress<string>(x => { UpdateStatus = x; Status = x; }), lifetime.Token);
        IsRestarting = true;
        Application.Current.Shutdown();
    }
    private void ApplyTheme()
    {
        var colors = Dark ? new[] { "#101827", "#192438", "#EDF3FF", "#ACBCD5", "#2C3B53", "#85A5FF", "#335CDB" } : new[] { "#F3F6FB", "#FFFFFF", "#17223B", "#536580", "#DCE4EF", "#335CDB", "#335CDB" };
        string[] keys = ["BackgroundBrush", "CardBrush", "TextBrush", "MutedBrush", "BorderBrush", "AccentBrush", "ButtonBrush"];
        for (int i = 0; i < keys.Length; i++) Application.Current.Resources[keys[i]] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i]));
    }
}
