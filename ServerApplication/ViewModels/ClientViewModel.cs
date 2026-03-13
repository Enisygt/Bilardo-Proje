using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using ServerApplication.Services;
using System.Windows.Threading;

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

    // Onaylanan adetten daha fazlaysa azaltılabilir
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

    public ClientViewModel()
    {
        _kioskService = new KioskService();
        _kioskService.AdminPanelRequested += () => IsAdminPanelVisible = true;
        _kioskService.StartKioskMode();

        MenuItems.Add(new UrunViewModel { Id = 1, Ad = "Çay", Fiyat = 15 });
        MenuItems.Add(new UrunViewModel { Id = 2, Ad = "Kahve", Fiyat = 35 });
        MenuItems.Add(new UrunViewModel { Id = 3, Ad = "Kola", Fiyat = 40 });
        MenuItems.Add(new UrunViewModel { Id = 4, Ad = "Su", Fiyat = 10 });
        MenuItems.Add(new UrunViewModel { Id = 5, Ad = "Meyve Suyu", Fiyat = 30 });

        _timer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => Heartbeat();
        _timer.Start();

        ConnectToServerAsync();
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
    private int _activePlayer = 1; // 1 for Player A, 2 for Player B

    [ObservableProperty]
    private int _turCount = 1; // Innings count

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
            // Player A bitirdi
            PlayerAVisits++;
            ActivePlayer = 2; // Sıra Player B'ye geçti
            
            // A ortalamasını hesapla
            if (PlayerAVisits > 0)
            {
                double avg = (double)PlayerAScore / PlayerAVisits;
                PlayerAAverageText = $"Istaka: {PlayerAVisits}   Ort: {avg:F2}";
            }
        }
        else
        {
            // Player B bitirdi
            PlayerBVisits++;
            ActivePlayer = 1; // Sıra Player A'ya geçti
            TurCount++; // Her ikisi de 1 kez oynadığı için tur arttı

            // B ortalamasını hesapla
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
                // TODO: İleride config.MasaId ile eşleşiyorsa sıfırla mantığı eklenebilir.
                // Şimdilik gelen her sıfırlama komutunda bu ekranı sıfırlıyoruz.
                ResetMatch();
            });
        });

        _hubConnection.Closed += async (error) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsInvalidIp = true;
                InvalidIpMessage = "Bağlantı koptu. Yeniden bağlanılıyor...";
            });
            await Task.Delay(new Random().Next(0, 5) * 1000);
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
        // Güncel saat (Dashboard için)
        CurrentDateTimeText = System.DateTime.Now.ToString("HH:mm:ss\ndd MMMM yyyy");

        if (!IsMatchStarted) return; // Maç başlamadıysa süreyi ve tutarı artırma

        var diff = System.DateTime.Now - StartTime;
        KalanSureText = $"Süre: {(int)diff.TotalHours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
        
        decimal sureUcreti = (decimal)diff.TotalMinutes * (SaatlikUcret / 60.0m);
        ToplamTutar = sureUcreti + SiparisToplami;

        if (_hubConnection?.State == HubConnectionState.Connected && diff.Seconds == 0)
        {
            _hubConnection.SendAsync("Heartbeat", ToplamTutar);
        }
    }
}
