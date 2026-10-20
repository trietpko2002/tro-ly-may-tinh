using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace TroLyMayTinh;

public sealed record HardwareSnapshot(string Json, string Network, string Volumes, DateTime CollectedAt, string? Error);

public static class HardwareService
{
    private const string Script = """
    $ErrorActionPreference = 'Stop'
    $failures = @()
    function Read-Class([string]$class) {
      try { @(Get-CimInstance -ClassName $class -OperationTimeoutSec 4 -ErrorAction Stop) }
      catch { $script:failures += $class + ': ' + $_.Exception.Message; @() }
    }
    $cpu = @(Read-Class 'Win32_Processor' | Select-Object Name,Manufacturer,NumberOfCores,NumberOfLogicalProcessors,MaxClockSpeed,SocketDesignation)
    $computer = @(Read-Class 'Win32_ComputerSystem' | Select-Object Manufacturer,Model,TotalPhysicalMemory,SystemType)
    $memory = @(Read-Class 'Win32_PhysicalMemory' | Select-Object DeviceLocator,Capacity,Speed,ConfiguredClockSpeed,Manufacturer,PartNumber)
    $gpu = @(Read-Class 'Win32_VideoController' | Select-Object Name,DriverVersion,CurrentHorizontalResolution,CurrentVerticalResolution,VideoProcessor)
    $board = @(Read-Class 'Win32_BaseBoard' | Select-Object Manufacturer,Product,Version)
    $bios = @(Read-Class 'Win32_BIOS' | Select-Object Manufacturer,SMBIOSBIOSVersion,@{n='ReleaseDate';e={if ($_.ReleaseDate) { $_.ReleaseDate.ToString('yyyy-MM-dd') }}})
    $disk = @(Read-Class 'Win32_DiskDrive' | Select-Object Model,InterfaceType,MediaType,Size,Status)
    $os = @(Read-Class 'Win32_OperatingSystem' | Select-Object Caption,Version,BuildNumber,OSArchitecture,TotalVisibleMemorySize,FreePhysicalMemory,@{n='LastBoot';e={if ($_.LastBootUpTime) { $_.LastBootUpTime.ToString('yyyy-MM-dd HH:mm:ss') }}})
    [pscustomobject]@{cpu=$cpu;computer=$computer;memory=$memory;gpu=$gpu;board=$board;bios=$bios;disk=$disk;os=$os;errors=$failures} | ConvertTo-Json -Depth 6 -Compress
    """;

