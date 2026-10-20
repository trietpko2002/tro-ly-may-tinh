using System.Diagnostics;
using System.Text;

namespace TroLyMayTinh;

public sealed record ProcessResult(int Code, string Output, string Error);

public static class ProcessRunner
{
    public static string PowerShell => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe");
    public static Task<ProcessResult> Script(string script, CancellationToken token) => Run(PowerShell,
        ["-NoProfile", "-NonInteractive", "-EncodedCommand", Convert.ToBase64String(Encoding.Unicode.GetBytes("[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false); " + script))], token);

    public static async Task<ProcessResult> Run(string executable, IEnumerable<string> arguments, CancellationToken token)
    {
        var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true, StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8 };
        foreach (var arg in arguments) start.ArgumentList.Add(arg);
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Cannot start process.");
        using var registration = token.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { } });
        Task<string> output = process.StandardOutput.ReadToEndAsync(token), error = process.StandardError.ReadToEndAsync(token);
        await process.WaitForExitAsync(token);
        return new(process.ExitCode, await output, await error);
    }
}
