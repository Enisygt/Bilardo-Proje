using System.Windows;
using System.Windows.Controls;
using ServerApplication.ViewModels;
using ServerApplication.Data;
using SharedLibrary.Models;
using SharedLibrary.Enums;
using System;
using System.Linq;

namespace ServerApplication.Views;

public partial class MasaControl : UserControl
{
    public MasaControl()
    {
        InitializeComponent();
    }

    /// <summary>Context menü açıldığında masanın durumuna göre öğeleri aktif/pasif yap</summary>
    private void ContextMenu_Opened(object sender, RoutedEventArgs e)
    {
        if (DataContext is MasaViewModel vm)
        {
            bool bosMu = vm.Durum == MasaDurum.Bos;
            bool aktifMi = vm.Durum == MasaDurum.Aktif || vm.Durum == MasaDurum.SiparisBekliyor;
            bool siparisVar = vm.Durum == MasaDurum.SiparisBekliyor;

            mnuBaslat.IsEnabled = bosMu;
            mnuUrunEkle.IsEnabled = aktifMi;
            mnuSiparisOnayla.IsEnabled = siparisVar;
            mnuHesapKes.IsEnabled = aktifMi;
        }
    }

    private void Menu_ManuelBaslat_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MasaViewModel vm)
        {
            vm.Durum = MasaDurum.Aktif;
            vm.BaslangicZamani = DateTime.Now;
            vm.ToplamBorc = 0;
            
            using var ctx = new AppDbContext();
            var masa = ctx.Masalar.Find(vm.Id);
            if (masa != null)
            {
                masa.Durum = MasaDurum.Aktif;
                masa.BaslangicZamani = DateTime.Now;
                masa.ToplamBorc = 0;
                ctx.SaveChanges();
            }
        }
    }

    private void Menu_ManuelUrunEkle_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MasaViewModel vm)
        {
            var picker = new UrunSecimWindow();
            picker.ShowDialog();

            if (picker.OnaylandiMi && picker.SecilenUrunler.Any())
            {
                using var ctx = new AppDbContext();
                ctx.Database.EnsureCreated();

                foreach (var kvp in picker.SecilenUrunler)
                {
                    var urun = ctx.Urunler.Find(kvp.Key);
                    if (urun != null)
                    {
                        // DB'ye sipariş kaydet
                        ctx.Siparisler.Add(new Siparis
                        {
                            MasaId = vm.Id,
                            UrunId = urun.Id,
                            Adet = kvp.Value,
                            SiparisZamani = DateTime.Now,
                            Durum = SiparisDurum.Hazirlaniyor
                        });

                        // Hesaba ekle
                        vm.ToplamBorc += urun.Fiyat * kvp.Value;
                        // Demo senkronizasyonu için
                        var mainVm = Application.Current.MainWindow?.DataContext as MainViewModel;
                        if (mainVm != null)
                        {
                            mainVm.EkleDemoBorc(vm.Id, (urun.Fiyat * kvp.Value));
                        }
                    }
                }

                // DB masa borcunu güncelle
                var dbMasa = ctx.Masalar.Find(vm.Id);
                if (dbMasa != null)
                {
                    dbMasa.ToplamBorc = vm.ToplamBorc;
                }

                ctx.SaveChanges();
            }
        }
    }

    private void Menu_SiparisOnayla_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MasaViewModel vm && vm.HasPendingOrder)
        {
            var mainWindow = Window.GetWindow(this);
            if (mainWindow?.DataContext is MainViewModel mainVm)
            {
                mainVm.SiparisiOnayla(vm.Id);
            }
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
