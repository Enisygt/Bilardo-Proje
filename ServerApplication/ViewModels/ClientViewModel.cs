using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using ServerApplication.Services;
using System.Windows.Threading;
using System.Linq;

namespace ServerApplication.ViewModels;

public partial class UrunViewModel : ObservableObject
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanDecrease))]
    private int _adet = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanDecrease))]
    private int _onaylananAdet = 0;

    public bool CanDecrease => Adet > OnaylananAdet;

    [RelayCommand]
    private void Arttir()
    {
        Adet++;
    }

    [RelayCommand]
    private void Azalt()
    {
        if (Adet > OnaylananAdet)
            Adet--;
    }
}

public partial class ClientViewModel : ObservableObject
{
    private HubConnection? _hubConnection;
    private KioskService _kioskService;
    private DispatcherTimer _timer;
    private int _masaId = 1;
    private bool _isDemoMode = false;

    public ClientViewModel()
    {
        _kioskService = new KioskService();
        _kioskService.AdminPanelRequested += () => IsAdminPanelVisible = true;
        _kioskService.DemoExitRequested += OnDemoExit;

        var configService = new ConfigurationService();
        var config = configService.LoadConfig();
        _masaId = config.MasaId;
        _isDemoMode = config.IsDemoMode;
        _kioskService.IsDemoMode = _isDemoMode;
        SalonAdi = config.SalonAdi;
        Telefon = config.Telefon;
        Adres = config.Adres;
        _kioskService.IsDemoMode = _isDemoMode;

        _kioskService.StartKioskMode();

        // Menü ürünlerini yükle
        LoadMenuItems();

        _timer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => Heartbeat();
        _timer.Start();

        if (!_isDemoMode)
        {
            ConnectToServerAsync();
        }

        LoadKampanyalar();
        _kampanyaTimer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(15) };
        _kampanyaTimer.Tick += (s, e) => RotateKampanya();
        _kampanyaTimer.Start();
    }

    private void LoadMenuItems()
    {
        try
        {
            using var ctx = new ServerApplication.Data.AppDbContext();
            ctx.Database.EnsureCreated();
            var urunler = ctx.Urunler.Where(u => u.IsAktif).ToList();
            foreach (var u in urunler)
            {
                MenuItems.Add(new UrunViewModel { Id = u.Id, Ad = u.Ad, Fiyat = u.Fiyat });
            }
        }
        catch
        {
            // Fallback menü
            MenuItems.Add(new UrunViewModel { Id = 1, Ad = "Çay", Fiyat = 15 });
            MenuItems.Add(new UrunViewModel { Id = 2, Ad = "Kahve", Fiyat = 35 });
            MenuItems.Add(new UrunViewModel { Id = 3, Ad = "Kola", Fiyat = 40 });
            MenuItems.Add(new UrunViewModel { Id = 4, Ad = "Su", Fiyat = 10 });
            MenuItems.Add(new UrunViewModel { Id = 5, Ad = "Meyve Suyu", Fiyat = 30 });
        }
    }

    private System.Collections.Generic.List<SharedLibrary.Models.Kampanya> _kampanyalar = new();
    private int _kampanyaIndex = 0;
    private DispatcherTimer _kampanyaTimer;
    public event System.Action? OnKampanyaChanged;

    [ObservableProperty]
    private string _salonAdi = "Bilardo Salonu";

    [ObservableProperty]
    private string _telefon = "";

    [ObservableProperty]
    private string _adres = "";

    [ObservableProperty]
    private string _kampanyaBaslik = "Kampanya Yok";

    [ObservableProperty]
    private string _kampanyaAciklama = "";

    [ObservableProperty]
    private string _kampanyaFiyat = "";

    private void LoadKampanyalar()
    {
        try
        {
            using var ctx = new ServerApplication.Data.AppDbContext();
            _kampanyalar = ctx.Kampanyalar.Where(k => k.IsAktif).ToList();
        }
        catch { }

        if (_kampanyalar.Any())
        {
            _kampanyaIndex = 0;
            ApplyKampanya(_kampanyalar[_kampanyaIndex]);
        }
    }

    private void RotateKampanya()
    {
        if (_kampanyalar.Count > 1)
        {
            _kampanyaIndex = (_kampanyaIndex + 1) % _kampanyalar.Count;
            // UI animasyon tetiklemesi için
            OnKampanyaChanged?.Invoke();
            
            // Animasyon süresi kadar gecikmeyle veriyi güncelle
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ApplyKampanya(_kampanyalar[_kampanyaIndex]);
                });
            });
        }
    }

    private void ApplyKampanya(SharedLibrary.Models.Kampanya k)
    {
        KampanyaBaslik = $"🌟 {k.Baslik}";
        KampanyaAciklama = k.Aciklama;
        KampanyaFiyat = $"{k.Fiyat:N0} TL";
    }

    private void OnDemoExit()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _timer?.Stop();
            _kioskService.StopKioskMode();

            var roleWindow = new ServerApplication.Views.Common.RoleSelectionWindow();
            roleWindow.Show();

            foreach (Window window in Application.Current.Windows)
            {
                if (window is ServerApplication.Views.Client.ClientWindow)
                {
                    window.Close();
                    break;
                }
            }
        });
    }

    // ==================== DASHBOARD (PRE-MATCH) ====================
    [ObservableProperty]
    private bool _isMatchStarted = false;

    [ObservableProperty]
    private string _currentDateTimeText = string.Empty;

    [ObservableProperty]
    private string _playerAName = string.Empty;

    [ObservableProperty]
    private string _playerBName = string.Empty;

    public string DisplayPlayerAName => string.IsNullOrWhiteSpace(PlayerAName) ? "PLAYER A" : PlayerAName.ToUpper();
    public string DisplayPlayerBName => string.IsNullOrWhiteSpace(PlayerBName) ? "PLAYER B" : PlayerBName.ToUpper();

    partial void OnPlayerANameChanged(string value) => OnPropertyChanged(nameof(DisplayPlayerAName));
    partial void OnPlayerBNameChanged(string value) => OnPropertyChanged(nameof(DisplayPlayerBName));

    // Hedef Sayı (Target Score)
    [ObservableProperty]
    private int _hedefSayi = 25;

    [RelayCommand]
    private void HedefSayiUp() => HedefSayi += 5;

    [RelayCommand]
    private void HedefSayiDown() { if (HedefSayi > 5) HedefSayi -= 5; }

    // Handikap
    [ObservableProperty]
    private int _handikapA = 0;

    [ObservableProperty]
    private int _handikapB = 0;

    [RelayCommand]
    private void HandikapAUp() => HandikapA++;
    [RelayCommand]
    private void HandikapADown() { if (HandikapA > 0) HandikapA--; }
    [RelayCommand]
    private void HandikapBUp() => HandikapB++;
    [RelayCommand]
    private void HandikapBDown() { if (HandikapB > 0) HandikapB--; }

    // Masa Notu
    [ObservableProperty]
    private string _masaNotu = string.Empty;

    [RelayCommand]
    private void StartMatch()
    {
        IsMatchStarted = true;
        StartTime = System.DateTime.Now;
        Heartbeat();
    }

    [RelayCommand]
    private void ResetMatch()
    {
        IsMatchStarted = false;
        PlayerAName = string.Empty;
        PlayerBName = string.Empty;
        PlayerAScore = 0;
        PlayerBScore = 0;
        ActivePlayer = 1;
        TurCount = 1;
        PlayerAVisits = 0;
        PlayerBVisits = 0;
        PlayerAAverageText = "Istaka: 0   Ort: -";
        PlayerBAverageText = "Istaka: 0   Ort: -";
        KalanSureText = "00:00:00";
        ToplamTutar = 0;
        HedefSayi = 25;
        HandikapA = 0;
        HandikapB = 0;
        MasaNotu = string.Empty;

        foreach (var item in MenuItems)
        {
            item.Adet = 0;
            item.OnaylananAdet = 0;
        }
    }

    // ==================== SCOREBOARD ====================

    [ObservableProperty]
    private int _playerAScore = 0;

    [ObservableProperty]
    private int _playerBScore = 0;

    [ObservableProperty]
    private int _activePlayer = 1;

    [ObservableProperty]
    private int _turCount = 1;

    [ObservableProperty]
    private int _playerAVisits = 0;

    [ObservableProperty]
    private int _playerBVisits = 0;

    [ObservableProperty]
    private string _playerAAverageText = "Istaka: 0   Ort: -";

    [ObservableProperty]
    private string _playerBAverageText = "Istaka: 0   Ort: -";

    [RelayCommand]
    private void PlayerAScoreUp()
    {
        PlayerAScore++;
    }

    [RelayCommand]
    private void PlayerAScoreDown()
    {
        if (PlayerAScore > 0) PlayerAScore--;
    }

    [RelayCommand]
    private void PlayerBScoreUp()
    {
        PlayerBScore++;
    }

    [RelayCommand]
    private void PlayerBScoreDown()
    {
        if (PlayerBScore > 0) PlayerBScore--;
    }

    [RelayCommand]
    private void SwitchTurn()
    {
        if (ActivePlayer == 1)
        {
            PlayerAVisits++;
            ActivePlayer = 2;

            if (PlayerAVisits > 0)
            {
                double avg = (double)PlayerAScore / PlayerAVisits;
                PlayerAAverageText = $"Istaka: {PlayerAVisits}   Ort: {avg:F2}";
            }
        }
        else
        {
            PlayerBVisits++;
            ActivePlayer = 1;
            TurCount++;

            if (PlayerBVisits > 0)
            {
                double avg = (double)PlayerBScore / PlayerBVisits;
                PlayerBAverageText = $"Istaka: {PlayerBVisits}   Ort: {avg:F2}";
            }
        }
    }

    // ==================== CAFE POPUP ====================

    [ObservableProperty]
    private bool _isCafePopupVisible = false;

    [RelayCommand]
    private void ToggleCafePopup()
    {
        IsCafePopupVisible = !IsCafePopupVisible;
    }

    [RelayCommand]
    private void CloseCafePopup()
    {
        IsCafePopupVisible = false;
    }

    // ==================== MEVCUT ALANLAR ====================

    [ObservableProperty]
    private string _kalanSureText = "00:00";

    [ObservableProperty]
    private decimal _toplamTutar;

    [ObservableProperty]
    private bool _isInvalidIp = false;

    [ObservableProperty]
    private string _invalidIpMessage = string.Empty;

    [ObservableProperty]
    private bool _isAdminPanelVisible;

    [ObservableProperty]
    private string _adminPassword = string.Empty;

    public ObservableCollection<UrunViewModel> MenuItems { get; } = new();

    // ==================== SİPARİŞ GÖNDERME ====================

    [RelayCommand]
    private async Task SiparisGonder()
    {
        var yeniSiparisler = MenuItems.Where(m => m.Adet > m.OnaylananAdet).ToList();
        if (!yeniSiparisler.Any()) return;

        var ozet = string.Join(", ", yeniSiparisler.Select(m => $"{m.Adet - m.OnaylananAdet}x {m.Ad}"));

        if (_isDemoMode)
        {
            // Demo modunda direkt onayla
            foreach (var item in yeniSiparisler)
            {
                item.OnaylananAdet = item.Adet;
            }
            return;
        }

        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SiparisGonder", _masaId, ozet);
            }
        }
        catch { }
    }

    // ==================== NETWORK ====================

    private async void ConnectToServerAsync()
    {
        var configService = new ConfigurationService();
        var config = configService.LoadConfig();
        var masterIp = string.IsNullOrWhiteSpace(config.MasterIp) ? "localhost" : config.MasterIp;
        var url = $"http://{masterIp}:5000/cafeHub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(url)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<string>("TerminalVerified", (masaNo) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsInvalidIp = false;
                InvalidIpMessage = string.Empty;
            });
        });

        _hubConnection.On<int>("ResetMatch", (masaId) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (masaId == _masaId)
                {
                    ResetMatch();
                }
            });
        });

        _hubConnection.On<int>("SiparisOnaylandi", (masaId) =>
        {
            if (masaId == _masaId)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var item in MenuItems)
                    {
                        item.OnaylananAdet = item.Adet;
                    }
                });
            }
        });

        _hubConnection.Closed += async (error) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsInvalidIp = true;
                InvalidIpMessage = "Bağlantı koptu. Yeniden bağlanılıyor...";
            });
            await Task.Delay(new System.Random().Next(0, 5) * 1000);
            await StartConnectionAsync();
        };

        await StartConnectionAsync();
    }

    private async Task StartConnectionAsync()
    {
        while (true)
        {
            try
            {
                if (_hubConnection?.State == HubConnectionState.Disconnected)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsInvalidIp = false;
                        InvalidIpMessage = "Sunucuya Arka Planda Bağlanılıyor...";
                    });

                    await _hubConnection.StartAsync();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InvalidIpMessage = "Sunucuya Bağlandı.";
                    });
                }
                break;
            }
            catch (System.Exception)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsInvalidIp = false;
                    InvalidIpMessage = $"Bağlantı Yok. (Arka planda deneniyor...)";
                });
                await Task.Delay(5000);
            }
        }
    }

    // ==================== TUTAR & SİPARİŞ ====================

    [ObservableProperty]
    private System.DateTime _startTime = System.DateTime.Now;

    [ObservableProperty]
    private decimal _saatlikUcret = 150.0m;

    private decimal SiparisToplami
    {
        get
        {
            decimal toplam = 0;
            foreach (var item in MenuItems)
                toplam += item.Adet * item.Fiyat;
            return toplam;
        }
    }

    [RelayCommand]
    private void CancelAdminPanel()
    {
        IsAdminPanelVisible = false;
        AdminPassword = string.Empty;
    }

    [RelayCommand]
    private async Task ExitApp()
    {
        if (AdminPassword == "admin123")
        {
            _timer?.Stop();
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }

            _kioskService.StopKioskMode();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var roleWindow = new ServerApplication.Views.Common.RoleSelectionWindow();
                roleWindow.Show();

                foreach (Window window in Application.Current.Windows)
                {
                    if (window is ServerApplication.Views.Client.ClientWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            });
        }
    }

    private void Heartbeat()
    {
        CurrentDateTimeText = System.DateTime.Now.ToString("HH:mm:ss\ndd MMMM yyyy");

        if (!IsMatchStarted) return;

        var diff = System.DateTime.Now - StartTime;
        KalanSureText = $"Süre: {(int)diff.TotalHours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";

        decimal sureUcreti = (decimal)diff.TotalMinutes * (SaatlikUcret / 60.0m);
        ToplamTutar = sureUcreti + SiparisToplami;

        if (!_isDemoMode && _hubConnection?.State == HubConnectionState.Connected && diff.Seconds == 0)
        {
            _hubConnection.SendAsync("MasaBilgisiGuncelle", _masaId, ToplamTutar, KalanSureText);
        }
    }
}
