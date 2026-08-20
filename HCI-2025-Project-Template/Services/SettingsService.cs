namespace HCI_2025_Project_Template.Services
{
    using HCI_2025_Project_Template.Models;
    using System.IO;
    using System.Text.Json;

    public class SettingsService
    {
        private readonly string _path;

        public SettingsService()
        {
            _path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PaperlessClient",
                "settings.json");
        }

        private Dictionary<string, AppSettings> LoadAll()
        {
            if (!File.Exists(_path))
                return new Dictionary<string, AppSettings>();

            var json = File.ReadAllText(_path);

            return JsonSerializer.Deserialize<Dictionary<string, AppSettings>>(json)
                   ?? new Dictionary<string, AppSettings>();
        }

        public AppSettings Load(string username)
        {
            var allSettings = LoadAll();

            if (allSettings.TryGetValue(username, out var settings))
                return settings;
            var defaultSettings = new AppSettings();

            Save(username, defaultSettings);
            return new AppSettings();
        }

        public void Save(string username, AppSettings settings)
        {
            var allSettings = LoadAll();

            allSettings[username] = settings;

            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            var json = JsonSerializer.Serialize(
                allSettings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(_path, json);
        }
    }
}