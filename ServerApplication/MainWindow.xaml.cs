using System.Windows;
using System.Windows.Input;
using ServerApplication.ViewModels;
using ServerApplication.Services;

namespace ServerApplication;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    // DI constructor (for host-resolved instances)
    public MainWindow(object _) : this() { }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            var config = ((App)Application.Current).ConfigService.LoadConfig();
            if (config.IsDemoMode)
            {
                e.Handled = true;
                BtnMainMenu_Click(sender, new RoutedEventArgs());
            }
        }
    }

    private async void BtnMainMenu_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.Cleanup();
        }

        var app = (App)Application.Current;
        
        if (!app.ConfigService.LoadConfig().IsDemoMode)
        {
            await app.StopMasterHostAsync();
        }

        var roleWindow = new ServerApplication.Views.Common.RoleSelectionWindow();
        roleWindow.Show();

        this.Close();
    }
}