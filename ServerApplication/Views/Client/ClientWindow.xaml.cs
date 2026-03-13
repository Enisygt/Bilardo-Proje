using System.Windows;
using ServerApplication.ViewModels;

namespace ServerApplication.Views.Client;

public partial class ClientWindow : Window
{
    public ClientWindow()
    {
        InitializeComponent();
        DataContext = new ClientViewModel();
    }

    private void txtAdminPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClientViewModel vm)
        {
            vm.AdminPassword = txtAdminPassword.Password;
        }
    }
}