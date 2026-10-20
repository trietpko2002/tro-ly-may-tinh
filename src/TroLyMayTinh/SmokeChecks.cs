using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TroLyMayTinh;

public sealed class BindingErrors : TraceListener
{
    public List<string> Errors { get; } = [];
    public override void Write(string? message) { if (message is not null) Errors.Add(message); }
    public override void WriteLine(string? message) { if (message is not null) Errors.Add(message); }
}

public static class SmokeChecks
{
    public static async Task Run(MainWindow window, BindingErrors errors)
    {
        string output = Path.GetFullPath("smoke-results.txt");
        int code = 0;
        try
        {
            var vm = (MainViewModel)window.DataContext;
            bool english = vm.English, dark = vm.Dark;
            try
            {
                foreach (bool lang in new[] { false, true })
                foreach (bool theme in new[] { false, true })
                {
                    vm.English = lang; vm.Dark = theme;
                    for (int page = 0; page < vm.Navigation.Count; page++)
                    {
                        vm.Page = page;
                        await window.Dispatcher.InvokeAsync(() => window.UpdateLayout(), DispatcherPriority.ContextIdle);
                        CheckTextInheritance(window);
                        if (!lang && page is 0 or 1 or 9 or 10) SaveImage(window, $"page-{page}-{(theme ? "dark" : "light")}.png");
                    }
                }
                if (errors.Errors.Count != 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors.Errors));
                if (vm.Software.Count != 36 || vm.Tools.Count != 21 || vm.WindowsSettings.Count != 38 || vm.Utilities.Count != 12) throw new InvalidOperationException("Missing catalog entries.");
                vm.Category = "browser";
                if (vm.SoftwareView.Cast<SoftwareItem>().Count() != 4) throw new InvalidOperationException("Category filter failed.");
                vm.Category = "all"; vm.Search = "__no_such_software__";
                if (!vm.NoApps) throw new InvalidOperationException("Software search failed.");
                vm.Search = ""; vm.QuickSearch = "__no_such_tool__";
                if (!vm.NoTools || !vm.NoSettings || !vm.NoUtilities) throw new InvalidOperationException("Shortcut search failed.");
                vm.QuickSearch = "";
                vm.OfficePresetCommand.Execute(null);
                if (vm.Software.Count(x => x.Selected) != 5) throw new InvalidOperationException("Office preset failed.");
                vm.ClearSoftwareCommand.Execute(null);
                if (vm.CleanupFiles.Count != 0) throw new InvalidOperationException("Smoke test must not modify user files.");
                await File.WriteAllTextAsync(output, "PASS: 12 pages, 2 languages, 2 themes; button/navigation text inheritance; catalogs and search/presets; no WPF binding errors. No maintenance or installation commands invoked. Screenshots in artifacts/ui-checks.");
            }
            finally { vm.English = english; vm.Dark = dark; }
        }
        catch (Exception ex) { code = 1; await File.WriteAllTextAsync(output, "FAIL: " + ex); }
        Application.Current.Shutdown(code);
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) yield return match;
            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }
    private static void CheckTextInheritance(MainWindow window)
    {
        foreach (var button in Descendants<Button>(window).Where(x => x.IsVisible && x.IsEnabled && x.Content is string))
        foreach (var text in Descendants<TextBlock>(button))
            if (text.Foreground is SolidColorBrush actual && button.Foreground is SolidColorBrush expected && actual.Color != expected.Color)
                throw new InvalidOperationException("Button text color does not inherit control foreground: " + button.Content);
        foreach (var item in Descendants<ListBoxItem>(window).Where(x => x.Style == window.FindResource("NavItem")))
        foreach (var text in Descendants<TextBlock>(item))
            if (text.Foreground is SolidColorBrush actual && item.Foreground is SolidColorBrush expected && actual.Color != expected.Color)
                throw new InvalidOperationException("Navigation text color does not inherit foreground.");
    }
    private static void SaveImage(MainWindow window, string name)
    {
        string directory = Path.GetFullPath("artifacts/ui-checks"); Directory.CreateDirectory(directory);
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(window.ActualWidth), (int)Math.Ceiling(window.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file = File.Create(Path.Combine(directory, name)); encoder.Save(file);
    }
}
