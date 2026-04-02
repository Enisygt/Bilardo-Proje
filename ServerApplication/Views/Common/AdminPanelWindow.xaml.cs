using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ServerApplication.Data;
using ServerApplication.Services;
using SharedLibrary.Models;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace ServerApplication.Views.Common;

public partial class AdminPanelWindow : Window
{
    private const string YeniKategoriItem = "➕ Yeni Kategori Oluştur";
    private string _selectedKategori = string.Empty;

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

        // Kategorileri yükle
        LoadKategoriler();
    }

    private void LoadKategoriler()
    {
        cmbUrunKategori.SelectionChanged -= CmbUrunKategori_SelectionChanged;

        var kategoriler = new List<string>();

        // Varsayılan kategoriler
        var defaultKategoriler = new[] { "Sıcak İçecekler", "Soğuk İçecekler", "Atıştırmalık" };
        kategoriler.AddRange(defaultKategoriler);

        // DB'deki ek kategorileri ekle
        try
        {
            using var ctx = new AppDbContext();
            var dbKategoriler = ctx.Urunler
                .Where(u => u.IsAktif)
                .Select(u => u.Kategori)
                .Distinct()
                .ToList();

            foreach (var k in dbKategoriler)
            {
                if (!string.IsNullOrWhiteSpace(k) && !kategoriler.Contains(k))
                {
                    kategoriler.Add(k);
                }
            }
        }
        catch { }

        kategoriler.Sort();
        kategoriler.Add(YeniKategoriItem);

        cmbUrunKategori.Items.Clear();
        foreach (var k in kategoriler)
        {
            cmbUrunKategori.Items.Add(k);
        }

        // İlk kategoriyi seç
        if (cmbUrunKategori.Items.Count > 1)
        {
            cmbUrunKategori.SelectedIndex = 0;
            _selectedKategori = cmbUrunKategori.Items[0]?.ToString() ?? "";
        }

        cmbUrunKategori.SelectionChanged += CmbUrunKategori_SelectionChanged;
    }

    private void CmbUrunKategori_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbUrunKategori.SelectedItem?.ToString() == YeniKategoriItem)
        {
            // Yeni kategori oluşturma dialogu
            var dialog = new Window
            {
                Title = "Yeni Kategori Oluştur",
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A1A2E")),
                ResizeMode = ResizeMode.NoResize
            };

            var panel = new StackPanel { Margin = new Thickness(30), VerticalAlignment = VerticalAlignment.Center };

            var label = new TextBlock
            {
                Text = "Yeni Kategori Adı:",
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 10)
            };
            panel.Children.Add(label);

            var textBox = new TextBox
            {
                FontSize = 16,
                Padding = new Thickness(10, 8, 10, 8),
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2A2A4A")),
                Foreground = System.Windows.Media.Brushes.White,
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#444466")),
                BorderThickness = new Thickness(1)
            };
            panel.Children.Add(textBox);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 15, 0, 0), HorizontalAlignment = HorizontalAlignment.Center };

            var btnOk = new Button
            {
                Content = "✅ Oluştur",
                Width = 120,
                Height = 36,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2E7D32")),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 10, 0)
            };
            btnOk.Click += (s, ev) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    dialog.Tag = textBox.Text.Trim();
                    dialog.DialogResult = true;
                }
            };
            btnPanel.Children.Add(btnOk);

            var btnCancel = new Button
            {
                Content = "İptal",
                Width = 90,
                Height = 36,
                FontSize = 14,
                Foreground = System.Windows.Media.Brushes.White,
                Background = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2A2A4A")),
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            btnCancel.Click += (s, ev) => { dialog.DialogResult = false; };
            btnPanel.Children.Add(btnCancel);

            panel.Children.Add(btnPanel);
            dialog.Content = panel;

            if (dialog.ShowDialog() == true && dialog.Tag is string yeniKategori && !string.IsNullOrWhiteSpace(yeniKategori))
            {
                // Yeni kategoriyi listeye ekle (YeniKategoriItem'dan önce)
                cmbUrunKategori.SelectionChanged -= CmbUrunKategori_SelectionChanged;

                if (!cmbUrunKategori.Items.Contains(yeniKategori))
                {
                    int insertIndex = cmbUrunKategori.Items.Count - 1; // YeniKategoriItem'dan önce
                    cmbUrunKategori.Items.Insert(insertIndex, yeniKategori);
                }
                cmbUrunKategori.SelectedItem = yeniKategori;
                _selectedKategori = yeniKategori;

                cmbUrunKategori.SelectionChanged += CmbUrunKategori_SelectionChanged;
            }
            else
            {
                // İptal edildi, önceki seçime geri dön
                cmbUrunKategori.SelectionChanged -= CmbUrunKategori_SelectionChanged;
                cmbUrunKategori.SelectedItem = _selectedKategori;
                cmbUrunKategori.SelectionChanged += CmbUrunKategori_SelectionChanged;
            }
        }
        else if (cmbUrunKategori.SelectedItem != null)
        {
            _selectedKategori = cmbUrunKategori.SelectedItem.ToString() ?? "";
        }
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

        string kategori = _selectedKategori;
        if (string.IsNullOrWhiteSpace(kategori) || kategori == YeniKategoriItem)
            kategori = "İçecek";

        ctx.Urunler.Add(new Urun
        {
            Ad = txtUrunAd.Text.Trim(),
            Fiyat = fiyat,
            Kategori = kategori,
            IsAktif = true
        });
        ctx.SaveChanges();

        lstUrunler.ItemsSource = ctx.Urunler.ToList();
        txtUrunAd.Clear();
        txtUrunFiyat.Clear();
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
            // Kategorileri yeniden yükle
            LoadKategoriler();
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
