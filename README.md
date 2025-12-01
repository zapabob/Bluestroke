# Bluestroke

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A lightweight Windows application that plays custom keyboard sounds with a cyberpunk-inspired interface.

## Features

### 🎹 Keystroke Hook
- Real-time keyboard input capture using Windows low-level keyboard hooks
- Safe thread handling and asynchronous processing
- Minimal system impact

### 🔊 Audio Playback
- High-precision audio playback powered by NAudio
- Multiple sound presets:
  - **Blue Switch** - Clicky mechanical keyboard sound
  - **Brown Switch** - Tactile mechanical keyboard sound
  - **Red Switch** - Linear mechanical keyboard sound
  - **MacBook** - Apple keyboard style sound
  - **Custom** - User-defined sounds

### 📌 System Tray Integration
- Runs silently in the system tray
- No persistent window cluttering your desktop
- Quick access via right-click context menu:
  - Enable/Disable sounds
  - Switch sound presets
  - Adjust volume
  - Open settings

### 🎨 Cyberpunk UI
- Neon color scheme with cyan accents
- Glitch-style decorative elements
- Scanline visual effects
- Dark theme optimized for low-light environments

### 📁 Custom Sounds
- Import your own `.wav` files
- Organize sounds in custom folders
- Random sound selection from multiple files

### ⚙️ Settings Persistence
- JSON-based configuration storage
- Automatic settings restoration on startup
- Settings stored in user's AppData folder

## Requirements

- Windows 10/11
- .NET 8.0 Runtime (Windows Desktop)

## Installation

### From Source

1. Clone the repository:
   ```bash
   git clone https://github.com/zapabob/Bluestroke.git
   cd Bluestroke
   ```

2. Build the solution:
   ```bash
   dotnet build --configuration Release
   ```

3. Run the application:
   ```bash
   dotnet run --project src/Bluestroke/Bluestroke/Bluestroke.csproj
   ```

### Pre-built Release

Download the latest release from the [Releases](https://github.com/zapabob/Bluestroke/releases) page.

## Usage

1. **Start the Application**: Double-click `Bluestroke.exe` or run from command line
2. **Access from System Tray**: Look for the Bluestroke icon in your system tray
3. **Configure Settings**: Right-click the tray icon to access options
4. **Change Sound Preset**: Select from Blue, Brown, Red, MacBook, or Custom presets
5. **Adjust Volume**: Choose from 10%, 25%, 50%, 75%, or 100% volume levels
6. **Open Settings Form**: Double-click the tray icon or select "Settings..."

## Adding Custom Sounds

1. Create a folder with your `.wav` sound files
2. Open Bluestroke settings
3. Select "Custom" as your sound preset
4. Browse to your custom sounds folder
5. Click Save

### Sound File Requirements

- **Format**: WAV (PCM)
- **Sample Rate**: 44100 Hz (recommended)
- **Channels**: Stereo (2 channels) preferred
- **Bit Depth**: 16-bit or 32-bit float
- **Duration**: Short sounds (< 500ms recommended)

## Project Structure

```
Bluestroke/
├── src/
│   └── Bluestroke/
│       └── Bluestroke/
│           ├── Forms/           # UI forms and application context
│           ├── Models/          # Data models and settings
│           ├── Services/        # Core services (audio, keyboard hook, settings)
│           ├── Sounds/          # Sound preset folders
│           └── Program.cs       # Application entry point
├── tests/
│   └── Bluestroke.Tests/       # Unit tests
└── Bluestroke.sln              # Solution file
```

## Development

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- Windows 10/11 (for running and testing)

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

Note: Tests require Windows to run due to Windows Forms dependencies.

### Publishing

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Dependencies

- [NAudio](https://github.com/naudio/NAudio) - Audio playback library
- [Newtonsoft.Json](https://www.newtonsoft.com/json) - JSON serialization

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## Acknowledgments

- Inspired by the satisfying sound of mechanical keyboards
- Cyberpunk aesthetic inspired by modern UI design trends