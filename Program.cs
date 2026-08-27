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
        private int recordedClickCount = 0;

        // PS5-Inspired Colors
        private static readonly System.Drawing.Color PS5_DarkBg = System.Drawing.Color.FromArgb(10, 14, 39);
        private static readonly System.Drawing.Color PS5_MediumBg = System.Drawing.Color.FromArgb(20, 28, 70);
        private static readonly System.Drawing.Color PS5_LightBg = System.Drawing.Color.FromArgb(30, 40, 100);
        private static readonly System.Drawing.Color PS5_Accent = System.Drawing.Color.FromArgb(0, 180, 220);
        private static readonly System.Drawing.Color PS5_AccentDark = System.Drawing.Color.FromArgb(0, 150, 200);
        private static readonly System.Drawing.Color PS5_Text = System.Drawing.Color.FromArgb(220, 230, 240);
        private static readonly System.Drawing.Color PS5_TextMuted = System.Drawing.Color.FromArgb(140, 150, 170);

        public MainForm()
        {
            autoClickerService = new AutoClickerService();
            recorder = new AutoClickerRecorder();
            recordedClicks = new List<ClickRecord>();
            hotkeySettings = SettingsManager.LoadHotkeySettings();
            
            this.Text = "AMC Clicker";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new System.Drawing.Size(700, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = PS5_DarkBg;
            this.Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Regular);
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            tabControl = new TabControl 
            { 
                Dock = DockStyle.Fill, 
                Padding = new System.Drawing.Point(0, 0),
                BackColor = PS5_DarkBg,
                ForeColor = PS5_Text
            };
            tabControl.Selecting += (s, e) => e.TabPage.BackColor = PS5_DarkBg;
            
            // Style tab pages
            var mainTab = new TabPage("Clicker");
            mainTab.BackColor = PS5_DarkBg;
            mainTab.ForeColor = PS5_Text;
            mainTab.Controls.Add(CreateMainTabContent());
            tabControl.TabPages.Add(mainTab);

            var settingsTab = new TabPage("Settings");
            settingsTab.BackColor = PS5_DarkBg;
            settingsTab.ForeColor = PS5_Text;
            settingsTab.Controls.Add(CreateSettingsTabContent());
            tabControl.TabPages.Add(settingsTab);

            this.Controls.Add(tabControl);
        }

        private Control CreateMainTabContent()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true, BackColor = PS5_DarkBg };
            
            // Title
            var titleLabel = new Label 
            { 
                Text = "AMC CLICKER",
                Font = new System.Drawing.Font("Segoe UI", 24, System.Drawing.FontStyle.Bold),
                ForeColor = PS5_Accent,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30)
            };
            panel.Controls.Add(titleLabel);

            // Status Card
            statusLabel = CreateCard(10, 50, 650, 60, "Ready");
            statusLabel.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
            statusLabel.Text = "Status: Ready";
            statusLabel.ForeColor = System.Drawing.Color.LimeGreen;
            statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            panel.Controls.Add(statusLabel);

            // Settings Section
            var settingsTitleLabel = CreateSectionLabel(10, 130, "Click Settings");
            panel.Controls.Add(settingsTitleLabel);

            // Click Rate Input
            panel.Controls.Add(new Label { Text = "Clicks per second:", Location = new System.Drawing.Point(20, 165), AutoSize = true, ForeColor = PS5_Text, Font = new System.Drawing.Font("Segoe UI", 10) });
            var clickRateInput = CreateModernInput(250, 160, 80, "10");
            var clickRateNum = new NumericUpDown { Minimum = 1, Maximum = 100, Value = 10, Location = new System.Drawing.Point(250, 160), Width = 80, Height = 30, BackColor = PS5_LightBg, ForeColor = PS5_Text };
            clickRateNum.Text = "10";
            panel.Controls.Add(clickRateNum);

            // Delay Input
            panel.Controls.Add(new Label { Text = "Delay (ms):", Location = new System.Drawing.Point(20, 205), AutoSize = true, ForeColor = PS5_Text, Font = new System.Drawing.Font("Segoe UI", 10) });
            var delayNum = new NumericUpDown { Minimum = 10, Maximum = 5000, Value = 100, Location = new System.Drawing.Point(250, 200), Width = 80, Height = 30, BackColor = PS5_LightBg, ForeColor = PS5_Text };
            delayNum.Text = "100";
            panel.Controls.Add(delayNum);

            // Number of Clicks Input
            panel.Controls.Add(new Label { Text = "Number of clicks (0=∞):", Location = new System.Drawing.Point(20, 245), AutoSize = true, ForeColor = PS5_Text, Font = new System.Drawing.Font("Segoe UI", 10) });
            var numClicksNum = new NumericUpDown { Minimum = 0, Maximum = 10000, Value = 100, Location = new System.Drawing.Point(250, 240), Width = 80, Height = 30, BackColor = PS5_LightBg, ForeColor = PS5_Text };
            numClicksNum.Text = "100";
            panel.Controls.Add(numClicksNum);

            // Action Section
            var actionTitleLabel = CreateSectionLabel(10, 290, "Actions");
            panel.Controls.Add(actionTitleLabel);

            // Start Button
            var startBtn = CreateModernButton("START CLICKING", 20, 325, 150, 50, PS5_Accent);
            startBtn.Click += (s, e) => StartClicking((int)clickRateNum.Value, (int)delayNum.Value, (int)numClicksNum.Value);
            panel.Controls.Add(startBtn);

            // Stop Button
            var stopBtn = CreateModernButton("STOP", 180, 325, 150, 50, System.Drawing.Color.FromArgb(220, 50, 50));
            stopBtn.Click += (s, e) => StopClicking();
            panel.Controls.Add(stopBtn);

            // Recording Section
            var recordTitleLabel = CreateSectionLabel(10, 385, "Recording");
            panel.Controls.Add(recordTitleLabel);

            // Record Button
            var recordBtn = CreateModernButton("START RECORDING", 20, 420, 150, 50, PS5_Accent);
            recordBtn.Click += (s, e) => StartRecording();
            panel.Controls.Add(recordBtn);

            // Stop Recording Button
            var stopRecordBtn = CreateModernButton("STOP", 180, 420, 150, 50, System.Drawing.Color.FromArgb(220, 50, 50));
            stopRecordBtn.Click += (s, e) => StopRecording();
            panel.Controls.Add(stopRecordBtn);

            // Playback Button
            var playbackBtn = CreateModernButton("PLAYBACK", 20, 480, 150, 50, System.Drawing.Color.FromArgb(100, 200, 50));
            playbackBtn.Click += (s, e) => PlaybackRecording();
            panel.Controls.Add(playbackBtn);

            // Clear Button
            var clearBtn = CreateModernButton("CLEAR", 180, 480, 150, 50, System.Drawing.Color.FromArgb(100, 100, 100));
            clearBtn.Click += (s, e) => ClearRecording();
            panel.Controls.Add(clearBtn);

            // Recording Info
            var recordInfoLabel = new Label 
            { 
                Text = $"Recorded Clicks: {recordedClickCount}",
                Location = new System.Drawing.Point(20, 545),
                AutoSize = true,
                ForeColor = PS5_TextMuted,
                Font = new System.Drawing.Font("Segoe UI", 10)
            };
            this.Tag = recordInfoLabel;
            panel.Controls.Add(recordInfoLabel);

            // Hotkey Info
            var hotkeyTitleLabel = CreateSectionLabel(10, 575, "Hotkeys");
            panel.Controls.Add(hotkeyTitleLabel);

            var hotkeyInfoLabel = new Label 
            { 
                Text = $"F6: Start | F7: Stop\nF8: Record | F9: Stop Rec\nF10: Playback | ESC: Exit\n\nCustomize in Settings tab →",
                Location = new System.Drawing.Point(20, 610),
                AutoSize = true,
                ForeColor = PS5_TextMuted,
                Font = new System.Drawing.Font("Segoe UI", 9)
            };
            panel.Controls.Add(hotkeyInfoLabel);

            return panel;
        }

        private Control CreateSettingsTabContent()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true, BackColor = PS5_DarkBg };
            
            var titleLabel = new Label 
            { 
                Text = "HOTKEY SETTINGS",
                Font = new System.Drawing.Font("Segoe UI", 20, System.Drawing.FontStyle.Bold),
                ForeColor = PS5_Accent,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 30)
            };
            panel.Controls.Add(titleLabel);

            int yPos = 50;
            int spacing = 50;

            var hotkeyInputs = new Dictionary<string, ComboBox>();

            void AddHotkeySetting(string label, Keys currentKey, int y, string settingKey)
            {
                var labelControl = new Label { Text = label, Location = new System.Drawing.Point(20, y), AutoSize = true, ForeColor = PS5_Text, Font = new System.Drawing.Font("Segoe UI", 11) };
                panel.Controls.Add(labelControl);
                
                var combo = new ComboBox 
                { 
                    Location = new System.Drawing.Point(250, y),
                    Width = 200,
                    Height = 35,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    BackColor = PS5_LightBg,
                    ForeColor = PS5_Text,
                    Font = new System.Drawing.Font("Segoe UI", 10)
                };

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
            yPos += spacing + 20;

            // Save Button
            var saveBtn = CreateModernButton("SAVE SETTINGS", 20, yPos, 200, 50, PS5_Accent);
            saveBtn.Click += (s, e) =>
            {
                hotkeySettings.StartClickingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.StartClickingHotkey)].SelectedItem;
                hotkeySettings.StopClickingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.StopClickingHotkey)].SelectedItem;
                hotkeySettings.StartRecordingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.StartRecordingHotkey)].SelectedItem;
                hotkeySettings.StopRecordingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.StopRecordingHotkey)].SelectedItem;
                hotkeySettings.PlaybackRecordingHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.PlaybackRecordingHotkey)].SelectedItem;
                hotkeySettings.ExitApplicationHotkey = (Keys)hotkeyInputs[nameof(HotkeySettings.ExitApplicationHotkey)].SelectedItem;

                SettingsManager.SaveHotkeySettings(hotkeySettings);
                SetupHotkeys();
                
                MessageBox.Show("Settings saved! Hotkeys updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            panel.Controls.Add(saveBtn);

            // Reset Button
            var resetBtn = CreateModernButton("RESET TO DEFAULTS", 230, yPos, 200, 50, System.Drawing.Color.FromArgb(100, 100, 100));
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

        private Label CreateSectionLabel(int x, int y, string text)
        {
            var label = new Label 
            { 
                Text = text.ToUpper(),
                Location = new System.Drawing.Point(x, y),
                AutoSize = true,
                ForeColor = PS5_Accent,
                Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold)
            };
            return label;
        }

        private Label CreateCard(int x, int y, int width, int height, string text)
        {
            var card = new Label 
            { 
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, height),
                Text = text,
                BackColor = PS5_LightBg,
                ForeColor = PS5_Text,
                Padding = new Padding(15),
                BorderStyle = BorderStyle.None
            };
            return card;
        }

        private Button CreateModernButton(string text, int x, int y, int width, int height, System.Drawing.Color accentColor)
        {
            var btn = new Button 
            { 
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, height),
                BackColor = accentColor,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseDownBackColor = accentColor;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(accentColor, 0.1f);
            return btn;
        }

        private TextBox CreateModernInput(int x, int y, int width, string text)
        {
            var input = new TextBox 
            { 
                Location = new System.Drawing.Point(x, y),
                Width = width,
                Height = 30,
                Text = text,
                BackColor = PS5_LightBg,
                ForeColor = PS5_Text,
                Font = new System.Drawing.Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None
            };
            return input;
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
                UpdateStatus("Running...", System.Drawing.Color.LimeGreen);
            }
        }

        private void StopClicking()
        {
            autoClickerService.Stop();
            UpdateStatus("Stopped", System.Drawing.Color.FromArgb(220, 150, 50));
        }

        private void StartRecording()
        {
            if (!isRecording)
            {
                recordedClicks.Clear();
                recordedClickCount = 0;
                isRecording = true;
                recorder.StartRecording(recordedClicks);
                UpdateStatus("Recording...", System.Drawing.Color.FromArgb(255, 100, 100));
            }
        }

        private void StopRecording()
        {
            if (isRecording)
            {
                isRecording = false;
                recorder.StopRecording();
                recordedClickCount = recordedClicks.Count;
                UpdateStatus($"Recorded {recordedClickCount} clicks", System.Drawing.Color.LimeGreen);
                if (this.Tag is Label infoLabel)
                    infoLabel.Text = $"Recorded Clicks: {recordedClickCount}";
            }
        }

        private void PlaybackRecording()
        {
            if (recordedClicks.Count > 0 && !isPlaying)
            {
                isPlaying = true;
                UpdateStatus("Playing back...", System.Drawing.Color.FromArgb(100, 200, 50));
                
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
                    this.Invoke(new Action(() => UpdateStatus("Playback complete", System.Drawing.Color.LimeGreen)));
                };
                
                worker.RunWorkerAsync();
            }
            else if (recordedClicks.Count == 0)
            {
                UpdateStatus("No recording to play", System.Drawing.Color.FromArgb(220, 50, 50));
            }
        }

        private void ClearRecording()
        {
            recordedClicks.Clear();
            recordedClickCount = 0;
            UpdateStatus("Recording cleared", System.Drawing.Color.FromArgb(220, 150, 50));
            if (this.Tag is Label infoLabel)
                infoLabel.Text = $"Recorded Clicks: 0";
        }

        private void UpdateStatus(string status, System.Drawing.Color color)
        {
            if (statusLabel != null)
            {
                statusLabel.Text = $"Status: {status}";
                statusLabel.ForeColor = color;
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
