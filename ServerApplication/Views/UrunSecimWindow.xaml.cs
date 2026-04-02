using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ServerApplication.Data;
using SharedLibrary.Models;

namespace ServerApplication.Views;

public partial class UrunSecimWindow : Window
{
    private readonly Dictionary<int, int> _secimler = new(); // UrunId → Adet
    private List<Urun> _urunler = new();

    /// <summary>Seçilen ürünler (UrunId, Adet)</summary>
    public Dictionary<int, int> SecilenUrunler => _secimler;
    public bool OnaylandiMi { get; private set; }

    public UrunSecimWindow()
    {
        InitializeComponent();
        LoadUrunler();
    }

    private void LoadUrunler()
    {
        try
        {
            using var ctx = new AppDbContext();
            ctx.Database.EnsureCreated();
            _urunler = ctx.Urunler.Where(u => u.IsAktif).OrderBy(u => u.Kategori).ThenBy(u => u.Ad).ToList();
        }
        catch
        {
            _urunler = new List<Urun>();
        }

        string? currentKategori = null;

        foreach (var urun in _urunler)
        {
            // Kategori başlığı
            if (urun.Kategori != currentKategori)
            {
                currentKategori = urun.Kategori;
                pnlUrunler.Children.Add(new TextBlock
                {
                    Text = $"— {currentKategori} —",
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")),
                    Margin = new Thickness(0, 10, 0, 4)
                });
            }

            // Ürün satırı
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.Margin = new Thickness(0, 2, 0, 2);

            // Ürün adı + fiyat
            var txtAd = new TextBlock
            {
                Text = $"{urun.Ad}  —  ₺{urun.Fiyat:N0}",
                FontSize = 14,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtAd, 0);
            grid.Children.Add(txtAd);

            // Eksi butonu
            var btnMinus = new Button
            {
                Content = "−",
                Width = 32, Height = 32,
                FontSize = 18, FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A1A1A")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = urun.Id
            };
            btnMinus.Click += BtnMinus_Click;
            Grid.SetColumn(btnMinus, 1);
            grid.Children.Add(btnMinus);

            // Adet
            var txtAdet = new TextBlock
            {
                Text = "0",
                FontSize = 16, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 36,
                TextAlignment = TextAlignment.Center,
                Tag = urun.Id
            };
            Grid.SetColumn(txtAdet, 2);
            grid.Children.Add(txtAdet);

            // Artı butonu
            var btnPlus = new Button
            {
                Content = "+",
                Width = 32, Height = 32,
                FontSize = 18, FontWeight = FontWeights.Bold,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B3A1B")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = urun.Id
            };
            btnPlus.Click += BtnPlus_Click;
            Grid.SetColumn(btnPlus, 3);
            grid.Children.Add(btnPlus);

            pnlUrunler.Children.Add(grid);
        }
    }

    private void BtnPlus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int urunId)
        {
            _secimler.TryGetValue(urunId, out int mevcut);
            _secimler[urunId] = mevcut + 1;
            UpdateAdetText(urunId);
            UpdateOzet();
        }
    }

    private void BtnMinus_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is int urunId)
        {
            _secimler.TryGetValue(urunId, out int mevcut);
            if (mevcut > 0)
            {
                _secimler[urunId] = mevcut - 1;
                if (_secimler[urunId] == 0) _secimler.Remove(urunId);
                UpdateAdetText(urunId);
                UpdateOzet();
            }
        }
    }

    private void UpdateAdetText(int urunId)
    {
        _secimler.TryGetValue(urunId, out int adet);
        foreach (var child in pnlUrunler.Children)
        {
            if (child is Grid grid)
            {
                foreach (var gridChild in grid.Children)
                {
                    if (gridChild is TextBlock tb && tb.Tag is int id && id == urunId)
                    {
                        tb.Text = adet.ToString();
                    }
                }
            }
        }
    }

    private void UpdateOzet()
    {
        if (!_secimler.Any())
        {
            txtSecimOzet.Text = "";
            return;
        }

        var parts = new List<string>();
        decimal toplam = 0;
        foreach (var kvp in _secimler)
        {
            var urun = _urunler.FirstOrDefault(u => u.Id == kvp.Key);
            if (urun != null)
            {
                parts.Add($"{kvp.Value}x {urun.Ad}");
                toplam += urun.Fiyat * kvp.Value;
            }
        }
        txtSecimOzet.Text = $"Seçim: {string.Join(", ", parts)}  →  ₺{toplam:N0}";
    }

    private void BtnEkle_Click(object sender, RoutedEventArgs e)
    {
        if (_secimler.Any())
        {
            OnaylandiMi = true;
            this.Close();
        }
    }

    private void BtnIptal_Click(object sender, RoutedEventArgs e)
    {
        OnaylandiMi = false;
        this.Close();
    }
}
