using System.Text.Json;

namespace TroLyMayTinh;

public static class SoftwareCatalog
{
    public static IReadOnlyList<SoftwareItem> Load()
    {
        using var stream = typeof(SoftwareCatalog).Assembly.GetManifestResourceStream("TroLyMayTinh.Data.software-catalog.json") ?? throw new InvalidDataException("Software catalog missing.");
        return JsonSerializer.Deserialize<List<SoftwareItem>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }
    public static IReadOnlyList<CategoryOption> Categories(bool english) =>
    [
        new("all", english ? "All categories" : "Tất cả danh mục"),
        new("vietnamese", english ? "Vietnamese input" : "Bộ gõ tiếng Việt"),
        new("browser", english ? "Browsers" : "Trình duyệt"),
        new("office", english ? "Office and documents" : "Văn phòng, tài liệu"),
        new("communication", english ? "Communication" : "Liên lạc"),
        new("media", english ? "Media and graphics" : "Đa phương tiện, đồ họa"),
        new("developer", english ? "Development" : "Lập trình"),
        new("utilities", english ? "Utilities" : "Tiện ích"),
        new("runtime", english ? "Runtimes" : "Thành phần hỗ trợ")
    ];
}