    public static async Task<HardwareSnapshot> Collect(CancellationToken token)
    {
        string network = GetNetwork(), volumes = GetVolumes();
        try
        {
            var result = await ProcessRunner.Script(Script, token);
            if (result.Code != 0) throw new InvalidOperationException(result.Error.Length == 0 ? "CIM query failed." : result.Error);
            using var doc = JsonDocument.Parse(result.Output);
            return new(result.Output, network, volumes, DateTime.Now, null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        { return new("{}", network, volumes, DateTime.Now, ex.Message); }
    }

    public static string GetNetwork()
    {
        var lines = new List<string>();
        try
        {
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces().Where(x => x.OperationalStatus == OperationalStatus.Up && x.NetworkInterfaceType != NetworkInterfaceType.Loopback))
            {
                try
                {
                    var ip = adapter.GetIPProperties();
                    lines.Add($"{adapter.Name} · {adapter.Description}\n{adapter.NetworkInterfaceType} · {adapter.Speed / 1_000_000.0:F0} Mbps\nIP: {string.Join(", ", ip.UnicastAddresses.Select(x => x.Address))}\nGateway: {string.Join(", ", ip.GatewayAddresses.Select(x => x.Address))}\nDNS: {string.Join(", ", ip.DnsAddresses)}");
                }
                catch (NetworkInformationException ex) { lines.Add(adapter.Name + ": " + ex.Message); }
            }
        }
        catch (Exception ex) { return ex.Message; }
        return lines.Count == 0 ? "Không có kết nối đang hoạt động / No active connection" : string.Join("\n\n", lines);
    }

    private static string GetVolumes()
    {
        var lines = new List<string>();
        foreach (var drive in DriveInfo.GetDrives())
        {
            try { if (drive.IsReady) lines.Add($"{drive.Name} {drive.VolumeLabel} · {drive.DriveFormat} · {drive.DriveType}\nFree / Trống: {MainViewModel.FormatBytes(drive.AvailableFreeSpace)} / {MainViewModel.FormatBytes(drive.TotalSize)}"); }
            catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        return string.Join("\n\n", lines);
    }

    public static IReadOnlyList<HardwareGroup> Present(HardwareSnapshot snapshot, bool english)
    {
        string T(string vi, string en) => english ? en : vi;
        string unknown = T("Không đọc được / không có dữ liệu", "Unavailable / no data");
        using var document = JsonDocument.Parse(snapshot.Json);
        var groups = new List<HardwareGroup>();
        var maps = new (string Id, string Title, (string Key, string Label, string Unit)[] Fields)[]
        {
            ("computer", T("Máy tính", "Computer"), [("Manufacturer",T("Hãng", "Manufacturer"),""),("Model",T("Dòng máy", "Model"),""),("TotalPhysicalMemory",T("RAM lắp đặt", "Installed RAM"),"bytes"),("SystemType",T("Loại hệ thống", "System type"),"")]),
            ("cpu", "CPU", [("Name",T("Tên", "Name"),""),("Manufacturer",T("Hãng", "Manufacturer"),""),("NumberOfCores",T("Nhân vật lý", "Cores"),""),("NumberOfLogicalProcessors",T("Luồng", "Logical processors"),""),("MaxClockSpeed",T("Xung nhịp tối đa khai báo", "Reported max clock")," MHz"),("SocketDesignation","Socket","")]),
            ("os", "Windows", [("Caption",T("Phiên bản", "Edition"),""),("Version","Version",""),("BuildNumber","Build",""),("OSArchitecture",T("Kiến trúc hệ điều hành", "OS architecture"),""),("TotalVisibleMemorySize",T("RAM Windows sử dụng được", "Usable RAM"),"kb"),("FreePhysicalMemory",T("RAM khả dụng lúc quét", "Available RAM at scan"),"kb"),("LastBoot",T("Khởi động gần nhất", "Last boot"),"")]),
            ("memory", T("Thanh RAM", "Memory module"), [("DeviceLocator",T("Khe", "Slot"),""),("Capacity",T("Dung lượng", "Capacity"),"bytes"),("Speed",T("Tốc độ khai báo", "Reported speed")," (SMBIOS)"),("ConfiguredClockSpeed",T("Tốc độ cấu hình", "Configured speed")," (SMBIOS)"),("Manufacturer",T("Hãng", "Manufacturer"),""),("PartNumber",T("Mã linh kiện", "Part number"),"")]),
            ("gpu", "GPU", [("Name",T("Tên", "Name"),""),("VideoProcessor",T("Bộ xử lý", "Processor"),""),("DriverVersion",T("Phiên bản driver", "Driver version"),""),("CurrentHorizontalResolution",T("Ngang", "Width")," px"),("CurrentVerticalResolution",T("Dọc", "Height")," px")]),
            ("board", T("Bo mạch chủ", "Motherboard"), [("Manufacturer",T("Hãng", "Manufacturer"),""),("Product",T("Sản phẩm", "Product"),""),("Version",T("Phiên bản", "Version"),"")]),
            ("bios", "BIOS / UEFI", [("Manufacturer",T("Hãng", "Manufacturer"),""),("SMBIOSBIOSVersion",T("Phiên bản BIOS", "BIOS version"),""),("ReleaseDate",T("Ngày phát hành", "Release date"),"")]),
            ("disk", T("Ổ đĩa vật lý", "Physical disk"), [("Model",T("Tên", "Model"),""),("InterfaceType",T("Giao tiếp khai báo", "Reported interface"),""),("MediaType",T("Loại khai báo", "Reported media"),""),("Size",T("Dung lượng", "Capacity"),"bytes"),("Status",T("Trạng thái WMI (không phải SMART)", "WMI status (not SMART)"),"")])
        };
        foreach (var section in maps)
        {
            if (!document.RootElement.TryGetProperty(section.Id, out var array) || array.ValueKind != JsonValueKind.Array || array.GetArrayLength() == 0)
            { groups.Add(new(section.Title, [new("", unknown)])); continue; }
            int index = 0;
            foreach (var entry in array.EnumerateArray())
            {
                var fields = new List<HardwareField>();
                foreach (var field in section.Fields)
                {
                    string value = entry.TryGetProperty(field.Key, out var v) && v.ValueKind != JsonValueKind.Null ? v.ToString().Trim() : "";
                    if (string.IsNullOrWhiteSpace(value)) value = unknown;
                    else if ((field.Unit is "bytes" or "kb") && long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out long n)) value = MainViewModel.FormatBytes(field.Unit == "kb" ? n * 1024 : n);
                    else value += field.Unit;
                    fields.Add(new(field.Label, value));
                }
                groups.Add(new(section.Title + (array.GetArrayLength() > 1 ? $" #{++index}" : ""), fields));
            }
        }
        groups.Add(new(T("Phân vùng / ổ đĩa logic", "Volumes"), [new("", snapshot.Volumes.Length == 0 ? unknown : snapshot.Volumes)]));
        groups.Add(new(T("Mạng đang kết nối", "Active network"), [new("", snapshot.Network)]));
        if (snapshot.Error is not null) groups.Add(new(T("Lỗi truy vấn", "Query error"), [new("", snapshot.Error)]));
        if (document.RootElement.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Array && errors.GetArrayLength() != 0)
            groups.Add(new(T("Nguồn dữ liệu chưa đọc được", "Unavailable data sources"), [new("", string.Join("\n", errors.EnumerateArray().Select(x => x.GetString())))]));
        return groups;
    }
}
