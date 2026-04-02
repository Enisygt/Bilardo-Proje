using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerApplication.Data;
using SharedLibrary.Models;
using System;
using System.Linq;

namespace ServerApplication.ViewModels;

public partial class CheckoutViewModel : ObservableObject
{
    private readonly MasaViewModel _masa;

    public CheckoutViewModel(MasaViewModel masa)
    {
        _masa = masa;
        MasaNo = masa.MasaNo;
        AraToplam = masa.ToplamBorc;
        BaslangicZamani = masa.BaslangicZamani ?? DateTime.Now.AddHours(-1);
        BitisZamani = DateTime.Now;

        // Süre hesapla
        var diff = BitisZamani - BaslangicZamani;
        SureText = $"⏱ {BaslangicZamani:HH:mm} → {BitisZamani:HH:mm} ({(int)diff.TotalHours}s {diff.Minutes:D2}dk)";

        // Sipariş toplamını hesapla
        try
        {
            using var ctx = new AppDbContext();
            var siparisler = ctx.Siparisler.Where(s => s.MasaId == masa.Id).ToList();
            SiparisToplam = siparisler.Sum(s =>
            {
                var urun = ctx.Urunler.Find(s.UrunId);
                return urun != null ? urun.Fiyat * s.Adet : 0;
            });
        }
        catch
        {
            SiparisToplam = 0;
        }

        HesaplaToplam();
    }

    [ObservableProperty]
    private string _masaNo;

    [ObservableProperty]
    private string _sureText = "";

    [ObservableProperty]
    private decimal _araToplam;

    [ObservableProperty]
    private decimal _siparisToplam;

    [ObservableProperty]
    private decimal _toplamTutar;

    [ObservableProperty]
    private decimal _toplamIndirim;

    [ObservableProperty]
    private string _indirimAciklama = "";

    [ObservableProperty]
    private int _yuzdeIndirimOrani;

    [ObservableProperty]
    private string _manuelDuzeltmeText = "";

    [ObservableProperty]
    private bool _isDiscountEnabled;

    [ObservableProperty]
    private string _adminPassword = string.Empty;

    private DateTime BaslangicZamani { get; }
    private DateTime BitisZamani { get; }

    partial void OnAdminPasswordChanged(string value)
    {
        IsDiscountEnabled = (value == "admin123");
        if (!IsDiscountEnabled)
        {
            YuzdeIndirimOrani = 0;
            ManuelDuzeltmeText = "";
            HesaplaToplam();
        }
    }

    partial void OnYuzdeIndirimOraniChanged(int value)
    {
        HesaplaToplam();
    }

    partial void OnManuelDuzeltmeTextChanged(string value)
    {
        HesaplaToplam();
    }

    [RelayCommand]
    private void YuzdeIndirim(string oran)
    {
        if (int.TryParse(oran, out int yuzde))
        {
            YuzdeIndirimOrani = yuzde;
        }
    }

    private void HesaplaToplam()
    {
        decimal brut = AraToplam + SiparisToplam;
        decimal yuzdeIndirim = brut * YuzdeIndirimOrani / 100m;

        // Manuel düzeltme: +50 veya 50 → ekle, -50 → çıkar
        decimal manuelDeger = 0;
        if (!string.IsNullOrWhiteSpace(ManuelDuzeltmeText))
        {
            string temiz = ManuelDuzeltmeText.Trim().Replace(",", ".");
            decimal.TryParse(temiz, System.Globalization.NumberStyles.Any, 
                System.Globalization.CultureInfo.InvariantCulture, out manuelDeger);
        }

        ToplamIndirim = yuzdeIndirim;
        
        // İndirim açıklaması
        if (YuzdeIndirimOrani > 0 && manuelDeger != 0)
        {
            string isaret = manuelDeger > 0 ? "+" : "";
            IndirimAciklama = $"%{YuzdeIndirimOrani} indirim + {isaret}₺{manuelDeger:N2} düzeltme";
        }
        else if (YuzdeIndirimOrani > 0)
            IndirimAciklama = $"%{YuzdeIndirimOrani} indirim uygulandı";
        else if (manuelDeger != 0)
        {
            string isaret = manuelDeger > 0 ? "+" : "";
            IndirimAciklama = $"{isaret}₺{manuelDeger:N2} manuel düzeltme";
        }
        else
            IndirimAciklama = "";

        // Toplam = brut - %indirim + manuelDüzeltme (+ artırır, - azaltır)
        ToplamTutar = brut - ToplamIndirim + manuelDeger;
        if (ToplamTutar < 0) ToplamTutar = 0;
    }

    [RelayCommand]
    private void OdemeYap(string odemeTipi)
    {
        // Manuel düzeltme değerini parse et
        decimal manuelDeger = 0;
        if (!string.IsNullOrWhiteSpace(ManuelDuzeltmeText))
        {
            string temiz = ManuelDuzeltmeText.Trim().Replace(",", ".");
            decimal.TryParse(temiz, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out manuelDeger);
        }

        using var context = new AppDbContext();
        var islem = new IslemGecmisi
        {
            MasaNo = _masa.MasaNo,
            Baslangic = BaslangicZamani,
            Bitis = BitisZamani,
            UcretSure = AraToplam,
            UcretSiparis = SiparisToplam,
            Indirim = ToplamIndirim,
            ManuelDuzeltme = manuelDeger,
            ToplamTutar = ToplamTutar,
            OdemeTipi = odemeTipi,
            IsManual = false
        };
        context.IslemGecmisleri.Add(islem);

        var dBMasa = context.Masalar.Find(_masa.Id);
        if (dBMasa != null)
        {
            dBMasa.Durum = SharedLibrary.Enums.MasaDurum.Bos;
            dBMasa.BaslangicZamani = null;
            dBMasa.ToplamBorc = 0;
            dBMasa.BekleyenSiparis = string.Empty;
            dBMasa.PlayerAName = string.Empty;
            dBMasa.PlayerBName = string.Empty;
        }

        var siparisler = context.Siparisler.Where(s => s.MasaId == _masa.Id).ToList();
        context.Siparisler.RemoveRange(siparisler);

        context.SaveChanges();

        _masa.Durum = SharedLibrary.Enums.MasaDurum.Bos;
        _masa.ToplamBorc = 0;
        _masa.GecenSureText = "00:00";
        _masa.BekleyenSiparis = string.Empty;
        _masa.BaslangicZamani = null;
    }
}
