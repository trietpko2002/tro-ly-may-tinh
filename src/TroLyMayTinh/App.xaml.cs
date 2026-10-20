using System.Windows;

namespace TroLyMayTinh;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            AppLog.Write("Unhandled UI exception", args.Exception);
            MessageBox.Show("Không thể hoàn tất thao tác / Operation failed.\n" + args.Exception.Message, "Trợ Lý Máy Tính", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };
        bool smoke = e.Args.Contains("--smoke-test");
        var errors = new BindingErrors();
        if (smoke) { System.Diagnostics.PresentationTraceSources.DataBindingSource.Listeners.Add(errors); System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Error; }
        var window = new MainWindow();
        if (smoke) window.Loaded += async (_, _) => await SmokeChecks.Run(window, errors);
        window.Show();
    }
}
