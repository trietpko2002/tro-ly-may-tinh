using System.ComponentModel;
using System.Windows;

namespace TroLyMayTinh;

public partial class MainWindow : Window
{
    private readonly MainViewModel model;
    public MainWindow()
    {
        InitializeComponent();
        model = new MainViewModel();
        DataContext = model;
        Loaded += async (_, _) => { if (!Environment.GetCommandLineArgs().Contains("--smoke-test")) await model.Startup(); };
        Closed += (_, _) => model.Dispose();
    }
    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (model.Busy && !model.IsRestarting)
        {
            MessageBox.Show(model.English ? "A task is running. Wait for it to finish, or cancel cleanup first." : "Có tác vụ đang chạy. Hãy đợi hoàn tất hoặc dừng quét/dọn dẹp trước khi đóng.", "Trợ Lý Máy Tính", MessageBoxButton.OK, MessageBoxImage.Information);
            e.Cancel = true;
        }
    }
}
