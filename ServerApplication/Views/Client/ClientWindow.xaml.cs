using System.Windows;
using ServerApplication.ViewModels;

namespace ServerApplication.Views.Client;

public partial class ClientWindow : Window
{
    public ClientWindow()
    {
        InitializeComponent();
        var vm = new ClientViewModel();
        DataContext = vm;
        vm.OnKampanyaChanged += AnimateKampanya;
    }

    private void AnimateKampanya()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var slideOut = new System.Windows.Media.Animation.DoubleAnimation(-400, System.TimeSpan.FromSeconds(0.4));
            slideOut.Completed += (s, e) =>
            {
                var slideIn = new System.Windows.Media.Animation.DoubleAnimation(400, 0, System.TimeSpan.FromSeconds(0.5));
                slideIn.EasingFunction = new System.Windows.Media.Animation.QuadraticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };
                KampanyaTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideIn);
            };
            KampanyaTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideOut);
        });
    }

    private void txtAdminPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ClientViewModel vm)
        {
            vm.AdminPassword = txtAdminPassword.Password;
        }
    }
}