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
        private Label statusLabel;
        private Label recordInfoLabel;
        private int recordedClickCount = 0;

        // Modern Minimalist Color Palette
        private static readonly System.Drawing.Color BG_DARK = System.Drawing.Color.FromArgb(15, 15, 20);
        private static readonly System.Drawing.Color BG_SURFACE = System.Drawing.Color.FromArgb(25, 25, 35);
        private static readonly System.Drawing.Color BG_HOVER = System.Drawing.Color.FromArgb(35, 35, 50);
        private static readonly System.Drawing.Color ACCENT = System.Drawing.Color.FromArgb(100, 200, 255);
        private static readonly System.Drawing.Color ACCENT_HOVER = System.Drawing.Color.FromArgb(150, 220, 255);
        private static readonly System.Drawing.Color TEXT_PRIMARY = System.Drawing.Color.FromArgb(230, 230, 240);
        private static readonly System.Drawing.Color TEXT_SECONDARY = System.Drawing.Color.FromArgb(140, 145, 160);
        private static readonly System.Drawing.Color SUCCESS = System.Drawing.Color.FromArgb(100, 220, 100);
        private static readonly System.Drawing.Color ERROR = System.Drawing.Color.FromArgb(240, 100, 100);
        private static readonly System.Drawing.Color WARNING = System.Drawing.Color.FromArgb(255, 160, 80);

        public MainForm()
        {
            autoClickerService = new AutoClickerService();
            recorder = new AutoClickerRecorder();
            recordedClicks = new List<ClickRecord>();
            hotkeySettings = SettingsManager.LoadHotkeySettings();
            
            this.Text = "AMC Clicker";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Size = new System.Drawing.Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BG_DARK;
            this.Font = new System.Drawing.Font("Segoe UI", 10);
            this.DoubleBuffered = true;
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, BackColor = BG_DARK };
            
            // Header
            var header = CreateHeader();
            mainPanel.Controls.Add(header);
            
            // Content Area
            var contentPanel = new Panel 
            { 
                Location = new System.Drawing.Point(0, 80),
                Size = new System.Drawing.Size(900, 620),
                BackColor = BG_DARK
            };
            
            // Left Column - Clicker
            var leftCol = CreateClickerSection();
            leftCol.Location = new System.Drawing.Point(20, 0);
            contentPanel.Controls.Add(leftCol);
            
            // Right Column - Settings & Info
            var rightCol = CreateSettingsSection();
            rightCol.Location = new System.Drawing.Point(450, 0);
            contentPanel.Controls.Add(rightCol);
            
            mainPanel.Controls.Add(contentPanel);
            this.Controls.Add(mainPanel);
        }

        private Panel CreateHeader()
        {
            var header = new Panel 
            { 
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = BG_SURFACE,
                Padding = new Padding(20)
            };
            
            var title = new Label 
            { 
                Text = "AMC CLICKER",
                Font = new System.Drawing.Font("Segoe UI", 28, System.Drawing.FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize = true,
                Location = new System.Drawing.Point(20, 20)
            };
            
            statusLabel = new Label 
            { 
                Text = "● Ready",
                Font = new System.Drawing.Font("Segoe UI", 11),
                ForeColor = TEXT_SECONDARY,
                AutoSize = true,
                Location = new System.Drawing.Point(300, 45)
            };
            
            header.Controls.Add(title);
            header.Controls.Add(statusLabel);
            
            return header;
        }

        private Panel CreateClickerSection()
        {
            var section = new Panel { Size = new System.Drawing.Size(400, 600), BackColor = BG_DARK };
            
            var titleLabel = CreateSmallTitle("CLICKER");
            titleLabel.Location = new System.Drawing.Point(0, 0);
            section.Controls.Add(titleLabel);
            
            int yPos = 40;
            
            // Settings Card
            yPos = AddSettingInput(section, yPos, "Clicks/sec", "10", 1, 100);
            yPos = AddSettingInput(section, yPos, "Delay (ms)", "100", 10, 5000);
            yPos = AddSettingInput(section, yPos, "Count (0=∞)", "100", 0, 10000);
            
            yPos += 30;
            
            // Action Buttons
            var startBtn = CreateButton("START", 0, yPos, 180, 50, ACCENT);
            var stopBtn = CreateButton("STOP", 210, yPos, 180, 50, ERROR);
            startBtn.Click += (s, e) => 
            {
                var cps = GetInputValue(section, "Clicks/sec", 10);
                var delay = GetInputValue(section, "Delay (ms)", 100);
                var count = GetInputValue(section, "Count (0=∞)", 100);
                StartClicking(cps, delay, count);
            };
            stopBtn.Click += (s, e) => StopClicking();
            section.Controls.Add(startBtn);
            section.Controls.Add(stopBtn);
            
            yPos += 70;
            
            // Recording Section
            var recTitle = CreateSmallTitle("RECORDING");
            recTitle.Location = new System.Drawing.Point(0, yPos);
            section.Controls.Add(recTitle);
            
            yPos += 40;
            
            var recordBtn = CreateButton("RECORD", 0, yPos, 180, 50, WARNING);
            var playBtn = CreateButton("PLAYBACK", 210, yPos, 180, 50, SUCCESS);
            recordBtn.Click += (s, e) => StartRecording();
            playBtn.Click += (s, e) => PlaybackRecording();
            section.Controls.Add(recordBtn);
            section.Controls.Add(playBtn);
            
            yPos += 70;
            
            recordInfoLabel = new Label 
            { 
                Text = "Recorded: 0 clicks",
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = TEXT_SECONDARY,
                AutoSize = true,
                Location = new System.Drawing.Point(0, yPos)
            };
            section.Controls.Add(recordInfoLabel);
            
            return section;
        }

        private Panel CreateSettingsSection()
        {
            var section = new Panel { Size = new System.Drawing.Size(400, 600), BackColor = BG_DARK };
            
            var titleLabel = CreateSmallTitle("HOTKEYS");
            titleLabel.Location = new System.Drawing.Point(0, 0);
            section.Controls.Add(titleLabel);
            
            int yPos = 40;
            
            // Hotkey selectors
            yPos = AddHotkeySelector(section, yPos, "Start", hotkeySettings.StartClickingHotkey, nameof(HotkeySettings.StartClickingHotkey));
            yPos = AddHotkeySelector(section, yPos, "Stop", hotkeySettings.StopClickingHotkey, nameof(HotkeySettings.StopClickingHotkey));
            yPos = AddHotkeySelector(section, yPos, "Record", hotkeySettings.StartRecordingHotkey, nameof(HotkeySettings.StartRecordingHotkey));
            yPos = AddHotkeySelector(section, yPos, "Playback", hotkeySettings.PlaybackRecordingHotkey, nameof(HotkeySettings.PlaybackRecordingHotkey));
            
            yPos += 20;
            
            var saveBtn = CreateButton("SAVE", 0, yPos, 180, 40, ACCENT);
            var resetBtn = CreateButton("RESET", 210, yPos, 180, 40, TEXT_SECONDARY);
            
            saveBtn.Click += (s, e) => SaveHotkeys(section);
            resetBtn.Click += (s, e) => ResetHotkeys(section);
            
            section.Controls.Add(saveBtn);
            section.Controls.Add(resetBtn);
            
            return section;
        }

        private Label CreateSmallTitle(string text)
        {
            return new Label 
            { 
                Text = text,
                Font = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold),
                ForeColor = ACCENT,
                AutoSize = true
            };
        }

        private int AddSettingInput(Panel parent, int yPos, string label, string defaultValue, int min, int max)
        {
            var labelControl = new Label 
            { 
                Text = label,
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = TEXT_SECONDARY,
                Location = new System.Drawing.Point(0, yPos),
                AutoSize = true
            };
            parent.Controls.Add(labelControl);
            
            var input = new NumericUpDown 
            { 
                Location = new System.Drawing.Point(0, yPos + 25),
                Size = new System.Drawing.Size(390, 35),
                Minimum = min,
                Maximum = max,
                Value = int.Parse(defaultValue),
                BackColor = BG_SURFACE,
                ForeColor = TEXT_PRIMARY,
                Font = new System.Drawing.Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None
            };
            input.Tag = label;
            parent.Controls.Add(input);
            
            return yPos + 65;
        }

        private int AddHotkeySelector(Panel parent, int yPos, string label, Keys currentKey, string settingKey)
        {
            var labelControl = new Label 
            { 
                Text = label,
                Font = new System.Drawing.Font("Segoe UI", 10),
                ForeColor = TEXT_SECONDARY,
                Location = new System.Drawing.Point(0, yPos),
                AutoSize = true
            };
            parent.Controls.Add(labelControl);
            
            var combo = new ComboBox 
            { 
                Location = new System.Drawing.Point(0, yPos + 25),
                Size = new System.Drawing.Size(390, 35),
                BackColor = BG_SURFACE,
                ForeColor = TEXT_PRIMARY,
                Font = new System.Drawing.Font("Segoe UI", 10),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            
            var keys = new Keys[] { Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10, Keys.F11, Keys.F12, Keys.Escape };
            foreach (var k in keys) combo.Items.Add(k);
            combo.SelectedItem = currentKey;
            combo.Tag = settingKey;
            
            parent.Controls.Add(combo);
            return yPos + 65;
        }

        private Button CreateButton(string text, int x, int y, int width, int height, System.Drawing.Color color)
        {
            var btn = new Button 
            { 
                Text = text,
                Location = new System.Drawing.Point(x, y),
                Size = new System.Drawing.Size(width, height),
                BackColor = color,
                ForeColor = BG_DARK,
                Font = new System.Drawing.Font("Segoe UI", 11, System.Drawing.FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.15f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.15f);
            return btn;
        }

        private int GetInputValue(Panel section, string tag, int defaultValue)
        {
            foreach (Control c in section.Controls)
            {
                if (c is NumericUpDown input && input.Tag?.ToString() == tag)
                    return (int)input.Value;
            }
            return defaultValue;
        }

        private void SaveHotkeys(Panel section)
        {
            foreach (Control c in section.Controls)
            {
                if (c is ComboBox combo && combo.SelectedItem != null)
                {
                    var settingKey = combo.Tag.ToString();
                    var selectedKey = (Keys)combo.SelectedItem;
                    
                    var prop = typeof(HotkeySettings).GetProperty(settingKey);
                    if (prop != null) prop.SetValue(hotkeySettings, selectedKey);
                }
            }
            SettingsManager.SaveHotkeySettings(hotkeySettings);
            SetupHotkeys();
            MessageBox.Show("Hotkeys saved!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetHotkeys(Panel section)
        {
            hotkeySettings = new HotkeySettings();
            SettingsManager.SaveHotkeySettings(hotkeySettings);
            
            foreach (Control c in section.Controls)
            {
                if (c is ComboBox combo)
                {
                    var prop = typeof(HotkeySettings).GetProperty(combo.Tag.ToString());
                    if (prop != null)
                        combo.SelectedItem = (Keys)prop.GetValue(hotkeySettings);
                }
            }
            SetupHotkeys();
            MessageBox.Show("Reset to defaults!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetupHotkeys()
        {
            // Hotkeys would require global hook - for now just UI
        }

        private void StartClicking(int clicksPerSecond, int delay, int numClicks)
        {
            if (!autoClickerService.IsRunning)
            {
                autoClickerService.Start(clicksPerSecond, delay, numClicks);
                UpdateStatus("Running", SUCCESS);
            }
        }

        private void StopClicking()
        {
            autoClickerService.Stop();
            UpdateStatus("Stopped", TEXT_SECONDARY);
        }

        private void StartRecording()
        {
            if (!isRecording)
            {
                recordedClicks.Clear();
                recordedClickCount = 0;
                isRecording = true;
                recorder.StartRecording(recordedClicks);
                UpdateStatus("Recording", WARNING);
            }
        }

        private void PlaybackRecording()
        {
            if (recordedClicks.Count > 0 && !isPlaying)
            {
                isPlaying = true;
                UpdateStatus("Playing", SUCCESS);
                
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
                    this.Invoke(new Action(() => UpdateStatus("Ready", TEXT_SECONDARY)));
                };
                worker.RunWorkerAsync();
            }
            else if (recordedClicks.Count == 0)
            {
                UpdateStatus("No recording", ERROR);
            }
        }

        private void UpdateStatus(string status, System.Drawing.Color color)
        {
            if (statusLabel != null)
            {
                statusLabel.Text = $"● {status}";
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
