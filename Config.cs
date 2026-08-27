using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

// Default Configuration for AMC Clicker
// You can modify these values to customize the application behavior

namespace AMCClicker.Config
{
    public static class AppConfig
    {
        // UI Settings
        public static readonly int DefaultWindowWidth = 600;
        public static readonly int DefaultWindowHeight = 750;
        public static readonly string ApplicationTitle = "AMC Clicker";

        // Clicking Settings
        public static readonly int DefaultClicksPerSecond = 10;
        public static readonly int DefaultDelayMs = 100;
        public static readonly int DefaultNumClicks = 100;
        
        // UI Constraints
        public static readonly int MinClicksPerSecond = 1;
        public static readonly int MaxClicksPerSecond = 100;
        public static readonly int MinDelayMs = 10;
        public static readonly int MaxDelayMs = 5000;
        public static readonly int MaxNumClicks = 10000;

        // Recording Settings
        public static readonly int ClickDebounceMs = 100; // Minimum time between recorded clicks
        public static readonly int ClickMovementDelayMs = 50; // Delay when moving to recorded position before clicking

        // Hotkey Settings
        public static readonly bool EnableGlobalHotkeys = true;

        // Application Behavior
        public static readonly bool ShowSystemTrayIcon = false;
        public static readonly bool AlwaysOnTop = false;
        public static readonly bool MinimizeToTray = false;
    }

    public class HotkeySettings
    {
        public Keys StartClickingHotkey { get; set; } = Keys.F6;
        public Keys StopClickingHotkey { get; set; } = Keys.F7;
        public Keys StartRecordingHotkey { get; set; } = Keys.F8;
        public Keys StopRecordingHotkey { get; set; } = Keys.F9;
        public Keys PlaybackRecordingHotkey { get; set; } = Keys.F10;
        public Keys ExitApplicationHotkey { get; set; } = Keys.Escape;
    }

    public class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");
        private static HotkeySettings _hotkeySettings;

        public static HotkeySettings LoadHotkeySettings()
        {
            if (_hotkeySettings != null)
                return _hotkeySettings;

            if (File.Exists(SettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsPath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _hotkeySettings = JsonSerializer.Deserialize<HotkeySettings>(json, options);
                    return _hotkeySettings;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading settings: {ex.Message}. Using defaults.");
                }
            }

            _hotkeySettings = new HotkeySettings();
            return _hotkeySettings;
        }

        public static void SaveHotkeySettings(HotkeySettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsPath, json);
                _hotkeySettings = settings;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error");
            }
        }
    }
}
