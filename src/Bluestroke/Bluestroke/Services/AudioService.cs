using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Bluestroke.Models;

namespace Bluestroke.Services;

/// <summary>
/// Service for playing keyboard sound effects using NAudio.
/// </summary>
public class AudioService : IDisposable
{
    private readonly Dictionary<SoundPreset, string[]> _soundPaths;
    private readonly List<CachedSound> _soundCache;
    private readonly object _playLock = new();
    private IWavePlayer? _waveOut;
    private MixingSampleProvider? _mixer;
    private float _volume = 0.5f;
    private SoundPreset _currentPreset = SoundPreset.Blue;
    private string? _customSoundFolder;
    private readonly Random _random = new();
    private bool _disposed;

    /// <summary>
    /// Gets or sets the volume (0.0 to 1.0).
    /// </summary>
    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0.0f, 1.0f);
    }

    /// <summary>
    /// Gets or sets the current sound preset.
    /// </summary>
    public SoundPreset CurrentPreset
    {
        get => _currentPreset;
        set
        {
            _currentPreset = value;
            LoadSoundsForPreset();
        }
    }

    /// <summary>
    /// Gets or sets the custom sound folder path.
    /// </summary>
    public string? CustomSoundFolder
    {
        get => _customSoundFolder;
        set
        {
            _customSoundFolder = value;
            if (_currentPreset == SoundPreset.Custom)
            {
                LoadSoundsForPreset();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of AudioService.
    /// </summary>
    public AudioService()
    {
        _soundPaths = new Dictionary<SoundPreset, string[]>();
        _soundCache = new List<CachedSound>();
        InitializeSoundPaths();
        InitializeAudioDevice();
    }

    private void InitializeSoundPaths()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string soundsDir = Path.Combine(baseDir, "Sounds");

        _soundPaths[SoundPreset.Blue] = GetSoundFiles(Path.Combine(soundsDir, "Blue"));
        _soundPaths[SoundPreset.Brown] = GetSoundFiles(Path.Combine(soundsDir, "Brown"));
        _soundPaths[SoundPreset.Red] = GetSoundFiles(Path.Combine(soundsDir, "Red"));
        _soundPaths[SoundPreset.MacBook] = GetSoundFiles(Path.Combine(soundsDir, "MacBook"));
        _soundPaths[SoundPreset.Custom] = Array.Empty<string>();
    }

    private static string[] GetSoundFiles(string folder)
    {
        if (Directory.Exists(folder))
        {
            return Directory.GetFiles(folder, "*.wav");
        }
        return Array.Empty<string>();
    }

    private void InitializeAudioDevice()
    {
        try
        {
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
            _mixer = new MixingSampleProvider(waveFormat)
            {
                ReadFully = true
            };

            _waveOut = new WaveOutEvent();
            _waveOut.Init(_mixer);
            _waveOut.Play();

            LoadSoundsForPreset();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize audio device: {ex.Message}");
        }
    }

    private void LoadSoundsForPreset()
    {
        lock (_playLock)
        {
            _soundCache.Clear();

            string[] paths;
            if (_currentPreset == SoundPreset.Custom && !string.IsNullOrEmpty(_customSoundFolder))
            {
                paths = GetSoundFiles(_customSoundFolder);
            }
            else if (_soundPaths.TryGetValue(_currentPreset, out var presetPaths))
            {
                paths = presetPaths;
            }
            else
            {
                return;
            }

            foreach (string path in paths)
            {
                try
                {
                    var sound = new CachedSound(path);
                    _soundCache.Add(sound);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load sound {path}: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Plays a random sound from the current preset.
    /// </summary>
    public void PlayKeySound()
    {
        if (_mixer == null || _soundCache.Count == 0)
        {
            return;
        }

        lock (_playLock)
        {
            try
            {
                int index = _random.Next(_soundCache.Count);
                var sound = _soundCache[index];
                var sampleProvider = new CachedSoundSampleProvider(sound);
                var volumeProvider = new VolumeSampleProvider(sampleProvider)
                {
                    Volume = _volume
                };
                _mixer.AddMixerInput(volumeProvider);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to play sound: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Disposes the audio service.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the audio service.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _waveOut?.Stop();
                _waveOut?.Dispose();
                _soundCache.Clear();
            }
            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer for AudioService.
    /// </summary>
    ~AudioService()
    {
        Dispose(false);
    }
}

/// <summary>
/// Represents a cached audio sample.
/// </summary>
internal class CachedSound
{
    public float[] AudioData { get; }
    public WaveFormat WaveFormat { get; }

    public CachedSound(string audioFileName)
    {
        using var audioFileReader = new AudioFileReader(audioFileName);
        WaveFormat = audioFileReader.WaveFormat;

        var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
        var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
        int samplesRead;
        while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
        {
            wholeFile.AddRange(readBuffer.Take(samplesRead));
        }
        AudioData = wholeFile.ToArray();
    }
}

/// <summary>
/// Sample provider for cached sounds.
/// </summary>
internal class CachedSoundSampleProvider : ISampleProvider
{
    private readonly CachedSound _cachedSound;
    private long _position;

    public CachedSoundSampleProvider(CachedSound cachedSound)
    {
        _cachedSound = cachedSound;
    }

    public WaveFormat WaveFormat => _cachedSound.WaveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        var availableSamples = _cachedSound.AudioData.Length - _position;
        var samplesToCopy = Math.Min(availableSamples, count);
        Array.Copy(_cachedSound.AudioData, _position, buffer, offset, samplesToCopy);
        _position += samplesToCopy;
        return (int)samplesToCopy;
    }
}
