using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using ServerApplication.Hubs;
using ServerApplication.Data;
using ServerApplication.Services;
using System.Configuration;
using System.Data;

namespace ServerApplication;

public partial class App : Application
{
    private IHost? _host;
    public ConfigurationService ConfigService { get; } = new ConfigurationService();

    public App()
    {
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var roleWindow = new ServerApplication.Views.Common.RoleSelectionWindow();
        roleWindow.Show();
    }

    public async Task StartMasterHostAsync()
    {
        if (_host == null)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddDbContext<AppDbContext>();
                        services.AddSignalR();
                        services.AddSingleton<MainWindow>();
                    });
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHub<CafeHub>("/cafeHub");
                        });
                    });
                    webBuilder.UseUrls("http://0.0.0.0:5000"); 
                })
                .Build();
        }
        await _host.StartAsync();
    }

    public async Task StopMasterHostAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
            _host = null;
        }
    }

    public MainWindow GetMainWindowFromHost()
    {
        return _host!.Services.GetRequiredService<MainWindow>();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await StopMasterHostAsync();
        base.OnExit(e);
    }
}
