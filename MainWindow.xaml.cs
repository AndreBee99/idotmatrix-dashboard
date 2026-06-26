using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace idotmatrix_gui
{
    public partial class MainWindow : Window
    {
        private readonly SceneCarouselManager _carouselManager = new SceneCarouselManager();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private int _globalFrameCount = 0;

        // Preview rendering buffers
        private WriteableBitmap? _previewBitmap;
        private readonly byte[] _pixelBuffer = new byte[32 * 32 * 4];
        private bool _isSendingBle = false;

        // System Tray & Configuration Persistence fields
        private System.Windows.Forms.NotifyIcon? _notifyIcon;
        private bool _isExplicitClose = false;

        public MainWindow()
        {
            InitializeComponent();
            
            // Register for window lifecycle events
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize logging & BLE notifications
            BleManager.Instance.LogMessage += BleManager_LogMessage;
            BleManager.Instance.ConnectionStateChanged += BleManager_ConnectionStateChanged;

            // Load saved settings & configure system tray
            LoadSavedConfig();
            InitializeTrayIcon();

            // Bind the ListView to our Carousel items
            LvPlaylist.ItemsSource = _carouselManager.Items;

            // Initialize local LED pixel preview
            InitializePreview();

            // Start system audio capture loopback
            try
            {
                AudioCapture.Instance.Start(null, true);
                LogToTerminal("Started WASAPI Loopback audio capture.");
            }
            catch (Exception ex)
            {
                LogToTerminal($"Failed to start audio capture: {ex.Message}");
                TxtAudioStatus.Text = "Audio capture unavailable.";
                TxtAudioStatus.Foreground = Brushes.Red;
            }

            // Configure 20 FPS frame tick timer
            _timer.Interval = TimeSpan.FromMilliseconds(50);
            _timer.Tick += OnFrameTick;
            _timer.Start();

            // Initialize system notification tracker (executes on UI thread for WinRT dialog compatibility)
            NotificationTracker.Instance.LogMessage += BleManager_LogMessage;
            NotificationTracker.Instance.NotificationReceived += NotificationReceivedHandler;
            _ = InitializeNotificationTrackerAsync();

            LogToTerminal("Application initialized. Ready.");
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            // Stop timers & capture
            _timer.Stop();
            AudioCapture.Instance.Stop();
            BleManager.Instance.Disconnect();
            CameraCapture.Instance.CleanUp();
        }

        private void NotificationReceivedHandler(NotificationAlert alert)
        {
            _carouselManager.TriggerNotification(alert);
        }

        private async Task InitializeNotificationTrackerAsync()
        {
            try
            {
                bool success = await NotificationTracker.Instance.InitializeAsync();
                if (success)
                {
                    LogToTerminal("System Notification listener is active.");
                }
            }
            catch (Exception ex)
            {
                LogToTerminal($"Failed to initialize notification listener: {ex.Message}");
            }
        }

        private void InitializePreview()
        {
            _previewBitmap = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Bgra32, null);
            ImgLedPreview.Source = _previewBitmap;
        }

        private void OnFrameTick(object? sender, EventArgs e)
        {
            _globalFrameCount++;

            // Draw current scene frame
            Color[,] pixels = _carouselManager.DrawFrame(_globalFrameCount);

            // Update local LED simulation
            UpdateLocalPreview(pixels);

            // Update volume level UI meter
            PbAudioLevel.Value = AudioCapture.Instance.CurrentBassFactor;

            // Check FPS status (simulate 20 FPS)
            TxtFps.Text = "20 FPS";

            // Ship payload to physical display via BLE if connected (throttled to prevent write overflow)
            if (BleManager.Instance.IsConnected && !_isSendingBle)
            {
                _isSendingBle = true;
                Task.Run(async () =>
                {
                    try
                    {
                        byte[] pngData = ConvertPixelsToPng(pixels);
                        await BleManager.Instance.SendImagePayloadAsync(pngData);
                    }
                    catch (Exception ex)
                    {
                        LogToTerminal($"BLE Write error: {ex.Message}");
                    }
                    finally
                    {
                        _isSendingBle = false;
                    }
                });
            }
        }

        private void UpdateLocalPreview(Color[,] pixels)
        {
            if (_previewBitmap == null) return;

            int stride = 32 * 4;
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Color c = pixels[y, x];
                    int idx = y * stride + x * 4;
                    _pixelBuffer[idx] = c.B;
                    _pixelBuffer[idx + 1] = c.G;
                    _pixelBuffer[idx + 2] = c.R;
                    _pixelBuffer[idx + 3] = 255; // Alpha opaque
                }
            }

            _previewBitmap.WritePixels(new Int32Rect(0, 0, 32, 32), _pixelBuffer, stride, 0);
        }

        private byte[] ConvertPixelsToPng(Color[,] pixels)
        {
            // Render the pixels to a temporary BitmapSource
            int width = pixels.GetLength(1);
            int height = pixels.GetLength(0);
            int stride = width * 4;
            byte[] rawBytes = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = pixels[y, x];
                    int idx = y * stride + x * 4;
                    rawBytes[idx] = c.B;
                    rawBytes[idx + 1] = c.G;
                    rawBytes[idx + 2] = c.R;
                    rawBytes[idx + 3] = 255;
                }
            }

            var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, rawBytes, stride);

            // Encode bitmap frame to PNG bytes
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var ms = new MemoryStream())
            {
                encoder.Save(ms);
                return ms.ToArray();
            }
        }

        private void LogToTerminal(string msg)
        {
            TxtLogTerminal.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
            TxtLogTerminal.ScrollToEnd();
        }

        private void BleManager_LogMessage(string message)
        {
            Dispatcher.InvokeAsync(() =>
            {
                LogToTerminal(message);
            });
        }

        private void BleManager_ConnectionStateChanged()
        {
            Dispatcher.InvokeAsync(() =>
            {
                bool isConnected = BleManager.Instance.IsConnected;
                if (isConnected)
                {
                    ElStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(30, 215, 96)); // Retro Green
                    TxtStatus.Text = $"Connected to {BleManager.Instance.ConnectedDeviceName ?? "Display"}";
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(30, 215, 96));
                    BtnConnect.Visibility = Visibility.Collapsed;
                    BtnDisconnect.Visibility = Visibility.Visible;
                }
                else
                {
                    ElStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 79)); // Hot Pink
                    TxtStatus.Text = "Disconnected";
                    TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 79));
                    BtnConnect.Visibility = Visibility.Visible;
                    BtnDisconnect.Visibility = Visibility.Collapsed;
                }
            });
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            BtnConnect.IsEnabled = false;
            TxtStatus.Text = "Connecting...";
            TxtStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 204, 0));
            ElStatusIndicator.Fill = new SolidColorBrush(Color.FromRgb(255, 204, 0)); // Yellow

            string targetMac = TxtMacAddress.Text.Trim();
            bool connected = false;

            // 1. Try scanless direct connection if a valid MAC is provided
            if (!string.IsNullOrEmpty(targetMac) && targetMac != "00:00:00:00:00:00")
            {
                LogToTerminal($"Attempting scanless direct connection to MAC: {targetMac}");
                connected = await Task.Run(() => BleManager.Instance.ConnectByAddressAsync(targetMac));
            }

            // 2. Scan and connect automatically if no MAC was provided or direct connection failed
            if (!connected)
            {
                LogToTerminal("MAC address not specified or connection failed. Scanning automatically for nearby iDotMatrix ('IDM-') displays...");
                var idmDevices = await Task.Run(() => BleManager.Instance.ScanForDevicesAsync());
                if (idmDevices.Count > 0)
                {
                    var target = idmDevices[0];
                    LogToTerminal($"Found matching display: {target.Name}. Connecting...");
                    connected = await Task.Run(() => BleManager.Instance.ConnectAsync(target.Id));

                    if (connected)
                    {
                        // Retrieve the connected device's MAC address and populate it in the UI/Config
                        string? resolvedMac = BleManager.Instance.ConnectedDeviceAddress;
                        if (!string.IsNullOrEmpty(resolvedMac))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                TxtMacAddress.Text = resolvedMac;
                            });
                            SaveCurrentConfig();
                            LogToTerminal($"Automatically resolved and saved device MAC address: {resolvedMac}");
                        }
                    }
                }
                else
                {
                    LogToTerminal("No 'IDM-' devices found during scanning.");
                }
            }

            if (connected)
            {
                LogToTerminal("Connection established. Sending DIY mode command...");
                bool modeSet = await BleManager.Instance.SendModeCommandAsync(1);
                LogToTerminal(modeSet ? "DIY Mode activated." : "Failed to set DIY Mode.");
            }
            else
            {
                LogToTerminal("Failed to connect to display.");
                BleManager_ConnectionStateChanged();
            }

            BtnConnect.IsEnabled = true;
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            BleManager.Instance.Disconnect();
        }

        private async void BtnForceDiy_Click(object sender, RoutedEventArgs e)
        {
            if (BleManager.Instance.IsConnected)
            {
                LogToTerminal("Sending DIY mode activation...");
                bool success = await BleManager.Instance.SendModeCommandAsync(1);
                LogToTerminal(success ? "DIY Mode command completed." : "Failed to send DIY Mode command.");
            }
            else
            {
                LogToTerminal("Cannot send mode: Display not connected.");
            }
        }

        // Scene Playlist Actions
        private void BtnPlayScene_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CarouselItem item)
            {
                int index = _carouselManager.Items.IndexOf(item);
                if (index >= 0)
                {
                    _carouselManager.ForceChangeScene(index);
                    LogToTerminal($"Manually playing scene: {item.Scene.Name}");
                }
            }
        }

        private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CarouselItem item)
            {
                int index = _carouselManager.Items.IndexOf(item);
                if (index > 0)
                {
                    _carouselManager.Items.RemoveAt(index);
                    _carouselManager.Items.Insert(index - 1, item);
                    LvPlaylist.Items.Refresh();
                    LogToTerminal($"Moved '{item.Scene.Name}' up in carousel cycle.");
                    SaveCurrentConfig();
                }
            }
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is CarouselItem item)
            {
                int index = _carouselManager.Items.IndexOf(item);
                if (index >= 0 && index < _carouselManager.Items.Count - 1)
                {
                    _carouselManager.Items.RemoveAt(index);
                    _carouselManager.Items.Insert(index + 1, item);
                    LvPlaylist.Items.Refresh();
                    LogToTerminal($"Moved '{item.Scene.Name}' down in carousel cycle.");
                    SaveCurrentConfig();
                }
            }
        }

        private void BtnForceNext_Click(object sender, RoutedEventArgs e)
        {
            _carouselManager.SelectNextScene();
            var current = _carouselManager.GetCurrentItem();
            if (current != null)
            {
                LogToTerminal($"Forced transition to next scene: {current.Scene.Name}");
            }
        }

        private void OnPlaylistItemChanged(object sender, RoutedEventArgs e)
        {
            // ListView items reflect states automatically via TwoWay binding.
            // Refresh playlist binding just in case
            LvPlaylist.Items.Refresh();
            SaveCurrentConfig();
        }

        private void OnDurationLostFocus(object sender, RoutedEventArgs e)
        {
            // Just refresh list values
            LvPlaylist.Items.Refresh();
            SaveCurrentConfig();
        }

        // Global Configurations
        private async void BtnSaveWeather_Click(object sender, RoutedEventArgs e)
        {
            string city = TxtWeatherCity.Text.Trim();
            if (!string.IsNullOrEmpty(city))
            {
                BtnSaveWeather.IsEnabled = false;
                LogToTerminal($"Resolving coordinates for city: {city}...");
                
                bool success = await Task.Run(() => WeatherScene.ResolveCityCoordinatesAsync(city));
                if (success)
                {
                    TxtCoordinates.Text = $"Lat: {WeatherScene.Latitude:F4}, Lon: {WeatherScene.Longitude:F4}";
                    LogToTerminal($"Successfully resolved {city} to Lat={WeatherScene.Latitude:F4}, Lon={WeatherScene.Longitude:F4}");
                    SaveCurrentConfig();
                }
                else
                {
                    LogToTerminal($"Failed to resolve coordinates for {city}. Standard backup city will be used.");
                }
                BtnSaveWeather.IsEnabled = true;
            }
        }

        private void BtnSaveCalendar_Click(object sender, RoutedEventArgs e)
        {
            string url = TxtCalendarUrl.Text.Trim();
            CalendarSyncScene.ICalUrl = url;
            LogToTerminal($"Calendar iCal URL updated.");

            // Auto-enable/disable calendar scene based on presence of URL
            var calItem = _carouselManager.Items.Find(item => item.Scene is CalendarSyncScene);
            if (calItem != null)
            {
                bool hasUrl = !string.IsNullOrEmpty(url);
                if (calItem.IsEnabled != hasUrl)
                {
                    calItem.IsEnabled = hasUrl;
                    LvPlaylist.Items.Refresh();
                    LogToTerminal($"Calendar Sync scene is now {(hasUrl ? "enabled" : "disabled")}.");
                }
            }
            SaveCurrentConfig();
        }

        // Config Saving & Loading Helpers
        private void SaveCurrentConfig()
        {
            try
            {
                var config = new AppConfig
                {
                    Scenes = _carouselManager.GetConfig(),
                    WeatherCity = TxtWeatherCity.Text.Trim(),
                    Latitude = WeatherScene.Latitude,
                    Longitude = WeatherScene.Longitude,
                    ICalUrl = CalendarSyncScene.ICalUrl ?? "",
                    TargetMac = TxtMacAddress.Text.Trim()
                };
                ConfigManager.SaveConfig(config);
            }
            catch (Exception ex)
            {
                LogToTerminal($"Failed to save configuration: {ex.Message}");
            }
        }

        private void LoadSavedConfig()
        {
            try
            {
                var config = ConfigManager.LoadConfig();
                
                // 1. Load scenes config
                if (config.Scenes != null && config.Scenes.Count > 0)
                {
                    _carouselManager.ApplyConfig(config.Scenes);
                }

                // 2. Load weather config
                if (!string.IsNullOrEmpty(config.WeatherCity))
                {
                    TxtWeatherCity.Text = config.WeatherCity;
                    WeatherScene.Latitude = config.Latitude;
                    WeatherScene.Longitude = config.Longitude;
                    TxtCoordinates.Text = $"Lat: {WeatherScene.Latitude:F4}, Lon: {WeatherScene.Longitude:F4}";
                }

                // 3. Load calendar config
                if (!string.IsNullOrEmpty(config.ICalUrl))
                {
                    TxtCalendarUrl.Text = config.ICalUrl;
                    CalendarSyncScene.ICalUrl = config.ICalUrl;
                }

                // 4. Load MAC Address
                if (!string.IsNullOrEmpty(config.TargetMac))
                {
                    TxtMacAddress.Text = config.TargetMac;
                }
                else
                {
                    TxtMacAddress.Text = "A0:3E:49:90:79:83"; // Default for current user
                }
            }
            catch (Exception ex)
            {
                LogToTerminal($"Failed to load configuration: {ex.Message}");
            }
        }

        // System Tray Integration
        private void InitializeTrayIcon()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Text = "iDotMatrix Dashboard";

            try
            {
                var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath) && System.IO.File.Exists(exePath))
                {
                    _notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                }
            }
            catch
            {
                _notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            _notifyIcon.Visible = true;
            _notifyIcon.DoubleClick += (s, e) => RestoreFromTray();

            // Setup context menu
            var contextMenu = new System.Windows.Forms.ContextMenuStrip();
            
            var restoreItem = new System.Windows.Forms.ToolStripMenuItem("Restore");
            restoreItem.Click += (s, e) => RestoreFromTray();
            
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => ExitApplication();

            contextMenu.Items.Add(restoreItem);
            contextMenu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            _isExplicitClose = true;
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            Close();
            System.Windows.Application.Current.Shutdown();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExplicitClose)
            {
                e.Cancel = true;
                Hide();
                if (_notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(3000, "iDotMatrix Dashboard", "Minimized to tray. Double-click the icon to restore.", System.Windows.Forms.ToolTipIcon.Info);
                }
            }
            else
            {
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
            base.OnStateChanged(e);
        }
    }
}