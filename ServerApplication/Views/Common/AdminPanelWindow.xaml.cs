using System.Linq;
using System.Windows;
using ServerApplication.Data;
using ServerApplication.Services;
using SharedLibrary.Models;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace ServerApplication.Views.Common;

public partial class AdminPanelWindow : Window
{
    public AdminPanelWindow()
    {
        InitializeComponent();
    }

    private void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        if (txtAdminPass.Password == "admin123")
        {
            pnlPassword.Visibility = Visibility.Collapsed;
            tabPanel.Visibility = Visibility.Visible;
            LoadData();
        }
        else
        {
            MessageBox.Show("Yanlış şifre!", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void LoadData()
    {
        using var ctx = new AppDbContext();
        ctx.Database.EnsureCreated();

        lstKampanyalar.ItemsSource = ctx.Kampanyalar.ToList();
        lstUrunler.ItemsSource = ctx.Urunler.ToList();

        var configService = new ConfigurationService();
        var config = configService.LoadConfig();
        txtSalonAdi.Text = config.SalonAdi;
        txtTelefon.Text = config.Telefon;
        txtAdres.Text = config.Adres;

        // Saatlik ücret ilk masadan al
        var ilkMasa = ctx.Masalar.FirstOrDefault();
        txtSaatlikUcret.Text = ilkMasa?.SaatlikUcret.ToString("N0") ?? "150";
    }

    // ==================== KAMPANYALAR ====================
    private void BtnKampanyaEkle_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtKampanyaBaslik.Text)) return;

        using var ctx = new AppDbContext();
        decimal.TryParse(txtKampanyaFiyat.Text, out decimal fiyat);

        ctx.Kampanyalar.Add(new Kampanya
        {
            Baslik = txtKampanyaBaslik.Text.Trim(),
            Aciklama = txtKampanyaAciklama.Text.Trim(),
            Fiyat = fiyat,
            IsAktif = true
        });
        ctx.SaveChanges();

        lstKampanyalar.ItemsSource = ctx.Kampanyalar.ToList();
        txtKampanyaBaslik.Clear();
        txtKampanyaAciklama.Clear();
        txtKampanyaFiyat.Clear();
        ShowNotification();
    }

    private void BtnKampanyaSil_Click(object sender, RoutedEventArgs e)
    {
        if (lstKampanyalar.SelectedItem is Kampanya secili)
        {
            using var ctx = new AppDbContext();
            var entity = ctx.Kampanyalar.Find(secili.Id);
            if (entity != null)
            {
                ctx.Kampanyalar.Remove(entity);
                ctx.SaveChanges();
            }
            lstKampanyalar.ItemsSource = ctx.Kampanyalar.ToList();
            ShowNotification();
        }
    }

    // ==================== ÜRÜNLER ====================
    private void BtnUrunEkle_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtUrunAd.Text)) return;

        using var ctx = new AppDbContext();
        decimal.TryParse(txtUrunFiyat.Text, out decimal fiyat);

        ctx.Urunler.Add(new Urun
        {
            Ad = txtUrunAd.Text.Trim(),
            Fiyat = fiyat,
            Kategori = string.IsNullOrWhiteSpace(txtUrunKategori.Text) ? "İçecek" : txtUrunKategori.Text.Trim(),
            IsAktif = true
        });
        ctx.SaveChanges();

        lstUrunler.ItemsSource = ctx.Urunler.ToList();
        txtUrunAd.Clear();
        txtUrunFiyat.Clear();
        txtUrunKategori.Clear();
        ShowNotification();
    }

    private void BtnUrunSil_Click(object sender, RoutedEventArgs e)
    {
        if (lstUrunler.SelectedItem is Urun secili)
        {
            using var ctx = new AppDbContext();
            var entity = ctx.Urunler.Find(secili.Id);
            if (entity != null)
            {
                ctx.Urunler.Remove(entity);
                ctx.SaveChanges();
            }
            lstUrunler.ItemsSource = ctx.Urunler.ToList();
            ShowNotification();
        }
    }

    // ==================== SALON AYARLARI ====================
    private void BtnAyarKaydet_Click(object sender, RoutedEventArgs e)
    {
        var configService = new ConfigurationService();
        var config = configService.LoadConfig();

        config.SalonAdi = txtSalonAdi.Text.Trim();
        config.Telefon = txtTelefon.Text.Trim();
        config.Adres = txtAdres.Text.Trim();
        configService.SaveConfig(config);

        // Saatlik ücret güncelle
        if (decimal.TryParse(txtSaatlikUcret.Text, out decimal ucret))
        {
            using var ctx = new AppDbContext();
            foreach (var masa in ctx.Masalar)
            {
                masa.SaatlikUcret = ucret;
            }
            ctx.SaveChanges();
        }

        ShowNotification();
    }

    private void ShowNotification()
    {
        NotificationBorder.Visibility = Visibility.Visible;
        var fadeIn = new DoubleAnimation(0, 1, System.TimeSpan.FromSeconds(0.3));
        NotificationBorder.BeginAnimation(UIElement.OpacityProperty, fadeIn);

        Task.Delay(3500).ContinueWith(_ =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var fadeOut = new DoubleAnimation(1, 0, System.TimeSpan.FromSeconds(0.4));
                fadeOut.Completed += (s, ev) => NotificationBorder.Visibility = Visibility.Collapsed;
                NotificationBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            });
        });
    }
}
