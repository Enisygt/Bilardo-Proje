using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ServerApplication.Data;
using SharedLibrary.Models;

namespace ServerApplication.Views;

public partial class MenuWindow : Window
{
    public MenuWindow()
    {
        InitializeComponent();
        LoadMenu();
    }

    private void LoadMenu()
    {
        List<Urun> urunler;
        try
        {
            using var ctx = new AppDbContext();
            ctx.Database.EnsureCreated();
            urunler = ctx.Urunler.Where(u => u.IsAktif).OrderBy(u => u.Kategori).ThenBy(u => u.Ad).ToList();
        }
        catch
        {
            urunler = new();
        }

        // Saatlik Masayı Çek (Ayarlardan ya da İlk Masadan)
        decimal saatlikUcret = 150m;
        try
        {
            using var ctxMenu = new AppDbContext();
            var ilkMasa = ctxMenu.Masalar.FirstOrDefault();
            if (ilkMasa != null) saatlikUcret = ilkMasa.SaatlikUcret;
        }
        catch { }

        // Saatlik Ücret Gösterimi
        pnlMenu.Children.Add(new TextBlock
        {
            Text = $"— SAATLİK MASA ÜCRETİ —",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")),
            Margin = new Thickness(0, 8, 0, 4)
        });

        var gridSaat = new Grid();
        gridSaat.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        gridSaat.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        gridSaat.Margin = new Thickness(0, 2, 0, 16);

        var txtSaatAd = new TextBlock { Text = "Masa Açılış / Saat", FontSize = 14, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(txtSaatAd, 0);
        gridSaat.Children.Add(txtSaatAd);

        var txtSaatFiyat = new TextBlock
        {
            Text = $"₺{saatlikUcret:N0}",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676")),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(txtSaatFiyat, 1);
        gridSaat.Children.Add(txtSaatFiyat);
        pnlMenu.Children.Add(gridSaat);

        string? currentKategori = null;
        foreach (var urun in urunler)
        {
            if (urun.Kategori != currentKategori)
            {
                currentKategori = urun.Kategori;
                pnlMenu.Children.Add(new TextBlock
                {
                    Text = $"— {currentKategori} —",
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")),
                    Margin = new Thickness(0, 12, 0, 4)
                });
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.Margin = new Thickness(0, 2, 0, 2);

            var txtAd = new TextBlock { Text = urun.Ad, FontSize = 14, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(txtAd, 0);
            grid.Children.Add(txtAd);

            var txtFiyat = new TextBlock
            {
                Text = $"₺{urun.Fiyat:N0}",
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676")),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtFiyat, 1);
            grid.Children.Add(txtFiyat);

            pnlMenu.Children.Add(grid);
        }
    }

    private void BtnKapat_Click(object sender, RoutedEventArgs e) => Close();
}
