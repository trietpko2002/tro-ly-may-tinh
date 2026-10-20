namespace TroLyMayTinh;

public sealed record SettingEntry(string Uri, string NameVi, string NameEn, string GroupVi, string GroupEn);

public static class SettingCatalog
{
    public static IReadOnlyList<SettingEntry> Entries { get; } =
    [
        new("ms-settings:display", "Màn hình", "Display", "Hệ thống", "System"),
        new("ms-settings:sound", "Âm thanh", "Sound", "Hệ thống", "System"),
        new("ms-settings:notifications", "Thông báo", "Notifications", "Hệ thống", "System"),
        new("ms-settings:powersleep", "Nguồn và chế độ ngủ", "Power and sleep", "Hệ thống", "System"),
        new("ms-settings:storagesense", "Dung lượng lưu trữ", "Storage", "Hệ thống", "System"),
        new("ms-settings:clipboard", "Bộ nhớ tạm", "Clipboard", "Hệ thống", "System"),
        new("ms-settings:about", "Thông tin Windows", "About Windows", "Hệ thống", "System"),
        new("ms-settings:recovery", "Khôi phục Windows", "Recovery", "Hệ thống", "System"),
        new("ms-settings:activation", "Kích hoạt Windows", "Activation", "Hệ thống", "System"),
        new("ms-settings:bluetooth", "Bluetooth", "Bluetooth", "Thiết bị", "Devices"),
        new("ms-settings:printers", "Máy in và máy quét", "Printers and scanners", "Thiết bị", "Devices"),
        new("ms-settings:mousetouchpad", "Chuột", "Mouse", "Thiết bị", "Devices"),
        new("ms-settings:devices-touchpad", "Bàn di chuột", "Touchpad", "Thiết bị", "Devices"),
        new("ms-settings:network-status", "Trạng thái mạng", "Network status", "Mạng", "Network"),
        new("ms-settings:network-wifi", "Wi-Fi", "Wi-Fi", "Mạng", "Network"),
        new("ms-settings:network-ethernet", "Ethernet", "Ethernet", "Mạng", "Network"),
        new("ms-settings:network-vpn", "VPN", "VPN", "Mạng", "Network"),
        new("ms-settings:network-proxy", "Proxy", "Proxy", "Mạng", "Network"),
        new("ms-settings:personalization-background", "Hình nền", "Background", "Cá nhân hóa", "Personalization"),
        new("ms-settings:colors", "Màu sắc", "Colors", "Cá nhân hóa", "Personalization"),
        new("ms-settings:themes", "Chủ đề", "Themes", "Cá nhân hóa", "Personalization"),
        new("ms-settings:taskbar", "Thanh tác vụ", "Taskbar", "Cá nhân hóa", "Personalization"),
        new("ms-settings:fonts", "Phông chữ", "Fonts", "Cá nhân hóa", "Personalization"),
        new("ms-settings:appsfeatures", "Ứng dụng đã cài", "Installed apps", "Ứng dụng", "Apps"),
        new("ms-settings:defaultapps", "Ứng dụng mặc định", "Default apps", "Ứng dụng", "Apps"),
        new("ms-settings:startupapps", "Ứng dụng khởi động", "Startup apps", "Ứng dụng", "Apps"),
        new("ms-settings:dateandtime", "Ngày giờ", "Date and time", "Ngôn ngữ", "Language"),
        new("ms-settings:regionlanguage", "Ngôn ngữ và khu vực", "Language and region", "Ngôn ngữ", "Language"),
        new("ms-settings:typing", "Nhập liệu", "Typing", "Ngôn ngữ", "Language"),
        new("ms-settings:windowsupdate", "Windows Update", "Windows Update", "Bảo mật", "Security"),
        new("ms-settings:windowsdefender", "Windows Security", "Windows Security", "Bảo mật", "Security"),
        new("ms-settings:privacy-microphone", "Quyền micro", "Microphone privacy", "Bảo mật", "Security"),
        new("ms-settings:privacy-webcam", "Quyền camera", "Camera privacy", "Bảo mật", "Security"),
        new("ms-settings:signinoptions", "Tùy chọn đăng nhập", "Sign-in options", "Tài khoản", "Accounts"),
        new("ms-settings:yourinfo", "Thông tin tài khoản", "Your info", "Tài khoản", "Accounts"),
        new("ms-settings:easeofaccess-display", "Trợ năng hiển thị", "Accessibility display", "Trợ năng", "Accessibility"),
        new("ms-settings:gaming-gamebar", "Game Bar", "Game Bar", "Trò chơi", "Gaming"),
        new("ms-settings:gaming-gamemode", "Chế độ trò chơi", "Game Mode", "Trò chơi", "Gaming")
    ];
    public static IReadOnlySet<string> AllowedUris { get; } = Entries.Select(x => x.Uri).ToHashSet(StringComparer.Ordinal);
}
