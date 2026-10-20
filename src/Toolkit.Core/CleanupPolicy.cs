namespace Toolkit.Core;

public static class CleanupPolicy
{
    public static bool IsWithin(string root, string file)
    {
        root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
        return Path.GetFullPath(file).StartsWith(root, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    public static bool HasLinkInPath(string root, string file)
    {
        if (!IsWithin(root, file)) return true;
        string? current = Path.GetFullPath(file);
        string stop = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        while (current is not null)
        {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) return true;
            if (string.Equals(Path.TrimEndingDirectorySeparator(current), stop, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) return false;
            current = Path.GetDirectoryName(current);
        }
        return true;
    }

    public static bool Eligible(DateTime writtenUtc, DateTime nowUtc) => writtenUtc < nowUtc.AddDays(-7);
}
