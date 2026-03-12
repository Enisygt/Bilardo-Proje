using System.Windows;
using System.Windows.Controls;
using ServerApplication.ViewModels;
using ServerApplication.Data;
using SharedLibrary.Models;
using SharedLibrary.Enums;

namespace ServerApplication.Views;

public partial class MasaControl : UserControl
{
    public MasaControl()
    {
        InitializeComponent();
    }

    private void Menu_ManuelBaslat_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MasaViewModel vm)
        {
            vm.Durum = MasaDurum.Aktif;
            using var ctx = new AppDbContext();
            var masa = ctx.Masalar.Find(vm.Id);
            if (masa != null)
            {
                masa.Durum = MasaDurum.Aktif;
                ctx.SaveChanges();
            }
        }
    }

    private void Menu_ManuelUrunEkle_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MasaViewModel vm)
        {
            vm.ToplamBorc += 50.0m; // Example fixed price
            using var ctx = new AppDbContext();
            
            var siparis = new Siparis 
            { 
                MasaId = vm.Id, 
                UrunId = 1, 
                Adet = 1, 
                SiparisZamani = System.DateTime.Now,
                Durum = SiparisDurum.Hazirlaniyor
            };
            ctx.Siparisler.Add(siparis);
            ctx.SaveChanges();
        }
    }

    private void Menu_ManuelBitir_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MasaViewModel masaVm)
        {
            var checkoutVm = new CheckoutViewModel(masaVm);
            var checkoutWindow = new CheckoutWindow(checkoutVm);
            checkoutWindow.ShowDialog();
        }
    }
}
