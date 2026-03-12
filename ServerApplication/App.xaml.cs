using System.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using ServerApplication.Hubs;
using ServerApplication.Data;
using System.Configuration;
using System.Data;

namespace ServerApplication;

public partial class App : Application
{
    private IHost? _host;

    public App()
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

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        await _host!.StartAsync();
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host != null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
            _host.Dispose();
        }
        base.OnExit(e);
    }
}
