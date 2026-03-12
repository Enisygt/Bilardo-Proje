using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerApplication.Data;
using SharedLibrary.Models;

namespace ServerApplication.ViewModels;

public partial class CheckoutViewModel : ObservableObject
{
    private readonly MasaViewModel _masa;

    public CheckoutViewModel(MasaViewModel masa)
    {
        _masa = masa;
        MasaNo = masa.MasaNo;
        AraToplam = masa.ToplamBorc;
        HesaplaToplam();
    }

    [ObservableProperty]
    private string _masaNo;

    [ObservableProperty]
    private decimal _araToplam;

    [ObservableProperty]
    private decimal _indirim;

    [ObservableProperty]
    private decimal _toplamTutar;

    [ObservableProperty]
    private bool _isDiscountEnabled;

    [ObservableProperty]
    private string _adminPassword = string.Empty;

    partial void OnIndirimChanged(decimal value)
    {
        HesaplaToplam();
    }

    partial void OnAdminPasswordChanged(string value)
    {
        if (value == "1234") // Hardcoded admin password validation
        {
            IsDiscountEnabled = true;
        }
        else
        {
            IsDiscountEnabled = false;
            Indirim = 0;
        }
    }

    private void HesaplaToplam()
    {
        ToplamTutar = AraToplam - Indirim;
        if (ToplamTutar < 0) ToplamTutar = 0;
    }

    [RelayCommand]
    private void OdemeYap(string odemeTipi)
    {
        using var context = new AppDbContext();
        var islem = new IslemGecmisi
        {
            MasaNo = _masa.MasaNo,
            Baslangic = System.DateTime.Now.AddMinutes(-120), // Placeholder logic for time difference
            Bitis = System.DateTime.Now,
            UcretSiparis = _masa.ToplamBorc,
            Indirim = Indirim,
            ToplamTutar = ToplamTutar,
            IsManual = true
        };
        context.IslemGecmisleri.Add(islem);
        
        var dBMasa = context.Masalar.Find(_masa.Id);
        if (dBMasa != null)
        {
            dBMasa.Durum = SharedLibrary.Enums.MasaDurum.Bos;
            _masa.Durum = SharedLibrary.Enums.MasaDurum.Bos;
            _masa.ToplamBorc = 0;
            _masa.GecenSureText = "00:00";
        }
        context.SaveChanges();
    }
}
