using Toolkit.Core;
using System.Text.Json;
using System.Reflection;

int assertions = 0;
void Check(bool condition, string message) { assertions++; if (!condition) throw new Exception(message); }
ReleaseVersion Parse(string value) { Check(ReleaseVersion.TryParse(value, out var parsed), "Parse failed: " + value); return parsed!; }
Check(Parse("1.10.0").CompareTo(Parse("1.9.0")) > 0, "Numeric comparison, not string ordering");
Check(Parse("v1.0.0").CompareTo(Parse("1.0.0+build.99")) == 0, "Ignore build metadata");
Check(Parse("1.0.0").CompareTo(Parse("1.0.0-rc.1")) > 0, "Stable is newer than prerelease");
Check(Parse("1.0.0-rc.10").CompareTo(Parse("1.0.0-rc.2")) > 0, "Numeric prerelease identifiers");
Check(Parse("1.0.0-1").CompareTo(Parse("1.0.0--1")) < 0, "Hyphen identifier is textual");
Check(Parse("9999999999999999999999.0.0").CompareTo(Parse("9.0.0")) > 0, "No numeric overflow");
string[] ordered = ["1.0.0-alpha", "1.0.0-alpha.1", "1.0.0-alpha.beta", "1.0.0-beta", "1.0.0-beta.2", "1.0.0-beta.11", "1.0.0-rc.1", "1.0.0"];
for (int i = 1; i < ordered.Length; i++) Check(Parse(ordered[i - 1]).CompareTo(Parse(ordered[i])) < 0, "SemVer precedence");
foreach (var malformed in new[] { "", "1", "1.2", "01.2.3", "1.0.0-01", "1.0.0-", "1.0.0+", " 1.0.0", "1.0.0\n", "1.0.0.0", "1.2.٣" })
    Check(!ReleaseVersion.TryParse(malformed, out _), "Rejected invalid version: " + malformed);
string root = Path.Combine(Path.GetTempPath(), "toolkit-policy-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    string file = Path.Combine(root, "old.tmp"); File.WriteAllText(file, "test");
    Check(CleanupPolicy.IsWithin(root, file), "Child allowed");
    Check(!CleanupPolicy.IsWithin(root, root + "-other" + Path.DirectorySeparatorChar + "x"), "Prefix collision rejected");
    Check(!CleanupPolicy.IsWithin(root, Path.Combine(root, "..", "outside")), "Traversal rejected");
    Check(!CleanupPolicy.HasLinkInPath(root, file), "Regular file accepted");
    var now = DateTime.UtcNow;
    Check(!CleanupPolicy.Eligible(now.AddDays(-7), now), "Seven-day boundary retained");
    Check(CleanupPolicy.Eligible(now.AddDays(-8), now), "Old files eligible");
    Check(!CleanupPolicy.Eligible(now.AddDays(1), now), "Future timestamps retained");
}
finally { Directory.Delete(root, true); }
using (var catalog = Assembly.GetExecutingAssembly().GetManifestResourceStream("catalog.json"))
using (var json = JsonDocument.Parse(catalog ?? throw new InvalidOperationException("Catalog resource missing.")))
{
    var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    string[] categories = ["vietnamese", "browser", "office", "communication", "media", "developer", "utilities", "runtime"];
    Check(json.RootElement.GetArrayLength() == 36, "Catalog count");
    foreach (var item in json.RootElement.EnumerateArray())
    {
        foreach (var key in new[] { "Name", "Id", "Publisher", "DescriptionVi", "DescriptionEn", "Category" }) Check(!string.IsNullOrWhiteSpace(item.GetProperty(key).GetString()), "Catalog field " + key);
        Check(ids.Add(item.GetProperty("Id").GetString()!), "Unique package ID");
        Check(categories.Contains(item.GetProperty("Category").GetString()), "Valid category");
        Check(item.GetProperty("DetectionNames").GetArrayLength() > 0, "Detection aliases");
    }
}
Console.WriteLine($"PASS: {assertions} assertions.");
