using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AMCClicker.Config;

namespace AMCClicker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }

    public partial class MainForm : Form
    {
        private AutoClickerService autoClickerService;
        private AutoClickerRecorder recorder;
        private List<ClickRecord> recordedClicks;
        private bool isRecording = false;
        private bool isPlaying = false;
        private HotkeySettings hotkeySettings;
        private TabControl tabControl;
        private Label statusLabel;

        public MainForm()
        {
            autoClickerService = new AutoClickerService();
            recorder = new AutoClickerRecorder();
            recordedClicks = new List<ClickRecord>();
            hotkeySettings = SettingsManager.LoadHotkeySettings();
            
            this.Text = "AMC Clicker";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new System.Drawing.Size(650, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            tabControl = new TabControl { Dock = DockStyle.Fill, Padding = new System.Drawing.Point(10, 10) };
            
            // Main Tab
            var mainTab = new TabPage("Clicker");
            mainTab.Controls.Add(CreateMainTabContent());
            tabControl.TabPages.Add(mainTab);

            // Settings Tab
            var settingsTab = new TabPage("Settings");
            settingsTab.Controls.Add(CreateSettingsTabContent());
            tabControl.TabPages.Add(settingsTab);

            this.Controls.Add(tabControl);
        }

        private Control CreateMainTabContent()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), AutoScroll = true };
            
            // Title
            var titleLabel = new Label 
            { 
                Text = "AMC Clicker - Auto Clicker",
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            panel.Controls.Add(titleLabel);

            // Click Rate
            panel.Controls.Add(new Label { Text = "Clicks per second:", Location = new System.Drawing.Point(10, 50), AutoSize = true });
            var clickRateInput = new NumericUpDown 
            { 
                Minimum = 1, 
                Maximum = 100, 
                Value = 10,
                Location = new System.Drawing.Point(180, 50),
                Width = 100
            };
            panel.Controls.Add(clickRateInput);

            // Click Delay
            panel.Controls.Add(new Label { Text = "Delay between clicks (ms):", Location = new System.Drawing.Point(10, 90), AutoSize = true });
            var delayInput = new NumericUpDown 
            { 
                Minimum = 10, 
                Maximum = 5000, 
                Value = 100,
                Location = new System.Drawing.Point(180, 90),
                Width = 100
            };
            panel.Controls.Add(delayInput);

            // Number of Clicks
            panel.Controls.Add(new Label { Text = "Number of clicks (0 = infinite):", Location = new System.Drawing.Point(10, 130), AutoSize = true });
            var numClicksInput = new NumericUpDown 
            { 
                Minimum = 0, 
                Maximum = 10000, 
                Value = 100,
                Location = new System.Drawing.Point(180, 130),
                Width = 100
            };
            panel.Controls.Add(numClicksInput);

            // Start Button
            var startBtn = new Button 
            { 
                Text = $"Start Clicking ({hotkeySettings.StartClickingHotkey})", 
                Location = new System.Drawing.Point(10, 180),
                Width = 160,
                Height = 40
            };
            startBtn.Click += (s, e) => StartClicking((int)clickRateInput.Value, (int)delayInput.Value, (int)numClicksInput.Value);
            panel.Controls.Add(startBtn);

            // Stop Button
            var stopBtn = new Button 
            { 
                Text = $"Stop Clicking ({hotkeySettings.StopClickingHotkey})", 
                Location = new System.Drawing.Point(180, 180),
                Width = 160,
                Height = 40
            };
            stopBtn.Click += (s, e) => StopClicking();
            panel.Controls.Add(stopBtn);

            // Record Button
            var recordBtn = new Button 
            { 
                Text = $"Start Recording ({hotkeySettings.StartRecordingHotkey})", 
                Location = new System.Drawing.Point(10, 230),
                Width = 160,
                Height = 40,
                BackColor = System.Drawing.Color.LightGray
            };
            recordBtn.Click += (s, e) => StartRecording();
            panel.Controls.Add(recordBtn);

            // Stop Recording Button
            var stopRecordBtn = new Button 
            { 
                Text = $"Stop Recording ({hotkeySettings.StopRecordingHotkey})", 
                Location = new System.Drawing.Point(180, 230),
                Width = 160,
                Height = 40,
                BackColor = System.Drawing.Color.LightGray
            };
            stopRecordBtn.Click += (s, e) => StopRecording();
            panel.Controls.Add(stopRecordBtn);

            // Playback Button
            var playbackBtn = new Button 
            { 
                Text = $"Playback Recording ({hotkeySettings.PlaybackRecordingHotkey})", 
                Location = new System.Drawing.Point(10, 280),
                Width = 160,
                Height = 40
            };
            playbackBtn.Click += (s, e) => PlaybackRecording();
            panel.Controls.Add(playbackBtn);

            // Clear Recording Button
            var clearBtn = new Button 
            { 
                Text = "Clear Recording", 
                Location = new System.Drawing.Point(180, 280),
                Width = 160,
                Height = 40
            };
            clearBtn.Click += (s, e) => ClearRecording();
            panel.Controls.Add(clearBtn);

            // Status Label
            statusLabel = new Label 
            { 
                Text = "Status: Ready",
                Location = new System.Drawing.Point(10, 330),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.Green
            };
            panel.Controls.Add(statusLabel);

            // Instructions
            var instructionsLabel = new Label 
            { 
                Text = $"Global Hotkeys:\n{hotkeySettings.StartClickingHotkey} - Start Clicking\n{hotkeySettings.StopClickingHotkey} - Stop Clicking\n{hotkeySettings.StartRecordingHotkey} - Start Recording\n{hotkeySettings.StopRecordingHotkey} - Stop Recording\n{hotkeySettings.PlaybackRecordingHotkey} - Playback Recording\n{hotkeySettings.ExitApplicationHotkey} - Exit App\n\nTip: Customize hotkeys in Settings tab!",
                Location = new System.Drawing.Point(10, 370),
                AutoSize = true,
                Font = new System.Drawing.Font("Arial", 9),
                BackColor = System.Drawing.Color.WhiteSmoke,
                Padding = new Padding(5)
            };
            panel.Controls.Add(instructionsLabel);

            return panel;
        }

        private Control CreateSettingsTabContent()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15), AutoScroll = true };
            
            var titleLabel = new Label 
            { 
                Text = "Hotkey Settings",
                Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            panel.Controls.Add(titleLabel);

            int yPos = 50;
            int spacing = 40;

            // Create hotkey setting inputs
            var hotkeyInputs = new Dictionary<string, ComboBox>();

            void AddHotkeySetting(string label, Keys currentKey, int y, string settingKey)
            {
                panel.Controls.Add(new Label { Text = $"{label}:", Location = new System.Drawing.Point(10, y), AutoSize = true });
                
                var combo = new ComboBox 
                { 
                    Location = new System.Drawing.Point(200, y),
                    Width = 150,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                // Populate with common function keys
                var keys = new Keys[] { Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12, Keys.Escape };
                foreach (var k in keys)
                {
                    combo.Items.Add(k);
                }

                combo.SelectedItem = currentKey;
                combo.Tag = settingKey;
                hotkeyInputs[settingKey] = combo;
                panel.Controls.Add(combo);
            }

            AddHotkeySetting("Start Clicking", hotkeySettings.StartClickingHotkey, yPos, nameof(HotkeySettings.StartClickingHotkey));
            yPos += spacing;
            AddHotkeySetting("Stop Clicking", hotkeySettings.StopClickingHotkey, yPos, nameof(HotkeySettings.StopClickingHotkey));
            yPos += spacing;
            AddHotkeySetting("Start Recording", hotkeySettings.StartRecordingHotkey, yPos, nameof(HotkeySettings.StartRecordingHotkey));
            yPos += spacing;
            AddHotkeySetting("Stop Recording", hotkeySettings.StopRecordingHotkey, yPos, nameof(HotkeySettings.StopRecordingHotkey));
            yPos += spacing;
            AddHotkeySetting("Playback Recording", hotkeySettings.PlaybackRecordingHotkey, yPos, nameof(HotkeySettings.PlaybackRecordingHotkey));
            yPos += spacing;
            AddHotkeySetting("Exit Application", hotkeySettings.ExitApplicationHotkey, yPos, nameof(HotkeySettings.ExitApplicationHotkey));
            yPos += spacing;

            // Save Button
            var saveBtn = new Button 
            { 
                Text = "Save Settings", 
                Location = new System.Drawing.Point(10, yPos + 20),
                Width = 120,
                Height = 40,
                BackColor = System.Drawing.Color.LightGreen
            };
            saveBtn.Click += (s, e) =>
            {
                // Update hotkey settings
                hotkeySettings.StartClickingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.StartClickingHotkey)].SelectedItem;
                hotkeySettings.StopClickingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.StopClickingHotkey)].SelectedItem;
                hotkeySettings.StartRecordingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.StartRecordingHotkey)].SelectedItem;
                hotkeySettings.StopRecordingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.StopRecordingHotkey)].SelectedItem;
                hotkeySettings.PlaybackRecordingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.PlaybackRecordingHotkey)].SelectedItem;
                hotkeySettings.ExitApplicationHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.ExitApplicationHotkey)].SelectedItem;

                // Save to file
                SettingsManager.SaveHotkeySettings(hotkeySettings);
                
                // Re-register hotkeys
                SetupHotkeys();
                
                MessageBox.Show("Settings saved! Hotkeys have been updated.", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Controls.Add(saveBtn);

            // Reset Button
            var resetBtn = new Button 
            { 
                Text = "Reset to Defaults", 
                Location = new System.Drawing.Point(140, yPos + 20),
                Width = 150,
                Height = 40
            };
            resetBtn.Click += (s, e) =>
            {
                hotkeySettings = new HotkeySettings();
                SettingsManager.SaveHotkeySettings(hotkeySettings);
                
                hotkeyInputs[nameof(HotkeySettings.StartClickingHotkey)].SelectedItem = hotkeySettings.StartClickingHotkey;
                hotkeyInputs[nameof(HotkeySettings.StopClickingHotkey)].SelectedItem = hotkeySettings.StopClickingHotkey;
                hotkeyInputs[nameof(HotkeySettings.StartRecordingHotkey)].SelectedItem = hotkeySettings.StartRecordingHotkey;
                hotkeyInputs[nameof(HotkeySettings.StopRecordingHotkey)].SelectedItem = hotkeySettings.StopRecordingHotkey;
                hotkeyInputs[nameof(HotkeySettings.PlaybackRecordingHotkey)].SelectedItem = hotkeySettings.PlaybackRecordingHotkey;
                hotkeyInputs[nameof(HotkeySettings.ExitApplicationHotkey)].SelectedItem = hotkeySettings.ExitApplicationHotkey;
                
                SetupHotkeys();
                MessageBox.Show("Settings reset to defaults!", "Reset Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Controls.Add(resetBtn);

            return panel;
        }

        private void SetupHotkeys()
        {
            // Global hotkeys require Windows-specific interop
            // For now, use button clicks as primary method
            // Full global hotkey support would require additional libraries
            MessageBox.Show("Global hotkeys work when the app window has focus.\nUse the buttons or Alt+key combinations.", "Hotkey Info");
        }

        private void StartClicking(int clicksPerSecond, int delay, int numClicks)
        {
            if (!autoClickerService.IsRunning)
            {
                autoClickerService.Start(clicksPerSecond, delay, numClicks);
                UpdateStatus("Clicking...");
            }
        }

        private void StopClicking()
        {
            autoClickerService.Stop();
            UpdateStatus("Stopped");
        }

        private void StartRecording()
        {
            if (!isRecording)
            {
                recordedClicks.Clear();
                isRecording = true;
                recorder.StartRecording(recordedClicks);
                UpdateStatus($"Recording clicks...");
            }
        }

        private void StopRecording()
        {
            if (isRecording)
            {
                isRecording = false;
                recorder.StopRecording();
                UpdateStatus($"Recorded {recordedClicks.Count} clicks");
            }
        }

        private void PlaybackRecording()
        {
            if (recordedClicks.Count > 0 && !isPlaying)
            {
                isPlaying = true;
                UpdateStatus("Playing back recording...");
                
                var worker = new System.ComponentModel.BackgroundWorker();
                worker.DoWork += (s, e) =>
                {
                    foreach (var click in recordedClicks)
                    {
                        if (!isPlaying) break;
                        
                        MouseSimulator.MoveMouse(click.X, click.Y);
                        System.Threading.Thread.Sleep(50);
                        MouseSimulator.Click();
                        System.Threading.Thread.Sleep(click.DelayMs);
                    }
                    isPlaying = false;
                    this.Invoke(new Action(() => UpdateStatus("Playback complete")));
                };
                
                worker.RunWorkerAsync();
            }
            else if (recordedClicks.Count == 0)
            {
                UpdateStatus("No recording available");
            }
        }

        private void ClearRecording()
        {
            recordedClicks.Clear();
            UpdateStatus("Recording cleared");
        }

        private void UpdateStatus(string status)
        {
            if (statusLabel != null)
            {
                statusLabel.Text = $"Status: {status}";
            }
        }
    }

    public class AutoClickerService
    {
        public bool IsRunning { get; private set; }
        private System.Threading.Thread clickThread;
        private volatile bool shouldStop = false;

        public void Start(int clicksPerSecond, int delayMs, int numClicks)
        {
            if (IsRunning) return;

            IsRunning = true;
            shouldStop = false;

            clickThread = new System.Threading.Thread(() =>
            {
                int clickCount = 0;
                while (!shouldStop && (numClicks == 0 || clickCount < numClicks))
                {
                    MouseSimulator.Click();
                    clickCount++;
                    System.Threading.Thread.Sleep(delayMs);
                }
                IsRunning = false;
            });
            clickThread.Start();
        }

        public void Stop()
        {
            shouldStop = true;
            IsRunning = false;
        }
    }

    public class AutoClickerRecorder
    {
        private System.Diagnostics.Stopwatch stopwatch;

        public void StartRecording(List<ClickRecord> recordList)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            new System.ComponentModel.BackgroundWorker().DoWork += (s, e) =>
            {
                var lastClickTime = 0L;
                
                while (true)
                {
                    if (Control.MouseButtons == MouseButtons.Left)
                    {
                        var currentTime = stopwatch.ElapsedMilliseconds;
                        var delay = (int)(currentTime - lastClickTime);
                        
                        var pos = Control.MousePosition;
                        recordList.Add(new ClickRecord { X = pos.X, Y = pos.Y, DelayMs = delay });
                        
                        lastClickTime = currentTime;
                        System.Threading.Thread.Sleep(100); // Debounce
                    }
                    System.Threading.Thread.Sleep(10);
                }
            };
        }

        public void StopRecording()
        {
            stopwatch?.Stop();
        }
    }

    public class ClickRecord
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int DelayMs { get; set; }
    }

    public class MouseSimulator
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_MOVE = 0x0001;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        public static void Click()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            System.Threading.Thread.Sleep(50);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public static void MoveMouse(int x, int y)
        {
            SetCursorPos(x, y);
        }
    }
}
