using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace TroLyMayTinh;

public abstract class Observable : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void Changed([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));
    protected bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    { if (EqualityComparer<T>.Default.Equals(field, value)) return false; field = value; Changed(name); return true; }
}

public sealed class ActionCommand(Action action) : ICommand
{
    public event EventHandler? CanExecuteChanged { add { } remove { } }
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => action();
}

public sealed class AsyncCommand(Func<Task> action) : ICommand
{
    private bool running;
    public event EventHandler? CanExecuteChanged;
    public bool CanExecute(object? parameter) => !running;
    public async void Execute(object? parameter)
    {
        if (running) return;
        running = true; CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try { await action(); }
        catch (Exception ex) { AppLog.Write("Command failed", ex); System.Windows.MessageBox.Show(ex.Message, "Trợ Lý Máy Tính", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error); }
        finally { running = false; CanExecuteChanged?.Invoke(this, EventArgs.Empty); }
    }
}

public sealed class Preferences
{
    public string Language { get; set; } = "vi";
    public bool Dark { get; set; }
    public bool CheckUpdates { get; set; } = true;
    public Dictionary<string, int?> RegistryOriginals { get; set; } = [];
    public HashSet<string> FavoriteTools { get; set; } = [];
    public static string DataFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TroLyMayTinh");
    private static string FilePath => Path.Combine(DataFolder, "settings.json");
    public static Preferences Load()
    {
        try
        {
            Preferences settings = File.Exists(FilePath) ? JsonSerializer.Deserialize<Preferences>(File.ReadAllText(FilePath)) ?? new Preferences() : new Preferences();
            settings.RegistryOriginals ??= [];
            settings.FavoriteTools ??= [];
            return settings;
        }
        catch (Exception ex) { AppLog.Write("Settings reset after read failure", ex); return new(); }
    }
    public void Save()
    {
        Directory.CreateDirectory(DataFolder);
        string temporary = FilePath + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporary, FilePath, true);
    }
}

public static class AppLog
{
    public static event Action<string>? Written;
    public static string LogPath => Path.Combine(Preferences.DataFolder, "activity.log");
    private static readonly object Gate = new();
    public static void Write(string message, Exception? error = null)
    {
        string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}" + (error is null ? "" : $" | {error.GetType().Name}: {error.Message}");
        try { lock (Gate) { Directory.CreateDirectory(Preferences.DataFolder); if (File.Exists(LogPath) && new FileInfo(LogPath).Length > 2_000_000) File.Move(LogPath, LogPath + ".old", true); File.AppendAllText(LogPath, line + Environment.NewLine); } } catch { /* Logging must not terminate the app. */ }
        Written?.Invoke(line);
    }
    public static void Open(string target)
    { Process.Start(new ProcessStartInfo(target) { UseShellExecute = true }); }
}
