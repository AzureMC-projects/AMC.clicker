# AMC Clicker

A sleek, modern auto clicker application built with C# and Windows Forms, inspired by PlayStation 5's clean UI design. Perfect for automating repetitive clicking tasks with style.

## Features

- ✨ **Modern Dark UI**: PlayStation 5-inspired design with dark theme and cyan accents
- ⚡ **Auto Clicking**: Start/stop automated clicking with customizable click rate and number of clicks
- 🎛️ **Adjustable Settings**: Control clicks per second, delay between clicks, and total number of clicks
- ⌨️ **Customizable Hotkeys**: Fully customizable hotkey bindings (F1-F12, ESC)
- 🎥 **Click Recording**: Record mouse clicks with precise coordinates and timing
- ▶️ **Playback**: Play back recorded click sequences with timing preserved
- 📊 **Real-time Status**: Live status indicator showing app state with color-coded feedback
- 🎨 **Clean Interface**: Organized into Clicker and Settings tabs for easy navigation

## UI Design

The application features a **PlayStation 5-inspired dark theme** with:
- Deep dark background (#0A0E27)
- Cyan accent colors (#00B4DC)
- Clean Segoe UI typography
- Well-organized sections and cards
- Modern flat button design
- Color-coded status indicators (Green = Running, Red = Stopped, etc.)

## Customizable Hotkeys

Fully customize each hotkey from the Settings tab:
- **Start Clicking** - Default: F6
- **Stop Clicking** - Default: F7
- **Start Recording** - Default: F8
- **Stop Recording** - Default: F9
- **Playback Recording** - Default: F10
- **Exit Application** - Default: ESC

Available keys: F1-F12, ESC

## Building the Project

### Prerequisites
- .NET 8 SDK or later
- Windows 10/11

### Build Steps

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build -c Release

# Run the application
dotnet run
```

### Create Standalone EXE

```bash
dotnet publish -c Release -o ./dist
# EXE will be in: dist/AMCClicker.exe
```

## Usage

### Basic Auto Clicking

1. Open the app
2. Adjust settings:
   - **Clicks per second**: How fast to click (1-100)
   - **Delay (ms)**: Time between clicks (10-5000ms)
   - **Number of clicks**: Total clicks (0 = infinite)
3. Click **START CLICKING** button or press default hotkey
4. Click **STOP** to halt clicking

### Recording Clicks

1. Click **START RECORDING** button
2. Click your mouse to record positions and timing
3. Click **STOP** button to finish recording
4. You'll see "Recorded Clicks: X" display

### Playback Recordings

1. After recording, click **PLAYBACK** button
2. The app moves to each recorded position and clicks
3. Timing between clicks is preserved from recording

### Customizing Hotkeys

1. Go to **Settings** tab
2. Choose new hotkey for each action (F1-F12, ESC)
3. Click **SAVE SETTINGS**
4. Hotkeys are immediately active
5. Use **RESET TO DEFAULTS** to restore original keys

## Status Indicators

- 🟢 **Green**: Running or successful action
- 🔴 **Red**: Stopped or error state
- 🟡 **Yellow/Orange**: Transitional state
- ⚪ **Muted Gray**: Informational text

## ⚠️ Safety & Warnings

- Use responsibly - unauthorized automation may violate terms of service for some applications
- Always have manual control available to stop the clicker
- Test with small numbers of clicks first
- The ESC key exits the application immediately
- Recording accuracy depends on system performance

## File Structure

```
AMCClicker/
├── AMCClicker.csproj      # Project configuration
├── Program.cs             # Main application (UI + logic)
├── Config.cs              # Settings and configuration
├── README.md              # This file
├── QUICKSTART.md          # Quick start guide
├── .gitignore            # Git ignore file
└── dist/                 # Release builds (generated)
    └── AMCClicker.exe    # Standalone executable
```

## Changelog

### v1.0.0 (Latest)
- ✨ Complete UI redesign with PlayStation 5-inspired dark theme
- ✨ Full hotkey customization system
- ⚡ Improved performance and stability
- 🐛 Bug fixes and code cleanup
- 📝 Enhanced documentation

## License

Free to use and modify for personal use.

## Support

For issues or feature requests, visit the GitHub repository.