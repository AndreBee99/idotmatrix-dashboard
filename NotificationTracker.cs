using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;

namespace idotmatrix_gui
{
    public class NotificationAlert
    {
        public string AppName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Body { get; set; } = "";
        public Color[,] LogoLarge { get; set; } = new Color[16, 16];
        public Color[,] LogoSmall { get; set; } = new Color[8, 8];
        public DateTime ReceivedTime { get; set; }
    }

    public class NotificationTracker
    {
        public static NotificationTracker Instance { get; } = new NotificationTracker();

        private UserNotificationListener? _listener;
        private bool _isInitialized = false;
        private readonly HashSet<uint> _knownNotificationIds = new HashSet<uint>();
        private bool _isFirstPoll = true;

        public event Action<NotificationAlert>? NotificationReceived;
        public event Action<string>? LogMessage;

        private NotificationTracker() { }

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return true;

            try
            {
                if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.Notifications.Management.UserNotificationListener"))
                {
                    LogMessage?.Invoke("System Notification Listener is not supported on this Windows version.");
                    return false;
                }

                _listener = UserNotificationListener.Current;
                
                var status = await _listener.RequestAccessAsync();
                if (status == UserNotificationListenerAccessStatus.Allowed)
                {
                    LogMessage?.Invoke("Notification access permission allowed.");

                    try
                    {
                        // Try subscribing to direct events (succeeds on packaged apps)
                        _listener.NotificationChanged += Listener_NotificationChanged;
                        LogMessage?.Invoke("Successfully subscribed to direct notification events.");
                    }
                    catch (Exception ex)
                    {
                        // Fallback: Windows throws 0x80070490 (Element Not Found) on unpackaged apps
                        LogMessage?.Invoke($"[Diagnostics] Event subscription failed: {ex.Message}. Falling back to active polling mode.");
                        
                        _isInitialized = true;
                        _isFirstPoll = true;
                        _knownNotificationIds.Clear();
                        
                        // Spin up the 500ms background polling loop
                        _ = Task.Run(StartPollingLoopAsync);
                        return true;
                    }

                    _isInitialized = true;
                    return true;
                }
                else
                {
                    LogMessage?.Invoke($"Notification access was denied (Status: {status}). Please go to Windows Settings -> System -> Notifications and ensure access is enabled.");
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Failed to initialize Notification listener: {ex}");
            }

            return false;
        }

        private void Listener_NotificationChanged(UserNotificationListener sender, UserNotificationChangedEventArgs args)
        {
            try
            {
                var notification = sender.GetNotification(args.UserNotificationId);
                if (notification == null) return;
                ProcessNotification(notification);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Error in direct notification event: {ex.Message}");
            }
        }

        private async Task StartPollingLoopAsync()
        {
            LogMessage?.Invoke("[Diagnostics] Notification polling loop started.");
            while (_isInitialized && _listener != null)
            {
                try
                {
                    // Query only toast notifications active in Action Center
                    var notifications = await _listener.GetNotificationsAsync(NotificationKinds.Toast);
                    var activeIds = new HashSet<uint>();

                    foreach (var notif in notifications)
                    {
                        activeIds.Add(notif.Id);
                        if (!_knownNotificationIds.Contains(notif.Id))
                        {
                            _knownNotificationIds.Add(notif.Id);
                            // Only trigger alerts for notifications received while the app is active
                            if (!_isFirstPoll)
                            {
                                ProcessNotification(notif);
                            }
                        }
                    }

                    // Remove dismissed/cleared notifications from our tracking cache
                    _knownNotificationIds.RemoveWhere(id => !activeIds.Contains(id));
                    _isFirstPoll = false;
                }
                catch (Exception)
                {
                    // Ignore transient COM issues during polling
                }

                await Task.Delay(500); // Check every 500ms
            }
        }

