using Bluestroke.Models;

namespace Bluestroke.Tests;

public class AppSettingsTests
{
    [Fact]
    public void DefaultSettings_ShouldHaveCorrectValues()
    {
        // Arrange & Act
        var settings = new AppSettings();

        // Assert
        Assert.Equal(SoundPreset.Blue, settings.SelectedPreset);
        Assert.Equal(0.5f, settings.Volume);
        Assert.True(settings.IsEnabled);
        Assert.Null(settings.CustomSoundFolder);
        Assert.False(settings.StartWithWindows);
    }

    [Theory]
    [InlineData(0.0f)]
    [InlineData(0.5f)]
    [InlineData(1.0f)]
    public void Volume_ShouldAcceptValidValues(float volume)
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.Volume = volume;

        // Assert
        Assert.Equal(volume, settings.Volume);
    }

    [Theory]
    [InlineData(SoundPreset.Blue)]
    [InlineData(SoundPreset.Brown)]
    [InlineData(SoundPreset.Red)]
    [InlineData(SoundPreset.MacBook)]
    [InlineData(SoundPreset.Custom)]
    public void SelectedPreset_ShouldAcceptAllPresets(SoundPreset preset)
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.SelectedPreset = preset;

        // Assert
        Assert.Equal(preset, settings.SelectedPreset);
    }

    [Fact]
    public void CustomSoundFolder_ShouldAcceptPath()
    {
        // Arrange
        var settings = new AppSettings();
        const string testPath = "C:\\Users\\Test\\Sounds";

        // Act
        settings.CustomSoundFolder = testPath;

        // Assert
        Assert.Equal(testPath, settings.CustomSoundFolder);
    }

    [Fact]
    public void IsEnabled_ShouldToggle()
    {
        // Arrange
        var settings = new AppSettings();

        // Act & Assert
        Assert.True(settings.IsEnabled);
        settings.IsEnabled = false;
        Assert.False(settings.IsEnabled);
        settings.IsEnabled = true;
        Assert.True(settings.IsEnabled);
    }
}
