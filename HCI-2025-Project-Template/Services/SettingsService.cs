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

        public AppSettings Load()
        {
            if (!File.Exists(_path))
                return new AppSettings();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        public void Save(AppSettings settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_path, json);
        }
    }
}
