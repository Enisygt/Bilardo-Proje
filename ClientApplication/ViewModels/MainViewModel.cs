using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using ClientApplication.Services;
using System.Windows.Threading;

namespace ClientApplication.ViewModels;

public partial class UrunViewModel : ObservableObject
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public decimal Fiyat { get; set; }
}

public partial class MainViewModel : ObservableObject
{
    private HubConnection? _hubConnection;
    private KioskService _kioskService;
    private DispatcherTimer _timer;

    public MainViewModel()
    {
        _kioskService = new KioskService();
        _kioskService.AdminPanelRequested += () => IsAdminPanelVisible = true;
        _kioskService.StartKioskMode();

        MenuItems.Add(new UrunViewModel { Id = 1, Ad = "Çay", Fiyat = 15 });
        MenuItems.Add(new UrunViewModel { Id = 2, Ad = "Kahve", Fiyat = 35 });
        MenuItems.Add(new UrunViewModel { Id = 3, Ad = "Kola", Fiyat = 40 });

        _timer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => Heartbeat();
        _timer.Start();

        ConnectToServerAsync();
    }

    [ObservableProperty]
    private string _kalanSureText = "00:00";

    [ObservableProperty]
    private decimal _toplamTutar;

    [ObservableProperty]
    private bool _isInvalidIp = true;

    [ObservableProperty]
    private string _invalidIpMessage = "Sunucuya bağlanılıyor...";

    [ObservableProperty]
    private bool _isAdminPanelVisible;

    [ObservableProperty]
    private string _adminPassword = string.Empty;

    public ObservableCollection<UrunViewModel> MenuItems { get; } = new();

    private async void ConnectToServerAsync()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/cafeHub")
            .Build();

        _hubConnection.On<string>("TerminalVerified", (masaNo) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsInvalidIp = false;
            });
        });

        try
        {
            await _hubConnection.StartAsync();
            // Assuming no validation logic implemented on Server for "invalid IP" to send message yet
            // If it connects but no TerminalVerified is sent, it means invalid IP. We wait 2 seconds.
            await Task.Delay(2000);
            if (IsInvalidIp)
            {
                 InvalidIpMessage = "Bu terminal sistemde kayıtlı değil.";
            }
        }
        catch (System.Exception ex)
        {
            InvalidIpMessage = $"Bağlantı Hatası: {ex.Message}";
        }
    }

    [ObservableProperty]
    private System.DateTime _startTime = System.DateTime.Now;

    [ObservableProperty]
    private decimal _saatlikUcret = 150.0m;

    private decimal _siparisToplami = 0;

    [RelayCommand]
    private void SiparisEkle(UrunViewModel urun)
    {
        _siparisToplami += urun.Fiyat;
        Heartbeat(); // Recalculate immediately
    }

    [RelayCommand]
    private void CancelAdminPanel()
    {
        IsAdminPanelVisible = false;
        AdminPassword = string.Empty;
    }

    [RelayCommand]
    private void ExitApp()
    {
        if (AdminPassword == "admin123") // Fixed admin password for kiosk exit
        {
            _kioskService.StopKioskMode();
            Application.Current.Shutdown();
        }
    }

    private void Heartbeat()
    {
        if (!IsInvalidIp)
        {
            var diff = System.DateTime.Now - StartTime;
            KalanSureText = $"Süre: {(int)diff.TotalHours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
            
            decimal sureUcreti = (decimal)diff.TotalMinutes * (SaatlikUcret / 60.0m);
            ToplamTutar = sureUcreti + _siparisToplami;

            // Here we would also send a ping to Server for caching
            if (_hubConnection?.State == HubConnectionState.Connected && diff.Seconds == 0)
            {
                // Send heartbeat every minute (when seconds == 0 approx, depending on timer precision)
                _hubConnection.SendAsync("Heartbeat", ToplamTutar);
            }
        }
    }
}
