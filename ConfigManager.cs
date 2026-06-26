using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace idotmatrix_gui
{
    public class SceneConfig
    {
        public string Name { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public int DurationSeconds { get; set; } = 10;
    }

    public class AppConfig
    {
        public List<SceneConfig> Scenes { get; set; } = new List<SceneConfig>();
        public string WeatherCity { get; set; } = "";
        public double Latitude { get; set; } = 0.0;
        public double Longitude { get; set; } = 0.0;
        public string ICalUrl { get; set; } = "";
        public string TargetMac { get; set; } = "";
    }

    public static class ConfigManager
    {
        private static readonly string ConfigFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "iDotMatrixDashboard"
        );
        private static readonly string ConfigPath = Path.Combine(ConfigFolder, "config.json");

        public static AppConfig LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        return config;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
            }

            return new AppConfig(); // Return empty default config
        }

        public static void SaveConfig(AppConfig config)
        {
            try
            {
                if (!Directory.Exists(ConfigFolder))
                {
                    Directory.CreateDirectory(ConfigFolder);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving config: {ex.Message}");
            }
        }
    }
}
