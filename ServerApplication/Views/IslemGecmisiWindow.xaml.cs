using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ServerApplication.Data;

namespace ServerApplication.Views;

public partial class IslemGecmisiWindow : Window
{
    public IslemGecmisiWindow()
    {
        InitializeComponent();
        LoadData("Bugun");
    }

    private void Filtre_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string filtre)
        {
            // Tüm butonları pasif renge döndür
            foreach (var child in ((StackPanel)btn.Parent).Children)
            {
                if (child is Button b)
                    b.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A4A"));
            }
            // Seçili butonu aktif yap
            btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32"));

            LoadData(filtre);
        }
    }

    private void LoadData(string filtre)
    {
        try
        {
            using var ctx = new AppDbContext();
            ctx.Database.EnsureCreated();

            var query = ctx.IslemGecmisleri.AsQueryable();

            var bugun = DateTime.Today;
            switch (filtre)
            {
                case "Bugun":
                    query = query.Where(i => i.Bitis >= bugun);
                    break;
                case "Dun":
                    var dun = bugun.AddDays(-1);
                    query = query.Where(i => i.Bitis >= dun && i.Bitis < bugun);
                    break;
                case "Hafta":
                    var haftaBasi = bugun.AddDays(-(int)bugun.DayOfWeek + 1);
                    query = query.Where(i => i.Bitis >= haftaBasi);
                    break;
                case "Tumu":
                    // Filtre yok
                    break;
            }

            var islemler = query.OrderByDescending(i => i.Bitis).ToList();
            lstIslemler.ItemsSource = islemler;

            // İstatistikler
            txtIslemSayisi.Text = islemler.Count.ToString();
            var nakit = islemler.Where(i => i.OdemeTipi == "Nakit").Sum(i => i.ToplamTutar);
            var kart = islemler.Where(i => i.OdemeTipi != "Nakit").Sum(i => i.ToplamTutar);
            txtNakit.Text = $"₺{nakit:N0}";
            txtKart.Text = $"₺{kart:N0}";
            txtToplam.Text = $"₺{nakit + kart:N0}";
        }
        catch { }
    }

    private void BtnKapat_Click(object sender, RoutedEventArgs e) => Close();
}
