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
    private bool _audioInitialized;
    private string? _lastError;

    /// <summary>
    /// Gets or sets the volume (0.0 to 1.0).
    /// </summary>
    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0.0f, 1.0f);
    }

    /// <summary>
    /// Gets whether audio is properly initialized.
    /// </summary>
    public bool IsAudioInitialized => _audioInitialized;

    /// <summary>
    /// Gets the last error message, if any.
    /// </summary>
    public string? LastError => _lastError;

    /// <summary>
    /// Gets the count of loaded sounds in the cache.
    /// </summary>
    public int LoadedSoundCount => _soundCache.Count;

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
            var files = Directory.GetFiles(folder, "*.wav");
            System.Diagnostics.Debug.WriteLine($"Found {files.Length} WAV files in {folder}");
            return files;
        }
        System.Diagnostics.Debug.WriteLine($"Sound folder not found: {folder}");
        return Array.Empty<string>();
    }

    private void InitializeAudioDevice()
    {
        try
        {
            // Check if any audio output devices are available
            if (WaveOut.DeviceCount == 0)
            {
                _lastError = "No audio output devices found";
                System.Diagnostics.Debug.WriteLine(_lastError);
                return;
            }

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
            _mixer = new MixingSampleProvider(waveFormat)
            {
                ReadFully = true
            };

            _waveOut = new WaveOutEvent();
            _waveOut.Init(_mixer);
            _waveOut.Play();

            _audioInitialized = true;
            System.Diagnostics.Debug.WriteLine("Audio device initialized successfully");
            
            LoadSoundsForPreset();
        }
        catch (NAudio.MmException ex)
        {
            _lastError = $"Audio device error: {ex.Message}. Result: {ex.Result}";
            System.Diagnostics.Debug.WriteLine(_lastError);
        }
        catch (Exception ex)
        {
            _lastError = $"Failed to initialize audio device: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(_lastError);
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
                _lastError = $"No sound paths found for preset: {_currentPreset}";
                System.Diagnostics.Debug.WriteLine(_lastError);
                return;
            }

            if (paths.Length == 0)
            {
                _lastError = $"No sound files found for preset: {_currentPreset}";
                System.Diagnostics.Debug.WriteLine(_lastError);
                return;
            }

            foreach (string path in paths)
            {
                try
                {
                    var sound = new CachedSound(path);
                    _soundCache.Add(sound);
                    System.Diagnostics.Debug.WriteLine($"Loaded sound: {Path.GetFileName(path)}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load sound {path}: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Loaded {_soundCache.Count} sounds for preset: {_currentPreset}");
        }
    }

    /// <summary>
    /// Plays a random sound from the current preset.
    /// </summary>
    public void PlayKeySound()
    {
        if (!_audioInitialized)
        {
            System.Diagnostics.Debug.WriteLine("Audio not initialized, cannot play sound");
            return;
        }

        if (_mixer == null || _soundCache.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine($"Cannot play sound: mixer={_mixer != null}, soundCache={_soundCache.Count}");
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
        var availableSamples = Math.Max(0, _cachedSound.AudioData.Length - _position);
        var samplesToCopy = Math.Min(availableSamples, count);
        if (samplesToCopy > 0)
        {
            Array.Copy(_cachedSound.AudioData, _position, buffer, offset, samplesToCopy);
            _position += samplesToCopy;
        }
        return (int)samplesToCopy;
    }
}
