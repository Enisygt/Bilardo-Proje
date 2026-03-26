using SharedLibrary.Enums;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Timers;

namespace ServerApplication.Services;

/// <summary>
/// Demo modu servisi. 9 masanın simüle edilmiş verilerini yönetir.
/// </summary>
public class DemoService : IDisposable
{
    private readonly System.Timers.Timer _updateTimer;
    private readonly Random _random = new();
    private readonly List<Masa> _demoMasalar = new();

    public event Action? OnMasalarUpdated;
    public event Action<int, string>? OnSiparisGeldi; // masaId, sipariş özeti

    private readonly string[] _isimlerA = { "Ahmet", "Mehmet", "Ali", "Veli", "Hasan", "Hüseyin", "Osman", "Kemal", "Emre" };
    private readonly string[] _isimlerB = { "Fatih", "Burak", "Can", "Murat", "Serkan", "Ege", "Barış", "Cem", "Kaan" };

    public DemoService()
    {
        InitializeDemoData();

        _updateTimer = new System.Timers.Timer(3000); // Her 3 saniyede güncelle
        _updateTimer.Elapsed += (s, e) => UpdateDemoData();
        _updateTimer.AutoReset = true;
    }

    public void Start()
    {
        _updateTimer.Start();
    }

    public void Stop()
    {
        _updateTimer.Stop();
    }

    public List<Masa> GetMasalar()
    {
        lock (_demoMasalar)
        {
            return new List<Masa>(_demoMasalar);
        }
    }

    private void InitializeDemoData()
    {
        _demoMasalar.Clear();

        // 3 boş, 4 aktif, 2 sipariş bekleyen
        var durumlar = new[]
        {
            MasaDurum.Aktif,         // Masa 1
            MasaDurum.Bos,           // Masa 2
            MasaDurum.Aktif,         // Masa 3
            MasaDurum.SiparisBekliyor, // Masa 4
            MasaDurum.Aktif,         // Masa 5
            MasaDurum.Bos,           // Masa 6
            MasaDurum.SiparisBekliyor, // Masa 7
            MasaDurum.Aktif,         // Masa 8
            MasaDurum.Bos,           // Masa 9
        };

        for (int i = 0; i < 9; i++)
        {
            var masa = new Masa
            {
                Id = i + 1,
                MasaNo = $"Masa {i + 1}",
                Durum = durumlar[i],
                SaatlikUcret = 150m,
                IpAddress = $"192.168.1.{101 + i}",
            };

            if (durumlar[i] == MasaDurum.Aktif || durumlar[i] == MasaDurum.SiparisBekliyor)
            {
                masa.BaslangicZamani = DateTime.Now.AddMinutes(-_random.Next(15, 180));
                masa.PlayerAName = _isimlerA[i];
                masa.PlayerBName = _isimlerB[i];
                masa.ToplamBorc = _random.Next(50, 400);
            }

            if (durumlar[i] == MasaDurum.SiparisBekliyor)
            {
                masa.BekleyenSiparis = "2x Çay, 1x Kahve";
            }

            _demoMasalar.Add(masa);
        }
    }

    private void UpdateDemoData()
    {
        lock (_demoMasalar)
        {
            foreach (var masa in _demoMasalar)
            {
                if (masa.Durum == MasaDurum.Aktif || masa.Durum == MasaDurum.SiparisBekliyor)
                {
                    // Rastgele tutar artır
                    masa.ToplamBorc += _random.Next(1, 5);
                }
            }

            // Rastgele bir aktif masaya sipariş bildirimi gönder (%15 ihtimal)
            if (_random.Next(100) < 15)
            {
                var aktifMasalar = _demoMasalar.FindAll(m => m.Durum == MasaDurum.Aktif);
                if (aktifMasalar.Count > 0)
                {
                    var masa = aktifMasalar[_random.Next(aktifMasalar.Count)];
                    var siparisler = new[] { "2x Çay", "1x Kahve, 1x Su", "3x Çay, 1x Tost", "1x Kola, 1x Sandviç" };
                    var siparis = siparisler[_random.Next(siparisler.Length)];

                    masa.Durum = MasaDurum.SiparisBekliyor;
                    masa.BekleyenSiparis = siparis;
                    OnSiparisGeldi?.Invoke(masa.Id, siparis);
                }
            }
        }

        OnMasalarUpdated?.Invoke();
    }

    public void SiparisiOnayla(int masaId)
    {
        lock (_demoMasalar)
        {
            var masa = _demoMasalar.Find(m => m.Id == masaId);
            if (masa != null)
            {
                masa.Durum = MasaDurum.Aktif;
                masa.BekleyenSiparis = string.Empty;
            }
        }
        OnMasalarUpdated?.Invoke();
        OnMasalarUpdated?.Invoke();
    }

    public void EkleBorc(int masaId, decimal miktar)
    {
        lock (_demoMasalar)
        {
            var masa = _demoMasalar.Find(m => m.Id == masaId);
            if (masa != null)
            {
                masa.ToplamBorc += miktar;
            }
        }
    }

    public void Dispose()
    {
        _updateTimer?.Stop();
        _updateTimer?.Dispose();
    }
}
