using System.Windows;
using ServerApplication.ViewModels;

namespace ServerApplication.Views;

public partial class CheckoutWindow : Window
{
    public CheckoutWindow(CheckoutViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Payment_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is CheckoutViewModel vm)
        {
            vm.AdminPassword = txtPassword.Password;
        }
    }
}
