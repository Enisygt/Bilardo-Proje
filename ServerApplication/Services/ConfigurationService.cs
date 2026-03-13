using System;
using System.IO;
using System.Text.Json;

namespace ServerApplication.Services;

public class AppConfig
{
    public string Role { get; set; } = string.Empty; // "Master" or "Node"
    public string MasterIp { get; set; } = "127.0.0.1";
}

public class ConfigurationService
{
    private readonly string _configFilePath;

    public ConfigurationService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appData, "BilardoApp");
        Directory.CreateDirectory(appFolder);
        _configFilePath = Path.Combine(appFolder, "config.json");
    }

    public AppConfig LoadConfig()
    {
        if (File.Exists(_configFilePath))
        {
            var json = File.ReadAllText(_configFilePath);
            return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
        }
        return new AppConfig();
    }

    public void SaveConfig(AppConfig config)
    {
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configFilePath, json);
    }
}
