using System;
using System.IO;
using System.Text.Json;

namespace SamsungTouchUtility.Configuration
{
    public sealed class AppConfig
    {
        public string ApplicationName { get; set; } = "Samsung Touchscreen Utility";
        public string Version { get; set; } = "1.0.0";
        public bool StartWithWindows { get; set; } = false;
        public string? PreferredInstanceId { get; set; } = null;

        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SamsungTouchUtility");

        private static readonly string ConfigFilePath = Path.Combine(ConfigDir, "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch
            {
            }

            return new AppConfig();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch
            {
            }
        }
    }
}
