## Quick Start Guide for AMC Clicker

### First Time Setup

1. **Open Terminal/Command Prompt** in the project directory

2. **Build the project**:
   ```bash
   dotnet build -c Release
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

### First Test Run

1. The AMC Clicker window will open
2. Keep default settings (10 clicks/sec, 100ms delay, 10 clicks)
3. Click on a text editor (like Notepad) to focus it
4. Switch back to AMC Clicker window
5. Press F6 to start clicking - you should see text appearing
6. Press F7 to stop

### Recording Test

1. Open a simple drawing app or click counter
2. Press F8 to start recording
3. Click a few times (the app records your mouse position)
4. Press F9 to stop recording
5. Press F10 to playback your recorded clicks in sequence

### Tips

- Always make sure the window you want to click in is accessible
- The app works globally - even when another window is in focus
- Use ESC to quickly exit if something goes wrong
- Recorded clicks include timing information for accurate playback

### Troubleshooting

**App won't click**:
- Make sure you click in the target window after starting
- Check that the window isn't capturing all mouse input

**Hotkeys not working**:
- Some applications capture global hotkeys
- Try running as Administrator

**Recording not capturing clicks**:
- Make sure clicks happen after you press F8
- The app records physical mouse clicks

### Next Steps

To customize the app:
1. Edit Program.cs to change default values
2. Modify the UI layout in InitializeComponent()
3. Add new features to AutoClickerService
4. Rebuild with `dotnet build -c Release`

### Building a Release

```bash
dotnet publish -c Release -o ./dist
```

This creates a standalone executable in the `dist` folder.
