using CommunityToolkit.Mvvm.ComponentModel;
using SharedLibrary.Models;
using SharedLibrary.Enums;
using System;

namespace ServerApplication.ViewModels;

public partial class MasaViewModel : ObservableObject
{
    private readonly Masa _masa;

    public MasaViewModel(Masa masa)
    {
        _masa = masa;
        Id = masa.Id;
        MasaNo = masa.MasaNo;
        Durum = masa.Durum;
        ToplamBorc = masa.ToplamBorc;
        BekleyenSiparis = masa.BekleyenSiparis;
        BaslangicZamani = masa.BaslangicZamani;
        PlayerAName = masa.PlayerAName;
        PlayerBName = masa.PlayerBName;
    }

    public int Id { get; }

    [ObservableProperty]
    private string _masaNo;

    [ObservableProperty]
    private MasaDurum _durum;

    [ObservableProperty]
    private decimal _toplamBorc;

    [ObservableProperty]
    private string _gecenSureText = "00:00";

    [ObservableProperty]
    private string _bekleyenSiparis = string.Empty;

    [ObservableProperty]
    private DateTime? _baslangicZamani;

    [ObservableProperty]
    private string _playerAName = string.Empty;

    [ObservableProperty]
    private string _playerBName = string.Empty;

    public bool HasPendingOrder => !string.IsNullOrEmpty(BekleyenSiparis);

    public string DurumText => Durum.ToString();

    public string BorderColor => Durum switch
    {
        MasaDurum.Bos => "#555555",
        MasaDurum.Aktif => "#00E676",
        MasaDurum.SiparisBekliyor => "#FF9800",
        MasaDurum.Arizali => "#F44336",
        _ => "#333333"
    };

    public string BackgroundColor => Durum switch
    {
        MasaDurum.Bos => "#2A2A2A",
        MasaDurum.Aktif => "#1B3A1B",
        MasaDurum.SiparisBekliyor => "#3A2A1B",
        MasaDurum.Arizali => "#3A1B1B",
        _ => "#2A2A2A"
    };

    public string DurumIcon => Durum switch
    {
        MasaDurum.Bos => "⚫",
        MasaDurum.Aktif => "🟢",
        MasaDurum.SiparisBekliyor => "🟠",
        MasaDurum.Arizali => "🔴",
        _ => "⚫"
    };

    partial void OnDurumChanged(MasaDurum value)
    {
        OnPropertyChanged(nameof(DurumText));
        OnPropertyChanged(nameof(BorderColor));
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(DurumIcon));
    }

    partial void OnBekleyenSiparisChanged(string value)
    {
        OnPropertyChanged(nameof(HasPendingOrder));
    }
}
