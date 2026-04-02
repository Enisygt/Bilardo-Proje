using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerApplication.Data;
using ServerApplication.Services;
using SharedLibrary.Enums;
using SharedLibrary.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Threading;

namespace ServerApplication.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private DemoService? _demoService;
    private DispatcherTimer? _clockTimer;

    public ObservableCollection<MasaViewModel> Masalar { get; } = new();

    [ObservableProperty]
    private string _saatText = "00:00";

    [ObservableProperty]
    private string _tarihText = "";

    [ObservableProperty]
    private string _salonAdi = "Bilardo Salonu";

    [ObservableProperty]
    private string _telefon = "";

    [ObservableProperty]
    private bool _isDemoMode;

    [ObservableProperty]
    private bool _hasNotification;

    [ObservableProperty]
    private string _kampanyaBannerText = "";

    private decimal _saatlikUcret = 150m;

    public MainViewModel()
    {
        var configService = new ConfigurationService();
        var config = configService.LoadConfig();

        SalonAdi = config.SalonAdi;
        Telefon = config.Telefon;
        IsDemoMode = config.IsDemoMode;

        // Saat timer
        _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clockTimer.Tick += (s, e) => UpdateClock();
        _clockTimer.Start();
        UpdateClock();

        if (IsDemoMode)
        {
            LoadDemoMode();
        }
        else
        {
            LoadMasalar();
        }

        LoadKampanyalar();
    }

    private void UpdateClock()
    {
        SaatText = DateTime.Now.ToString("HH:mm");
        TarihText = DateTime.Now.ToString("dd MMMM yyyy, dddd");

        // Süre + ücret güncellemesi (aktif masalar için)
        foreach (var masa in Masalar)
        {
            if (masa.BaslangicZamani.HasValue && 
                (masa.Durum == MasaDurum.Aktif || masa.Durum == MasaDurum.SiparisBekliyor))
            {
                var diff = DateTime.Now - masa.BaslangicZamani.Value;
                masa.GecenSureText = $"{(int)diff.TotalHours:D2}:{diff.Minutes:D2}";

                // Saatlik ücret üzerinden süre borcunu hesapla
                decimal sureBorcu = (decimal)diff.TotalMinutes * (_saatlikUcret / 60m);
                
                // Sipariş toplamını hesapla (DB'deki siparişler)
                decimal siparisToplam = 0;
                if (!IsDemoMode)
                {
                    try
                    {
                        using var ctx = new AppDbContext();
                        var siparisler = ctx.Siparisler.Where(s => s.MasaId == masa.Id).ToList();
                        foreach (var s in siparisler)
                        {
                            var urun = ctx.Urunler.Find(s.UrunId);
                            if (urun != null) siparisToplam += urun.Fiyat * s.Adet;
                        }
                    }
                    catch { }
                }

                if (!IsDemoMode)
                {
                    masa.ToplamBorc = sureBorcu + siparisToplam;
                }
            }
        }
    }

    private void LoadDemoMode()
    {
        _demoService = new DemoService();

        _demoService.OnMasalarUpdated += () =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RefreshFromDemo();
            });
        };

        _demoService.OnSiparisGeldi += (masaId, siparis) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                HasNotification = true;
                try { SystemSounds.Exclamation.Play(); } catch { }
            });
        };

        RefreshFromDemo();
        _demoService.Start();
    }

    private void RefreshFromDemo()
    {
        if (_demoService == null) return;

        var demoMasalar = _demoService.GetMasalar();
        
        if (Masalar.Count == 0)
        {
            foreach (var m in demoMasalar)
            {
                Masalar.Add(new MasaViewModel(m));
            }
        }
        else
        {
            for (int i = 0; i < demoMasalar.Count && i < Masalar.Count; i++)
            {
                var src = demoMasalar[i];
                var dest = Masalar[i];
                dest.Durum = src.Durum;
                dest.ToplamBorc = src.ToplamBorc;
                dest.BekleyenSiparis = src.BekleyenSiparis;
                dest.PlayerAName = src.PlayerAName;
                dest.PlayerBName = src.PlayerBName;
                dest.BaslangicZamani = src.BaslangicZamani;
            }
        }
    }

    [RelayCommand]
    private void Refresh()
    {
        if (IsDemoMode)
        {
            RefreshFromDemo();
        }
        else
        {
            LoadMasalar();
        }
        LoadKampanyalar();
    }

    private void LoadMasalar()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        var masalarList = context.Masalar.ToList();
        
        if (!masalarList.Any())
        {
            masalarList = context.Masalar.ToList();
        }

        // Saatlik ücret
        var ilkMasa = masalarList.FirstOrDefault();
        if (ilkMasa != null) _saatlikUcret = ilkMasa.SaatlikUcret;

        Masalar.Clear();
        foreach (var m in masalarList)
        {
            Masalar.Add(new MasaViewModel(m));
        }
    }

    private void LoadKampanyalar()
    {
        try
        {
            using var context = new AppDbContext();
            context.Database.EnsureCreated();
            var kampanyalar = context.Kampanyalar.Where(k => k.IsAktif).ToList();
            if (kampanyalar.Any())
            {
                KampanyaBannerText = string.Join("   |   ", 
                    kampanyalar.Select(k => $"{k.Baslik}: {k.Aciklama} — ₺{k.Fiyat:N0}"));
            }
            else
            {
                KampanyaBannerText = "Henüz aktif kampanya yok.";
            }
        }
        catch
        {
            KampanyaBannerText = "🎯 2 Saatlik Oyun: 2 saat + 4 çay — ₺250   |   ☕ Cafe Paketi: 3 Çay + 1 Kahve — ₺70";
        }
    }

    [RelayCommand]
    private void SiparisleriGoster()
    {
        var bekleyenler = Masalar.Where(m => m.HasPendingOrder).ToList();
        var window = new ServerApplication.Views.BildirimlerWindow(bekleyenler);
        window.ShowDialog();
        HasNotification = false;
    }

    [RelayCommand]
    private void MenuGoster()
    {
        var window = new ServerApplication.Views.MenuWindow();
        window.ShowDialog();
    }

    [RelayCommand]
    private void IslemGecmisiGoster()
    {
        var window = new ServerApplication.Views.IslemGecmisiWindow();
        window.ShowDialog();
    }

    [RelayCommand]
    private void AdminPanel()
    {
        var adminWindow = new ServerApplication.Views.Common.AdminPanelWindow();
        adminWindow.ShowDialog();
        // Otomatik yenile
        Refresh();
    }

    [RelayCommand]
    private void Save()
    {
        var adminWindow = new ServerApplication.Views.Common.AdminPanelWindow();
        adminWindow.ShowDialog();
        // Kaydet sonrası otomatik yenile
        Refresh();

        // Config'den güncel bilgileri yükle
        var configService = new ConfigurationService();
        var config = configService.LoadConfig();
        SalonAdi = config.SalonAdi;
        Telefon = config.Telefon;
    }

    public void SiparisiOnayla(int masaId)
    {
        if (IsDemoMode)
        {
            _demoService?.SiparisiOnayla(masaId);
        }
        else
        {
            using var context = new AppDbContext();
            var masa = context.Masalar.Find(masaId);
            if (masa != null)
            {
                masa.Durum = MasaDurum.Aktif;
                masa.BekleyenSiparis = string.Empty;
                context.SaveChanges();
            }
        }

        var vm = Masalar.FirstOrDefault(m => m.Id == masaId);
        if (vm != null)
        {
            vm.Durum = MasaDurum.Aktif;
            vm.BekleyenSiparis = string.Empty;
        }
    }

    public void EkleDemoBorc(int masaId, decimal miktar)
    {
        if (IsDemoMode)
        {
            _demoService?.EkleBorc(masaId, miktar);
        }
    }

    public void Cleanup()
    {
        _clockTimer?.Stop();
        _demoService?.Stop();
        _demoService?.Dispose();
    }
}
