using Newtonsoft.Json;

namespace Bluestroke.Services;

/// <summary>
/// Service for managing application settings persistence.
/// </summary>
public class SettingsService
{
    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Bluestroke");

    private static readonly string SettingsPath = Path.Combine(SettingsFolder, "settings.json");

    /// <summary>
    /// Loads settings from the JSON file, or returns defaults if not found.
    /// </summary>
    public Models.AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                var settings = JsonConvert.DeserializeObject<Models.AppSettings>(json);
                return settings ?? new Models.AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }

        return new Models.AppSettings();
    }

    /// <summary>
    /// Saves settings to the JSON file.
    /// </summary>
    public void SaveSettings(Models.AppSettings settings)
    {
        try
        {
            if (!Directory.Exists(SettingsFolder))
            {
                Directory.CreateDirectory(SettingsFolder);
            }

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the path to the settings file.
    /// </summary>
    public string GetSettingsPath() => SettingsPath;
}
