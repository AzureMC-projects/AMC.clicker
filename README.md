# AMC Clicker

A feature-rich auto clicker application built with C# and Windows Forms. Perfect for automating repetitive clicking tasks.

## Features

- ⚡ **Auto Clicking**: Start/stop automated clicking with customizable click rate and number of clicks
- 🎛️ **Adjustable Settings**: Control clicks per second, delay between clicks, and total number of clicks
- ⌨️ **Global Hotkeys**: Use F6-F10 and ESC for quick control without focusing the window
- 🎥 **Click Recording**: Record mouse clicks with precise coordinates and timing
- ▶️ **Playback**: Play back recorded click sequences with timing preserved
- 📊 **Real-time Status**: See what the app is doing at all times

## Hotkey Controls

- **F6** - Start Clicking
- **F7** - Stop Clicking  
- **F8** - Start Recording Clicks
- **F9** - Stop Recording
- **F10** - Playback Recording
- **ESC** - Exit Application

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

## Usage

1. **Basic Auto Clicking**:
   - Set "Clicks per second" (default: 10)
   - Set "Number of clicks" (0 = infinite)
   - Press F6 or click "Start Clicking"
   - Press F7 or click "Stop Clicking" to stop

2. **Recording Clicks**:
   - Press F8 to start recording
   - Click the mouse to record positions
   - Press F9 to stop recording
   - The app records coordinates and timing between clicks

3. **Playback Recording**:
   - After recording, press F10 to playback
   - The app will move to each recorded position and click

## Configuration

Edit the default values in the UI:
- **Clicks per second**: How fast to click (1-100)
- **Delay between clicks**: Time in milliseconds between clicks (10-5000ms)
- **Number of clicks**: Total clicks to perform (0 = infinite until manually stopped)

## ⚠️ Safety Features & Warnings

- Use responsibly - unauthorized automation may violate terms of service for some applications
- Always have manual control available to stop the clicker
- Test with small numbers of clicks first
- The ESC key exits the application immediately

## Project Structure

```
AMCClicker/
├── AMCClicker.csproj      # Project file
├── Program.cs             # Main application code
├── README.md              # This file
└── .gitignore            # Git ignore file
```

## License

Free to use and modify for personal use.