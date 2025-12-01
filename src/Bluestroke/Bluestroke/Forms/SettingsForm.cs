using Bluestroke.Models;
using Bluestroke.Services;

namespace Bluestroke.Forms;

/// <summary>
/// Cyberpunk-styled settings form for Bluestroke configuration.
/// </summary>
public class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly AudioService _audioService;
    private TrackBar? _volumeSlider;
    private ComboBox? _presetCombo;
    private TextBox? _customFolderText;
    private CheckBox? _enabledCheck;
    private CheckBox? _startWithWindowsCheck;
    private Button? _browseButton;
    private Button? _testButton;
    private Button? _saveButton;
    private Button? _cancelButton;

    /// <summary>
    /// Event raised when settings are changed and saved.
    /// </summary>
    public event EventHandler<AppSettings>? SettingsChanged;

    // Cyberpunk color scheme
    private static readonly Color BackgroundColor = Color.FromArgb(20, 20, 30);
    private static readonly Color PanelColor = Color.FromArgb(30, 30, 45);
    private static readonly Color AccentColor = Color.FromArgb(0, 255, 255);
    private static readonly Color SecondaryAccent = Color.FromArgb(255, 0, 128);
    private static readonly Color TextColor = Color.FromArgb(220, 220, 220);
    private static readonly Color ButtonHoverColor = Color.FromArgb(50, 50, 70);

    /// <summary>
    /// Initializes the settings form.
    /// </summary>
    public SettingsForm(AppSettings settings, AudioService audioService)
    {
        _settings = new AppSettings
        {
            IsEnabled = settings.IsEnabled,
            SelectedPreset = settings.SelectedPreset,
            Volume = settings.Volume,
            CustomSoundFolder = settings.CustomSoundFolder,
            StartWithWindows = settings.StartWithWindows
        };
        _audioService = audioService;

        InitializeComponent();
        LoadSettingsToUI();
    }

    private void InitializeComponent()
    {
        Text = "Bluestroke Settings";
        Size = new Size(480, 420);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = BackgroundColor;
        ForeColor = TextColor;
        Font = new Font("Segoe UI", 10F);

        // Title label with neon effect
        var titleLabel = new Label
        {
            Text = "⌨ BLUESTROKE",
            Font = new Font("Segoe UI", 18F, FontStyle.Bold),
            ForeColor = AccentColor,
            Location = new Point(20, 15),
            Size = new Size(250, 35),
            AutoSize = false
        };

        // Subtitle
        var subtitleLabel = new Label
        {
            Text = "KEYBOARD SOUND ENHANCER",
            Font = new Font("Segoe UI", 8F),
            ForeColor = SecondaryAccent,
            Location = new Point(20, 50),
            Size = new Size(200, 20)
        };

        // Main panel with border effect
        var mainPanel = new Panel
        {
            Location = new Point(20, 80),
            Size = new Size(435, 250),
            BackColor = PanelColor
        };
        mainPanel.Paint += (s, e) =>
        {
            using var pen = new Pen(AccentColor, 2);
            e.Graphics.DrawRectangle(pen, 0, 0, mainPanel.Width - 1, mainPanel.Height - 1);
        };

        // Enable checkbox
        _enabledCheck = CreateStyledCheckBox("Enable Keyboard Sounds", 15, 15);
        mainPanel.Controls.Add(_enabledCheck);

        // Preset selection
        var presetLabel = CreateStyledLabel("Sound Preset:", 15, 50);
        mainPanel.Controls.Add(presetLabel);

        _presetCombo = new ComboBox
        {
            Location = new Point(150, 47),
            Size = new Size(150, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = BackgroundColor,
            ForeColor = AccentColor,
            FlatStyle = FlatStyle.Flat
        };
        foreach (SoundPreset preset in Enum.GetValues<SoundPreset>())
        {
            _presetCombo.Items.Add(preset);
        }
        _presetCombo.SelectedIndexChanged += OnPresetChanged;
        mainPanel.Controls.Add(_presetCombo);

        // Volume slider
        var volumeLabel = CreateStyledLabel("Volume:", 15, 85);
        mainPanel.Controls.Add(volumeLabel);

        _volumeSlider = new TrackBar
        {
            Location = new Point(145, 80),
            Size = new Size(200, 45),
            Minimum = 0,
            Maximum = 100,
            TickFrequency = 10,
            BackColor = PanelColor
        };
        _volumeSlider.ValueChanged += OnVolumeChanged;
        mainPanel.Controls.Add(_volumeSlider);

        var volumeValueLabel = new Label
        {
            Name = "volumeValueLabel",
            Location = new Point(350, 85),
            Size = new Size(50, 20),
            ForeColor = AccentColor,
            Text = "50%"
        };
        mainPanel.Controls.Add(volumeValueLabel);

        // Custom folder
        var customLabel = CreateStyledLabel("Custom Sounds:", 15, 130);
        mainPanel.Controls.Add(customLabel);

        _customFolderText = new TextBox
        {
            Location = new Point(150, 127),
            Size = new Size(180, 25),
            BackColor = BackgroundColor,
            ForeColor = TextColor,
            BorderStyle = BorderStyle.FixedSingle
        };
        mainPanel.Controls.Add(_customFolderText);

        _browseButton = CreateStyledButton("...", 335, 126, 50, 27);
        _browseButton.Click += OnBrowseClick;
        mainPanel.Controls.Add(_browseButton);

        // Test button
        _testButton = CreateStyledButton("🔊 Test Sound", 15, 170, 120, 35);
        _testButton.Click += OnTestClick;
        mainPanel.Controls.Add(_testButton);

        // Start with Windows
        _startWithWindowsCheck = CreateStyledCheckBox("Start with Windows", 15, 215);
        mainPanel.Controls.Add(_startWithWindowsCheck);

        // Bottom buttons
        _saveButton = CreateStyledButton("💾 Save", 240, 345, 100, 35);
        _saveButton.Click += OnSaveClick;

        _cancelButton = CreateStyledButton("✖ Cancel", 350, 345, 100, 35);
        _cancelButton.Click += OnCancelClick;

        // Add glitch line decoration
        var glitchLine1 = new Panel
        {
            Location = new Point(0, 75),
            Size = new Size(480, 2),
            BackColor = AccentColor
        };

        var glitchLine2 = new Panel
        {
            Location = new Point(20, 340),
            Size = new Size(435, 1),
            BackColor = SecondaryAccent
        };

        Controls.AddRange(new Control[] 
        { 
            titleLabel, 
            subtitleLabel, 
            mainPanel, 
            _saveButton, 
            _cancelButton,
            glitchLine1,
            glitchLine2
        });
    }

    private static Label CreateStyledLabel(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(130, 25),
            ForeColor = TextColor
        };
    }

    private static CheckBox CreateStyledCheckBox(string text, int x, int y)
    {
        return new CheckBox
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(200, 25),
            ForeColor = AccentColor,
            FlatStyle = FlatStyle.Flat
        };
    }

    private static Button CreateStyledButton(string text, int x, int y, int width, int height)
    {
        var btn = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(width, height),
            BackColor = PanelColor,
            ForeColor = AccentColor,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderColor = AccentColor;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.MouseOverBackColor = ButtonHoverColor;
        return btn;
    }

    private void LoadSettingsToUI()
    {
        _enabledCheck!.Checked = _settings.IsEnabled;
        _presetCombo!.SelectedItem = _settings.SelectedPreset;
        _volumeSlider!.Value = (int)(_settings.Volume * 100);
        UpdateVolumeLabel();
        _customFolderText!.Text = _settings.CustomSoundFolder ?? string.Empty;
        _startWithWindowsCheck!.Checked = _settings.StartWithWindows;
        UpdateCustomFolderVisibility();
    }

    private void UpdateVolumeLabel()
    {
        var volumeLabel = Controls.Find("volumeValueLabel", true).FirstOrDefault() as Label;
        if (volumeLabel == null)
        {
            foreach (Control ctrl in Controls)
            {
                if (ctrl is Panel panel)
                {
                    foreach (Control panelCtrl in panel.Controls)
                    {
                        if (panelCtrl is Label lbl && lbl.Name == "volumeValueLabel")
                        {
                            lbl.Text = $"{_volumeSlider!.Value}%";
                            return;
                        }
                    }
                }
            }
        }
        else
        {
            volumeLabel.Text = $"{_volumeSlider!.Value}%";
        }
    }

    private void UpdateCustomFolderVisibility()
    {
        bool isCustom = _presetCombo!.SelectedItem is SoundPreset preset && preset == SoundPreset.Custom;
        _customFolderText!.Enabled = isCustom;
        _browseButton!.Enabled = isCustom;
    }

    private void OnPresetChanged(object? sender, EventArgs e)
    {
        UpdateCustomFolderVisibility();
    }

    private void OnVolumeChanged(object? sender, EventArgs e)
    {
        UpdateVolumeLabel();
    }

    private void OnBrowseClick(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Select folder containing .wav sound files",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _customFolderText!.Text = dialog.SelectedPath;
        }
    }

    private void OnTestClick(object? sender, EventArgs e)
    {
        _audioService.PlayKeySound();
    }

    private void OnSaveClick(object? sender, EventArgs e)
    {
        _settings.IsEnabled = _enabledCheck!.Checked;
        _settings.SelectedPreset = (SoundPreset)_presetCombo!.SelectedItem!;
        _settings.Volume = _volumeSlider!.Value / 100.0f;
        _settings.CustomSoundFolder = string.IsNullOrWhiteSpace(_customFolderText!.Text) 
            ? null 
            : _customFolderText.Text;
        _settings.StartWithWindows = _startWithWindowsCheck!.Checked;

        SettingsChanged?.Invoke(this, _settings);
        Close();
    }

    private void OnCancelClick(object? sender, EventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Override to add custom painting for glitch effects.
    /// </summary>
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        // Add subtle scan lines effect
        using var pen = new Pen(Color.FromArgb(10, 0, 255, 255), 1);
        for (int y = 0; y < Height; y += 4)
        {
            e.Graphics.DrawLine(pen, 0, y, Width, y);
        }
    }
}
