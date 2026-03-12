using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServerApplication.Data;
using System.Collections.ObjectModel;
using System.Linq;

namespace ServerApplication.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public ObservableCollection<MasaViewModel> Masalar { get; } = new();

    public MainViewModel()
    {
        LoadMasalar();
    }

    [RelayCommand]
    private void Refresh()
    {
        LoadMasalar();
    }

    private void LoadMasalar()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        if (!context.Masalar.Any())
        {
            for (int i = 1; i <= 10; i++)
            {
                context.Masalar.Add(new SharedLibrary.Models.Masa
                {
                    MasaNo = $"Masa {i}",
                    IpAddress = $"192.168.1.{100 + i}",
                    Durum = SharedLibrary.Enums.MasaDurum.Bos,
                    SaatlikUcret = 150.0m
                });
            }
            context.SaveChanges();
        }

        var masalarList = context.Masalar.ToList();
        Masalar.Clear();
        foreach (var m in masalarList)
        {
            Masalar.Add(new MasaViewModel(m));
        }
    }
}
