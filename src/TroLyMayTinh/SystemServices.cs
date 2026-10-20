using System.Diagnostics;
using Microsoft.Win32;
using Toolkit.Core;

namespace TroLyMayTinh;

public sealed record TempEntry(string Path, long Length, DateTime WrittenUtc);
public sealed record CleanupResult(int Deleted, long Bytes, int Skipped);

public static class Cleaner
{
    public static string Root => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp");
    public static List<TempEntry> Scan(CancellationToken token)
    {
        var result = new List<TempEntry>();
        if (!Directory.Exists(Root) || (File.GetAttributes(Root) & FileAttributes.ReparsePoint) != 0) return result;
        var pending = new Stack<string>(); pending.Push(Root);
        var now = DateTime.UtcNow;
        while (pending.Count > 0)
        {
            token.ThrowIfCancellationRequested();
            var folder = pending.Pop();
            try
            {
                if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != 0) continue;
                if (!string.Equals(Path.GetFullPath(folder), Path.GetFullPath(Root), StringComparison.OrdinalIgnoreCase) && CleanupPolicy.HasLinkInPath(Root, folder)) continue;
                foreach (var directory in Directory.GetDirectories(folder))
                    if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) == 0) pending.Push(directory);
                foreach (var file in Directory.GetFiles(folder))
                {
                    token.ThrowIfCancellationRequested();
                    try
                    {
                        var info = new FileInfo(file);
                        if ((info.Attributes & FileAttributes.ReparsePoint) == 0 && CleanupPolicy.Eligible(info.LastWriteTimeUtc, now))
                            result.Add(new(info.FullName, info.Length, info.LastWriteTimeUtc));
                    }
                    catch (IOException) { } catch (UnauthorizedAccessException) { }
                }
            }
            catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        return result.OrderByDescending(x => x.Length).ToList();
    }

    public static CleanupResult Delete(IEnumerable<TempEntry> selected, CancellationToken token)
    {
        int count = 0, skipped = 0; long bytes = 0;
        foreach (var item in selected)
        {
            token.ThrowIfCancellationRequested();
            try
            {
                var info = new FileInfo(item.Path);
                if (!info.Exists || !CleanupPolicy.IsWithin(Root, item.Path) || CleanupPolicy.HasLinkInPath(Root, item.Path) || info.Length != item.Length || info.LastWriteTimeUtc != item.WrittenUtc || !CleanupPolicy.Eligible(info.LastWriteTimeUtc, DateTime.UtcNow))
                { skipped++; continue; }
                File.Delete(item.Path); count++; bytes += item.Length;
            }
            catch (IOException) { skipped++; } catch (UnauthorizedAccessException) { skipped++; }
        }
        return new(count, bytes, skipped);
    }
}

public static class WindowsTools
{
    public static void Open(string command)
    {
        var allowed = new Dictionary<string, string>
        {
            ["taskmgr"] = "Taskmgr.exe", ["diskmgmt"] = "diskmgmt.msc", ["devmgmt"] = "devmgmt.msc", ["services"] = "services.msc",
            ["eventvwr"] = "eventvwr.msc", ["msinfo"] = "msinfo32.exe", ["resmon"] = "resmon.exe", ["control"] = "control.exe",
            ["taskschd"] = "taskschd.msc", ["winver"] = "winver.exe", ["dxdiag"] = "dxdiag.exe", ["cleanmgr"] = "cleanmgr.exe",
            ["optionalfeatures"] = "optionalfeatures.exe", ["mdsched"] = "mdsched.exe", ["certmgr"] = "certmgr.msc",
            ["ncpa"] = "ncpa.cpl", ["power"] = "powercfg.cpl", ["sysdm"] = "sysdm.cpl", ["calc"] = "calc.exe", ["notepad"] = "notepad.exe", ["charmap"] = "charmap.exe", ["osk"] = "osk.exe", ["magnify"] = "magnify.exe"
        };
        if (allowed.TryGetValue(command, out var file))
            AppLog.Open(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), file));
        else if (SettingCatalog.AllowedUris.Contains(command) || command == "ms-screenclip:")
            AppLog.Open(command);
        else throw new InvalidOperationException("Unknown tool.");
        AppLog.Write("Opened tool: " + command);
    }
}

public static class RegistryTweaks
{
    private const string Explorer = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private static readonly string[] Allowed = ["HideFileExt", "Hidden", "ShowSuperHidden"];
    public static int? Read(string name)
    {
        if (!Allowed.Contains(name)) throw new ArgumentException("Unknown setting.");
        using var key = Registry.CurrentUser.OpenSubKey(Explorer);
        return key?.GetValue(name) is int n ? n : null;
    }
    public static void Apply(string name, int value, Preferences preferences)
    {
        if (!Allowed.Contains(name)) throw new ArgumentException("Unknown setting.");
        if (!preferences.RegistryOriginals.ContainsKey(name))
        { preferences.RegistryOriginals[name] = Read(name); preferences.Save(); }
        using var key = Registry.CurrentUser.CreateSubKey(Explorer);
        key.SetValue(name, value, RegistryValueKind.DWord);
        AppLog.Write($"Applied Explorer setting: {name}={value}");
    }
    public static void Restore(Preferences preferences)
    {
        using var key = Registry.CurrentUser.CreateSubKey(Explorer);
        foreach (var entry in preferences.RegistryOriginals.ToArray())
        {
            if (!Allowed.Contains(entry.Key)) continue;
            if (entry.Value is int value) key.SetValue(entry.Key, value, RegistryValueKind.DWord);
            else key.DeleteValue(entry.Key, false);
            preferences.RegistryOriginals.Remove(entry.Key);
            preferences.Save();
        }
        AppLog.Write("Restored Explorer settings.");
    }
}

public static class SoftwareInstaller
{
    public static string WingetPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "winget.exe");
    public static bool Available => File.Exists(WingetPath);

    public static async Task<int> Install(string id, IProgress<string> progress)
    {
        if (!Available) throw new InvalidOperationException("Cần App Installer (WinGet). / App Installer (WinGet) is required.");
        var start = new ProcessStartInfo(WingetPath) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        foreach (var arg in new[] { "install", "--id", id, "--exact", "--source", "winget", "--accept-source-agreements", "--accept-package-agreements", "--disable-interactivity" }) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Cannot start WinGet.");
        async Task Drain(StreamReader reader) { while (await reader.ReadLineAsync() is { } line) if (!string.IsNullOrWhiteSpace(line)) progress.Report(line); }
        await Task.WhenAll(Drain(process.StandardOutput), Drain(process.StandardError), process.WaitForExitAsync());
        return process.ExitCode;
    }

    public static string? FindInstalledVersion(string[] names)
    {
        foreach (var hive in new[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            try
            {
                using var root = RegistryKey.OpenBaseKey(hive, view);
                using var uninstall = root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
                if (uninstall is null) continue;
                foreach (string child in uninstall.GetSubKeyNames())
                {
                    using var key = uninstall.OpenSubKey(child);
                    if (key?.GetValue("DisplayName") is string label && names.Any(n => label.Contains(n, StringComparison.OrdinalIgnoreCase)))
                        return key.GetValue("DisplayVersion") as string ?? "?";
                }
            }
            catch (System.Security.SecurityException) { } catch (UnauthorizedAccessException) { } catch (IOException) { }
        }
        return null;
    }
}
