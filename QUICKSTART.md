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

### First Look - UI Tour

The app features a **dark PlayStation 5-inspired design**:
- **Dark background** with cyan accents
- **Two tabs**: "Clicker" (main controls) and "Settings" (hotkey customization)
- **Color-coded status** indicator showing app state
- **Organized sections** for settings, actions, and recording

### First Test Run

1. The AMC Clicker window will open with the modern dark UI
2. Keep default settings (10 clicks/sec, 100ms delay, 10 clicks)
3. Click on a text editor (like Notepad) to focus it
4. Switch back to AMC Clicker window
5. Click **"START CLICKING"** button - you should see text appearing
6. Click **"STOP"** button to stop

### Recording Test

1. Open a simple drawing app or click counter
2. Click **"START RECORDING"** button
3. Click a few times (the app records your mouse position)
4. Click **"STOP"** button to finish
5. Click **"PLAYBACK"** to play back your recorded clicks in sequence

### Customizing Hotkeys

1. Click the **"Settings"** tab
2. You'll see dropdowns for each hotkey
3. Change any hotkey from F1-F12 or ESC
4. Click **"SAVE SETTINGS"** 
5. Your new hotkeys are immediately active!
6. Use **"RESET TO DEFAULTS"** to restore original F6-F10 keys

### Understanding the Status Indicator

The colored status box at the top shows:
- 🟢 **Green**: Running or successful
- 🔴 **Red**: Stopped or error
- 🟡 **Orange**: Transitioning
- ⚪ **Gray**: Informational

### Tips

- The app uses a **dark theme** for reduced eye strain
- **Cyan buttons** are primary actions (Start, Record)
- **Red buttons** are stop actions
- **Green buttons** are playback/positive actions
- All settings are saved to `settings.json`

### Troubleshooting

**App won't click**:
- Make sure you click in the target window after starting
- Check that the window isn't capturing all mouse input

**Hotkeys not working**:
- Make sure you saved hotkey settings
- Some applications capture global hotkeys
- Try running as Administrator

**Recording not capturing clicks**:
- Make sure clicks happen after you click "START RECORDING"
- The app records physical mouse clicks

### Next Steps

To customize the app further:
1. Edit `Program.cs` to change default values
2. Modify colors in the color constants (PS5_Accent, PS5_DarkBg, etc.)
3. Rebuild with `dotnet build -c Release`
4. Create a standalone EXE: `dotnet publish -c Release -o ./dist`

### Building a Release

```bash
dotnet publish -c Release -o ./dist
```

This creates a standalone executable in the `dist` folder that works on any Windows PC without needing .NET installed!

### Sharing Your Build

1. Build the release: `dotnet publish -c Release -o ./dist`
2. Send the entire `dist` folder to others
3. They just double-click `AMCClicker.exe` to run it
4. No installation or dependencies needed!
