using System.Windows;
using ServerApplication.ViewModels;

namespace ServerApplication;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private async void BtnMainMenu_Click(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        await app.StopMasterHostAsync();
        
        var roleWindow = new ServerApplication.Views.Common.RoleSelectionWindow();
        roleWindow.Show();
        
        this.Close();
    }
}