        private async void ProcessNotification(UserNotification notification)
        {
            try
            {
                // 1. Get app details (Safely wrapped because AppInfo throws NotImplementedException on unpackaged apps)
                string appName = "Notification";
                try
                {
                    if (notification.AppInfo?.DisplayInfo != null)
                    {
                        appName = notification.AppInfo.DisplayInfo.DisplayName ?? "Notification";
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"[Diagnostics] AppInfoDisplayName failed: {ex.Message}");
                    appName = "Notification";
                }

                string title = appName;
                string body = "New alert";

                // 2. Extract notification visual binding & text
                try
                {
                    var appNotification = notification.Notification;
                    if (appNotification != null)
                    {
                        var visual = appNotification.Visual;
                        if (visual != null)
                        {
                            NotificationBinding? toastBinding = null;
                            try
                            {
                                toastBinding = visual.GetBinding(KnownNotificationBindings.ToastGeneric);
                            }
                            catch (Exception ex)
                            {
                                LogMessage?.Invoke($"[Diagnostics] GetBinding ToastGeneric failed: {ex.Message}");
                            }

                            if (toastBinding == null)
                            {
                                try
                                {
                                    var bindings = visual.Bindings;
                                    if (bindings != null && bindings.Count > 0)
                                    {
                                        toastBinding = bindings[0];
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogMessage?.Invoke($"[Diagnostics] Bindings access failed: {ex.Message}");
                                }
                            }

                            if (toastBinding != null)
                            {
                                var textElements = toastBinding.GetTextElements();
                                if (textElements != null && textElements.Any())
                                {
                                    title = textElements.First().Text ?? appName;
                                    body = textElements.Count() > 1 
                                        ? string.Join(" | ", textElements.Skip(1).Select(t => t.Text ?? "")) 
                                        : "";
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"[Diagnostics] Visual/Text extraction failed: {ex.GetType().Name} - {ex.Message}");
                }

                LogMessage?.Invoke($"[Notification] App: '{appName}', Title: '{title}', Body: '{body}'");

                // 3. Load Logo images (Safely wrapped because AppInfo.GetLogo throws on unpackaged apps)
                byte[]? logoBytes = null;
                try
                {
                    if (notification.AppInfo?.DisplayInfo != null)
                    {
                        var logoRef = notification.AppInfo.DisplayInfo.GetLogo(new Windows.Foundation.Size(32, 32));
                        if (logoRef != null)
                        {
                            using (var stream = await logoRef.OpenReadAsync())
                            {
                                logoBytes = new byte[stream.Size];
                                await stream.ReadAsync(logoBytes.AsBuffer(), (uint)stream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogMessage?.Invoke($"[Diagnostics] Logo load failed: {ex.Message}");
                }

                // Load logos: try custom hand-crafted registry first, then fallback to decoded bytes or generic
                Color[,] logoLarge;
                Color[,] logoSmall;

                if (logoBytes != null && logoBytes.Length > 0)
                {
                    logoLarge = DecodeLogoBytes(logoBytes, 16);
                    logoSmall = DecodeLogoBytes(logoBytes, 8);
                }
                else
                {
                    var logos = NotificationLogoRegistry.GetLogos(appName);
                    logoLarge = logos.Large;
                    logoSmall = logos.Small;
                }

                var alert = new NotificationAlert
                {
                    AppName = appName,
                    Title = title,
                    Body = body,
                    LogoLarge = logoLarge,
                    LogoSmall = logoSmall,
                    ReceivedTime = DateTime.Now
                };

                NotificationReceived?.Invoke(alert);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke($"Error parsing notification payload: {ex.GetType().FullName} - {ex.Message}\n{ex.StackTrace}");
            }
        }

        private Color[,] DecodeLogoBytes(byte[]? logoBytes, int targetSize)
        {
            Color[,] pixels = new Color[targetSize, targetSize];
            Color fallback = Color.FromRgb(0, 240, 255); // Cyan backup
            
            if (logoBytes == null || logoBytes.Length == 0)
            {
                for (int y = 0; y < targetSize; y++)
                    for (int x = 0; x < targetSize; x++)
                        pixels[y, x] = fallback;
                return pixels;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream(logoBytes))
                {
                    var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    var frame = decoder.Frames[0];

                    var scale = new TransformedBitmap(frame, new ScaleTransform((double)targetSize / frame.PixelWidth, (double)targetSize / frame.PixelHeight));
                    var writeable = new WriteableBitmap(scale);

                    int stride = targetSize * 4;
                    byte[] rawBytes = new byte[targetSize * stride];
                    writeable.CopyPixels(rawBytes, stride, 0);

                    for (int y = 0; y < targetSize; y++)
                    {
                        for (int x = 0; x < targetSize; x++)
                        {
                            int idx = y * stride + x * 4;
                            byte b = rawBytes[idx];
                            byte g = rawBytes[idx + 1];
                            byte r = rawBytes[idx + 2];
                            byte a = rawBytes[idx + 3];

                            // Blend transparent background with solid black
                            if (a < 50)
                            {
                                pixels[y, x] = Color.FromRgb(0, 0, 0);
                            }
                            else
                            {
                                pixels[y, x] = Color.FromRgb(r, g, b);
                            }
                        }
                    }
                }
            }
            catch
            {
                for (int y = 0; y < targetSize; y++)
                    for (int x = 0; x < targetSize; x++)
                        pixels[y, x] = fallback;
            }

            return pixels;
        }
    }
}
