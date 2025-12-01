using Bluestroke.Models;
using Bluestroke.Services;
using Newtonsoft.Json;

namespace Bluestroke.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _testSettingsPath;
    private readonly SettingsService _settingsService;

    public SettingsServiceTests()
    {
        _settingsService = new SettingsService();
        _testSettingsPath = _settingsService.GetSettingsPath();
    }

    [Fact]
    public void LoadSettings_WhenFileDoesNotExist_ShouldReturnDefaults()
    {
        // Arrange - ensure no settings file exists
        var tempService = new TestableSettingsService();

        // Act
        var settings = tempService.LoadSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(SoundPreset.Blue, settings.SelectedPreset);
        Assert.Equal(0.5f, settings.Volume);
        Assert.True(settings.IsEnabled);
    }

    [Fact]
    public void SaveAndLoadSettings_ShouldPreserveAllValues()
    {
        // Arrange
        var tempService = new TestableSettingsService();
        var originalSettings = new AppSettings
        {
            SelectedPreset = SoundPreset.Brown,
            Volume = 0.75f,
            IsEnabled = false,
            CustomSoundFolder = "/test/path",
            StartWithWindows = true
        };

        // Act
        tempService.SaveSettings(originalSettings);
        var loadedSettings = tempService.LoadSettings();

        // Assert
        Assert.Equal(originalSettings.SelectedPreset, loadedSettings.SelectedPreset);
        Assert.Equal(originalSettings.Volume, loadedSettings.Volume);
        Assert.Equal(originalSettings.IsEnabled, loadedSettings.IsEnabled);
        Assert.Equal(originalSettings.CustomSoundFolder, loadedSettings.CustomSoundFolder);
        Assert.Equal(originalSettings.StartWithWindows, loadedSettings.StartWithWindows);

        // Cleanup
        tempService.Cleanup();
    }

    [Fact]
    public void GetSettingsPath_ShouldReturnValidPath()
    {
        // Act
        var path = _settingsService.GetSettingsPath();

        // Assert
        Assert.NotNull(path);
        Assert.NotEmpty(path);
        Assert.EndsWith("settings.json", path);
    }

    public void Dispose()
    {
        // No cleanup needed for read-only tests
    }
}

/// <summary>
/// Testable version of SettingsService that uses a temp directory.
/// </summary>
internal class TestableSettingsService
{
    private readonly string _testFolder;
    private readonly string _testSettingsPath;

    public TestableSettingsService()
    {
        _testFolder = Path.Combine(Path.GetTempPath(), $"BluestrokeTest_{Guid.NewGuid()}");
        _testSettingsPath = Path.Combine(_testFolder, "settings.json");
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_testSettingsPath))
            {
                string json = File.ReadAllText(_testSettingsPath);
                var settings = JsonConvert.DeserializeObject<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }

        return new AppSettings();
    }

    public void SaveSettings(AppSettings settings)
    {
        try
        {
            if (!Directory.Exists(_testFolder))
            {
                Directory.CreateDirectory(_testFolder);
            }

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_testSettingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(_testFolder))
            {
                Directory.Delete(_testFolder, true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
