using System.Windows;
using System.Windows.Input;
using ServerApplication.ViewModels;

namespace ServerApplication.Views.Client;

public partial class ClientWindow : Window
{
    public ClientWindow()
    {
        InitializeComponent();
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is ClientViewModel vm)
        {
            // B tuşu: Sıra değiştir (sadece maç başladıysa ve admin paneli açık değilse)
            if (e.Key == Key.B && vm.IsMatchStarted && !vm.IsAdminPanelVisible && !vm.IsCafePopupVisible)
            {
                vm.SwitchTurnCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void BtnAdminExit_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClientViewModel vm)
        {
            vm.AdminPassword = txtAdminPassword.Password;
            vm.ExitAppCommand.Execute(null);
            txtAdminPassword.Clear();
        }
    }
}