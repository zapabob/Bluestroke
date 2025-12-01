using Newtonsoft.Json;

namespace Bluestroke.Models;

/// <summary>
/// Represents the application settings that are persisted to JSON.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// The currently selected sound preset.
    /// </summary>
    public SoundPreset SelectedPreset { get; set; } = SoundPreset.Blue;

    /// <summary>
    /// The volume level (0.0 to 1.0).
    /// </summary>
    public float Volume { get; set; } = 0.5f;

    /// <summary>
    /// Whether the application is enabled (playing sounds).
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Path to custom sound folder for user-defined sounds.
    /// </summary>
    public string? CustomSoundFolder { get; set; }

    /// <summary>
    /// Whether to start with Windows.
    /// </summary>
    public bool StartWithWindows { get; set; } = false;
}

/// <summary>
/// Available sound presets for keyboard sounds.
/// </summary>
public enum SoundPreset
{
    /// <summary>
    /// Blue switch mechanical keyboard sound (clicky).
    /// </summary>
    Blue,

    /// <summary>
    /// Brown switch mechanical keyboard sound (tactile).
    /// </summary>
    Brown,

    /// <summary>
    /// Red switch mechanical keyboard sound (linear).
    /// </summary>
    Red,

    /// <summary>
    /// MacBook style keyboard sound.
    /// </summary>
    MacBook,

    /// <summary>
    /// Custom user-defined sounds.
    /// </summary>
    Custom
}
