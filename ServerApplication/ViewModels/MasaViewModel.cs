using CommunityToolkit.Mvvm.ComponentModel;
using SharedLibrary.Models;
using SharedLibrary.Enums;

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

    public string DurumText => Durum.ToString();

    public string BorderColor => Durum switch
    {
        MasaDurum.Bos => "Gray",
        MasaDurum.Aktif => "Green",
        MasaDurum.SiparisBekliyor => "Orange",
        MasaDurum.Arizali => "Red",
        _ => "Black"
    };

    partial void OnDurumChanged(MasaDurum value)
    {
        OnPropertyChanged(nameof(DurumText));
        OnPropertyChanged(nameof(BorderColor));
    }
}
