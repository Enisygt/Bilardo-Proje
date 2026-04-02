using System.Windows;
using ClientApplication.ViewModels;

namespace ClientApplication;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void txtAdminPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            vm.AdminPassword = txtAdminPassword.Password;
        }
    }
}