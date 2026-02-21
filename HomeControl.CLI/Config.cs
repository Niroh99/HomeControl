using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeControl.CLI
{
    public class Config
    {
        private static readonly string _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HomeControl.CLI", "config.json");

        public string HostUrl { get; set; } = string.Empty;

        public static Config Load()
        {
            if (!File.Exists(_configPath)) return new Config();

            var configJson = File.ReadAllText(_configPath);

            return System.Text.Json.JsonSerializer.Deserialize<Config>(configJson) ?? new Config();
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath) ?? string.Empty);

            var configJson = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(_configPath, configJson);
        }
    }
}
