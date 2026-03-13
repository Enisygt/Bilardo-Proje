using System.Windows;
using ServerApplication.Services;

namespace ServerApplication.Views.Common;

public partial class RoleSelectionWindow : Window
{
    private readonly ConfigurationService _configService;
    private AppConfig _config = new AppConfig();

    public RoleSelectionWindow()
    {
        InitializeComponent();
        _configService = ((App)Application.Current).ConfigService;
        _config = _configService.LoadConfig();

        if (_config.Role == "Node")
        {
            txtMasterIp.Text = _config.MasterIp;
        }
    }

    private async void BtnMaster_Click(object sender, RoutedEventArgs e)
    {
        _config.Role = "Master";
        _config.MasterIp = "127.0.0.1"; // Self
        _configService.SaveConfig(_config);
        
        btnMaster.IsEnabled = false;
        btnNode.IsEnabled = false;

        var app = (App)Application.Current;
        await app.StartMasterHostAsync();
        
        var mainWindow = app.GetMainWindowFromHost();
        mainWindow.Show();
        this.Close();
    }

    private void BtnNode_Click(object sender, RoutedEventArgs e)
    {
        _config.Role = "Node";
        btnMaster.Visibility = Visibility.Collapsed;
        btnNode.Visibility = Visibility.Collapsed;
        pnlIpEntry.Visibility = Visibility.Visible;
    }

    private void BtnSaveNode_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtMasterIp.Text))
        {
            MessageBox.Show("Lütfen geçerli bir IP adresi girin.");
            return;
        }

        _config.MasterIp = txtMasterIp.Text.Trim();
        _configService.SaveConfig(_config);
        
        var clientWindow = new ServerApplication.Views.Client.ClientWindow();
        clientWindow.Show();
        this.Close();
    }
}
