using System.Runtime.InteropServices;
using Bluestroke.Models;
using Bluestroke.Services;

namespace Bluestroke.Forms;

/// <summary>
/// Main application form that manages the system tray icon and context menu.
/// </summary>
public partial class TrayApplicationContext : ApplicationContext
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

    private readonly NotifyIcon _trayIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly KeyboardHookService _keyboardHook;
    private readonly AudioService _audioService;
    private readonly SettingsService _settingsService;
    private AppSettings _settings;
    private SettingsForm? _settingsForm;

    /// <summary>
    /// Initializes the tray application context.
    /// </summary>
    public TrayApplicationContext()
    {
        _settingsService = new SettingsService();
        _settings = _settingsService.LoadSettings();
        _audioService = new AudioService
        {
            Volume = _settings.Volume,
            CurrentPreset = _settings.SelectedPreset,
            CustomSoundFolder = _settings.CustomSoundFolder
        };

        _keyboardHook = new KeyboardHookService();
        _keyboardHook.KeyPressed += OnKeyPressed;

        _contextMenu = CreateContextMenu();

        _trayIcon = new NotifyIcon
        {
            Icon = CreateDefaultIcon(),
            Text = "Bluestroke",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };

        _trayIcon.DoubleClick += OnTrayIconDoubleClick;

        if (_settings.IsEnabled)
        {
            _keyboardHook.Start();
        }

        UpdateMenuState();
    }

    private static Icon CreateDefaultIcon()
    {
        // Create a simple 16x16 icon with a keyboard-like design
        using var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.FromArgb(0, 255, 255)); // Cyan background
        
        using var darkBrush = new SolidBrush(Color.FromArgb(30, 30, 30));
        g.FillRectangle(darkBrush, 2, 4, 12, 8);
        
        using var cyanPen = new Pen(Color.FromArgb(0, 255, 255));
        g.DrawRectangle(cyanPen, 2, 4, 11, 7);
        
        // Draw small "key" squares
        using var keyBrush = new SolidBrush(Color.FromArgb(0, 255, 255));
        g.FillRectangle(keyBrush, 3, 5, 2, 2);
        g.FillRectangle(keyBrush, 6, 5, 2, 2);
        g.FillRectangle(keyBrush, 9, 5, 2, 2);
        g.FillRectangle(keyBrush, 4, 8, 6, 2);

        IntPtr hIcon = bitmap.GetHicon();
        try
        {
            // Clone the icon so we can safely destroy the original handle
            var tempIcon = Icon.FromHandle(hIcon);
            var clonedIcon = (Icon)tempIcon.Clone();
            return clonedIcon;
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.BackColor = Color.FromArgb(30, 30, 40);
        menu.ForeColor = Color.FromArgb(0, 255, 255);
        menu.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        menu.Renderer = new CyberpunkMenuRenderer();

        // Enable/Disable toggle
        var enableItem = new ToolStripMenuItem("Enable Sounds")
        {
            Name = "enableItem",
            CheckOnClick = true,
            Checked = _settings.IsEnabled
        };
        enableItem.Click += OnEnableClick;
        menu.Items.Add(enableItem);

        menu.Items.Add(new ToolStripSeparator());

        // Sound preset submenu
        var presetMenu = new ToolStripMenuItem("Sound Preset") { Name = "presetMenu" };
        foreach (SoundPreset preset in Enum.GetValues<SoundPreset>())
        {
            var presetItem = new ToolStripMenuItem(preset.ToString())
            {
                Tag = preset,
                Checked = preset == _settings.SelectedPreset
            };
            presetItem.Click += OnPresetClick;
            presetMenu.DropDownItems.Add(presetItem);
        }
        menu.Items.Add(presetMenu);

        // Volume submenu
        var volumeMenu = new ToolStripMenuItem("Volume") { Name = "volumeMenu" };
        int[] volumeLevels = { 10, 25, 50, 75, 100 };
        foreach (int level in volumeLevels)
        {
            var volumeItem = new ToolStripMenuItem($"{level}%")
            {
                Tag = level / 100.0f,
                Checked = Math.Abs(_settings.Volume - level / 100.0f) < 0.05f
            };
            volumeItem.Click += OnVolumeClick;
            volumeMenu.DropDownItems.Add(volumeItem);
        }
        menu.Items.Add(volumeMenu);

        menu.Items.Add(new ToolStripSeparator());

        // Settings
        var settingsItem = new ToolStripMenuItem("Settings...");
        settingsItem.Click += OnSettingsClick;
        menu.Items.Add(settingsItem);

        menu.Items.Add(new ToolStripSeparator());

        // Exit
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += OnExitClick;
        menu.Items.Add(exitItem);

        return menu;
    }

    private void OnKeyPressed(object? sender, KeyPressedEventArgs e)
    {
        if (_settings.IsEnabled)
        {
            _audioService.PlayKeySound();
        }
    }

    private void OnEnableClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item)
        {
            _settings.IsEnabled = item.Checked;
            if (_settings.IsEnabled)
            {
                _keyboardHook.Start();
            }
            else
            {
                _keyboardHook.Stop();
            }
            _settingsService.SaveSettings(_settings);
        }
    }

    private void OnPresetClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item && item.Tag is SoundPreset preset)
        {
            _settings.SelectedPreset = preset;
            _audioService.CurrentPreset = preset;
            UpdatePresetMenuChecks(preset);
            _settingsService.SaveSettings(_settings);
        }
    }

    private void OnVolumeClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem item && item.Tag is float volume)
        {
            _settings.Volume = volume;
            _audioService.Volume = volume;
            UpdateVolumeMenuChecks(volume);
            _settingsService.SaveSettings(_settings);
        }
    }

    private void OnSettingsClick(object? sender, EventArgs e)
    {
        if (_settingsForm == null || _settingsForm.IsDisposed)
        {
            _settingsForm = new SettingsForm(_settings, _audioService);
            _settingsForm.SettingsChanged += OnSettingsFormChanged;
            _settingsForm.FormClosed += (s, args) => _settingsForm = null;
            _settingsForm.Show();
        }
        else
        {
            _settingsForm.BringToFront();
            _settingsForm.Focus();
        }
    }

    private void OnSettingsFormChanged(object? sender, AppSettings newSettings)
    {
        _settings = newSettings;
        _audioService.Volume = _settings.Volume;
        _audioService.CurrentPreset = _settings.SelectedPreset;
        _audioService.CustomSoundFolder = _settings.CustomSoundFolder;
        _settingsService.SaveSettings(_settings);
        UpdateMenuState();
    }

    private void OnTrayIconDoubleClick(object? sender, EventArgs e)
    {
        OnSettingsClick(sender, e);
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        Shutdown();
    }

    private void UpdateMenuState()
    {
        if (_contextMenu.Items["enableItem"] is ToolStripMenuItem enableItem)
        {
            enableItem.Checked = _settings.IsEnabled;
        }

        UpdatePresetMenuChecks(_settings.SelectedPreset);
        UpdateVolumeMenuChecks(_settings.Volume);
    }

    private void UpdatePresetMenuChecks(SoundPreset currentPreset)
    {
        if (_contextMenu.Items["presetMenu"] is ToolStripMenuItem presetMenu)
        {
            foreach (ToolStripMenuItem item in presetMenu.DropDownItems.OfType<ToolStripMenuItem>())
            {
                if (item.Tag is SoundPreset preset)
                {
                    item.Checked = preset == currentPreset;
                }
            }
        }
    }

    private void UpdateVolumeMenuChecks(float currentVolume)
    {
        if (_contextMenu.Items["volumeMenu"] is ToolStripMenuItem volumeMenu)
        {
            foreach (ToolStripMenuItem item in volumeMenu.DropDownItems.OfType<ToolStripMenuItem>())
            {
                if (item.Tag is float volume)
                {
                    item.Checked = Math.Abs(volume - currentVolume) < 0.05f;
                }
            }
        }
    }

    private void Shutdown()
    {
        _keyboardHook.Stop();
        _keyboardHook.Dispose();
        _audioService.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _keyboardHook.Dispose();
            _audioService.Dispose();
            _trayIcon.Dispose();
            _contextMenu.Dispose();
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Custom renderer for cyberpunk-style context menu.
/// </summary>
internal class CyberpunkMenuRenderer : ToolStripProfessionalRenderer
{
    public CyberpunkMenuRenderer() : base(new CyberpunkColorTable()) { }

    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
    {
        if (e.Item.Selected)
        {
            using var brush = new SolidBrush(Color.FromArgb(60, 0, 255, 255));
            e.Graphics.FillRectangle(brush, e.Item.ContentRectangle);

            using var pen = new Pen(Color.FromArgb(0, 255, 255), 1);
            e.Graphics.DrawRectangle(pen, e.Item.ContentRectangle.X, e.Item.ContentRectangle.Y,
                e.Item.ContentRectangle.Width - 1, e.Item.ContentRectangle.Height - 1);
        }
        else
        {
            base.OnRenderMenuItemBackground(e);
        }
    }

    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
    {
        using var pen = new Pen(Color.FromArgb(0, 255, 255, 128), 1);
        int y = e.Item.ContentRectangle.Height / 2;
        e.Graphics.DrawLine(pen, 0, y, e.Item.Width, y);
    }
}

/// <summary>
/// Color table for cyberpunk menu theme.
/// </summary>
internal class CyberpunkColorTable : ProfessionalColorTable
{
    public override Color MenuBorder => Color.FromArgb(0, 255, 255);
    public override Color MenuItemBorder => Color.FromArgb(0, 255, 255);
    public override Color MenuItemSelected => Color.FromArgb(60, 0, 255, 255);
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(40, 40, 50);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(40, 40, 50);
    public override Color MenuStripGradientBegin => Color.FromArgb(30, 30, 40);
    public override Color MenuStripGradientEnd => Color.FromArgb(30, 30, 40);
    public override Color ToolStripDropDownBackground => Color.FromArgb(30, 30, 40);
    public override Color ImageMarginGradientBegin => Color.FromArgb(30, 30, 40);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(30, 30, 40);
    public override Color ImageMarginGradientEnd => Color.FromArgb(30, 30, 40);
}
