using System.Collections.Generic;
using System.Windows;
using ServerApplication.ViewModels;

namespace ServerApplication.Views;

public partial class BildirimlerWindow : Window
{
    public BildirimlerWindow(List<MasaViewModel> bekleyenler)
    {
        InitializeComponent();

        if (bekleyenler.Count > 0)
        {
            lstBildirimler.ItemsSource = bekleyenler;
            lstBildirimler.Visibility = Visibility.Visible;
            txtBos.Visibility = Visibility.Collapsed;
        }
        else
        {
            lstBildirimler.Visibility = Visibility.Collapsed;
            txtBos.Visibility = Visibility.Visible;
        }
    }

    private void BtnKapat_Click(object sender, RoutedEventArgs e) => Close();
}
