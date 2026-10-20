using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Toolkit.Core;

namespace TroLyMayTinh;

public sealed record ReleaseInfo(string Version, string Notes, Uri Page, Uri? Download, string? Digest, long Size);

public static class UpdateService
{
    public const string Owner = "trietpko2002";
    public const string Repository = "tro-ly-may-tinh";
    public static string CurrentVersion
    {
        get { var v = typeof(UpdateService).Assembly.GetName().Version!; return $"{v.Major}.{v.Minor}.{v.Build}"; }
    }
    private static readonly HttpClient Client = CreateClient();
    private static HttpClient CreateClient()
    {
        var c = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        c.DefaultRequestHeaders.UserAgent.ParseAdd("TroLyMayTinh/" + CurrentVersion);
        c.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        return c;
    }

    public static async Task<ReleaseInfo?> Check(CancellationToken token)
    {
        ReleaseVersion.TryParse(CurrentVersion, out var current);
        ReleaseInfo? best = null; ReleaseVersion? bestVersion = current;
        string architecture = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "arm64" : "x64";
        // Inspect a bounded release list, compare SemVer instead of dates or string ordering.
        for (int page = 1; page <= 5; page++)
        {
            using var response = await Client.GetAsync($"https://api.github.com/repos/{Owner}/{Repository}/releases?per_page=100&page={page}", token);
            if (response.StatusCode == HttpStatusCode.NotFound) throw new InvalidOperationException("Repository chưa tồn tại, riêng tư hoặc không truy cập được. / Repository not accessible.");
            if (response.StatusCode == HttpStatusCode.Forbidden || (int)response.StatusCode == 429) throw new InvalidOperationException("GitHub giới hạn truy cập. Hãy thử sau. / GitHub rate limit. Try later.");
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(token));
            var entries = document.RootElement;
            foreach (var release in entries.EnumerateArray())
            {
                if (release.GetProperty("draft").GetBoolean() || release.GetProperty("prerelease").GetBoolean()) continue;
                string tag = release.GetProperty("tag_name").GetString() ?? "";
                if (!ReleaseVersion.TryParse(tag, out var version) || version!.IsPrerelease || version.CompareTo(bestVersion) <= 0) continue;
                string expected = $"TroLyMayTinh-win-{architecture}.exe";
                Uri? download = null; string? digest = null; long size = 0;
                foreach (var asset in release.GetProperty("assets").EnumerateArray())
                {
                    if (asset.GetProperty("name").GetString() != expected) continue;
                    string? url = asset.GetProperty("browser_download_url").GetString();
                    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.Host != "github.com" || !uri.AbsolutePath.StartsWith($"/{Owner}/{Repository}/releases/download/", StringComparison.Ordinal)) continue;
                    download = uri;
                    digest = asset.TryGetProperty("digest", out var d) ? d.GetString() : null;
                    size = asset.GetProperty("size").GetInt64();
                }
                bestVersion = version;
                best = new(tag, release.TryGetProperty("body", out var body) ? body.GetString() ?? "" : "", new Uri($"https://github.com/{Owner}/{Repository}/releases"), download, digest, size);
            }
            if (entries.GetArrayLength() < 100) break;
        }
        return best;
    }

    public static bool CanApply(ReleaseInfo release) =>
        release.Download is not null && release.Digest is not null && Regex.IsMatch(release.Digest, "^sha256:[0-9a-fA-F]{64}$") && release.Size is > 0 and < 600_000_000;

    public static async Task DownloadAndSchedule(ReleaseInfo release, IProgress<string> progress, CancellationToken token)
    {
        if (!CanApply(release)) throw new InvalidOperationException("Thiếu gói phù hợp hoặc SHA-256. / Missing compatible package or SHA-256.");
        string target = Environment.ProcessPath ?? throw new InvalidOperationException("Unknown executable path.");
        if (Path.GetFileName(target).Equals("dotnet.exe", StringComparison.OrdinalIgnoreCase) || !string.IsNullOrEmpty(typeof(App).Assembly.Location))
            throw new InvalidOperationException("Chỉ bản portable đóng gói một EXE mới hỗ trợ tự cập nhật. / Self-update requires the single-file portable build.");
        string parent = Path.GetDirectoryName(target)!;
        string probe = Path.Combine(parent, ".write-test-" + Guid.NewGuid());
        try { await File.WriteAllTextAsync(probe, "", token); } finally { if (File.Exists(probe)) File.Delete(probe); }
        string stage = Path.Combine(Preferences.DataFolder, "Updates", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(stage);
        try
        {
        string package = Path.Combine(stage, "new.exe");
        using (var response = await Client.GetAsync(release.Download, HttpCompletionOption.ResponseHeadersRead, token))
        {
            response.EnsureSuccessStatusCode();
            await using var source = await response.Content.ReadAsStreamAsync(token);
            await using var destination = new FileStream(package, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            var buffer = new byte[81920]; long total = 0; int read;
            while ((read = await source.ReadAsync(buffer, token)) != 0)
            {
                total += read;
                if (total > release.Size) throw new InvalidDataException("Package size mismatch.");
                await destination.WriteAsync(buffer.AsMemory(0, read), token);
                progress.Report($"{total / 1048576.0:F1} / {release.Size / 1048576.0:F1} MB");
            }
            if (total != release.Size) throw new InvalidDataException("Incomplete download.");
        }
        await using (var file = File.OpenRead(package))
        {
            string hash = Convert.ToHexString(await SHA256.HashDataAsync(file, token));
            if (!hash.Equals(release.Digest![7..], StringComparison.OrdinalIgnoreCase)) { File.Delete(package); throw new InvalidDataException("SHA-256 không khớp. / SHA-256 mismatch."); }
        }
        using (var binary = new BinaryReader(File.OpenRead(package)))
        {
            if (binary.ReadUInt16() != 0x5A4D) throw new InvalidDataException("Not a Windows executable.");
            binary.BaseStream.Position = 0x3c;
            int offset = binary.ReadInt32();
            if (offset < 64 || offset > binary.BaseStream.Length - 6) throw new InvalidDataException("Invalid PE header.");
            binary.BaseStream.Position = offset;
            if (binary.ReadUInt32() != 0x00004550) throw new InvalidDataException("Invalid PE signature.");
            ushort machine = binary.ReadUInt16();
            ushort expected = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? (ushort)0xAA64 : (ushort)0x8664;
            if (machine != expected) throw new InvalidDataException("Wrong executable architecture.");
        }
        var metadata = FileVersionInfo.GetVersionInfo(package);
        if (metadata.ProductName != "Trợ Lý Máy Tính" || !ReleaseVersion.TryParse(metadata.ProductVersion, out var fileVersion) || !ReleaseVersion.TryParse(release.Version, out var publishedVersion) || fileVersion!.CompareTo(publishedVersion) != 0)
            throw new InvalidDataException("Tên sản phẩm hoặc phiên bản không khớp bản phát hành. / Product or version does not match release.");
        string Quote(string value) => "'" + value.Replace("'", "''") + "'";
        // The helper touches only the current single-file EXE. File.Replace keeps a backup.
        string script = $$"""
        $ErrorActionPreference = 'Stop'
        $target = {{Quote(target)}}
        $package = {{Quote(package)}}
        $backup = $target + '.previous'
        $log = {{Quote(Path.Combine(stage, "updater.log"))}}
        try {
            $old = Get-Process -Id {{Environment.ProcessId}} -ErrorAction SilentlyContinue
            if ($null -ne $old) { if (-not $old.WaitForExit(60000)) { throw 'Application did not exit.' } }
            $actual = (Get-FileHash -LiteralPath $package -Algorithm SHA256).Hash
            if ($actual -ne {{Quote(release.Digest![7..])}}) { throw 'SHA-256 mismatch.' }
            $incoming = $target + '.incoming'
            Copy-Item -LiteralPath $package -Destination $incoming -Force
            [System.IO.File]::Replace($incoming, $target, $backup, $true)
            try { Start-Process -FilePath $target -WorkingDirectory ([System.IO.Path]::GetDirectoryName($target)) -ErrorAction Stop | Out-Null }
            catch { [System.IO.File]::Copy($backup, $target, $true); throw }
            Remove-Item -LiteralPath $package -Force
        } catch {
            $_ | Out-String | Set-Content -LiteralPath $log
            Add-Type -AssemblyName PresentationFramework
            [System.Windows.MessageBox]::Show('Update failed. See: ' + $log, 'Trợ Lý Máy Tính') | Out-Null
        }
        """;
        string helper = Path.Combine(stage, "apply.ps1");
        await File.WriteAllTextAsync(helper, script, new System.Text.UTF8Encoding(true), token);
        var start = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe")) { UseShellExecute = false, CreateNoWindow = true };
        foreach (var arg in new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", helper }) start.ArgumentList.Add(arg);
        using var helperProcess = Process.Start(start) ?? throw new InvalidOperationException("Cannot start update helper.");
        AppLog.Write("Verified update staged: " + release.Version);
        }
        catch
        {
            try { Directory.Delete(stage, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            throw;
        }
    }
}
