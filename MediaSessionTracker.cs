using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace idotmatrix_gui
{
    public class MediaSessionTracker
    {
        public static MediaSessionTracker Instance { get; } = new MediaSessionTracker();

        private GlobalSystemMediaTransportControlsSessionManager? _manager;
        private GlobalSystemMediaTransportControlsSession? _currentSession;

        public string Title { get; private set; } = "";
        public string Artist { get; private set; } = "";
        public byte[]? AlbumArtBytes { get; private set; }
        public double DurationSeconds { get; private set; } = 0;
        public double PositionSeconds { get; private set; } = 0;
        public bool IsPlaying { get; private set; } = false;

        private double _lastRawPosition = 0;
        private DateTime _lastPositionUpdate = DateTime.MinValue;

        public event Action? MediaPropertiesChanged;

        private MediaSessionTracker()
        {
            // Initialize on a background thread
            Task.Run(InitializeAsync);
        }

        private async Task InitializeAsync()
        {
            try
            {
                _manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                if (_manager != null)
                {
                    _manager.CurrentSessionChanged += Manager_CurrentSessionChanged;
                    UpdateSession(_manager.GetCurrentSession());
                }
            }
            catch (Exception)
            {
                // Fallback / ignore exceptions on systems without SMTC support
            }
        }

        private void Manager_CurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            UpdateSession(sender.GetCurrentSession());
        }

        private void UpdateSession(GlobalSystemMediaTransportControlsSession? session)
        {
            if (_currentSession != null)
            {
                _currentSession.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged -= Session_PlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged -= Session_TimelinePropertiesChanged;
            }

            _currentSession = session;

            if (_currentSession != null)
            {
                _currentSession.MediaPropertiesChanged += Session_MediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged += Session_PlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged += Session_TimelinePropertiesChanged;
                
                // Trigger immediate updates
                _ = RefreshMediaPropertiesAsync();
                RefreshPlaybackInfo();
                RefreshTimeline();
            }
            else
            {
                Title = "";
                Artist = "";
                AlbumArtBytes = null;
                DurationSeconds = 0;
                PositionSeconds = 0;
                IsPlaying = false;
                MediaPropertiesChanged?.Invoke();
            }
        }

        private void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
            _ = RefreshMediaPropertiesAsync();
        }

        private void Session_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            RefreshPlaybackInfo();
        }

        private void Session_TimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
        {
            RefreshTimeline();
        }

        private async Task RefreshMediaPropertiesAsync()
        {
            if (_currentSession == null) return;

            try
            {
                var props = await _currentSession.TryGetMediaPropertiesAsync();
                if (props != null)
                {
                    Title = props.Title ?? "Unknown Title";
                    Artist = props.Artist ?? "Unknown Artist";

                    // Read Album Art
                    if (props.Thumbnail != null)
                    {
                        using (var stream = await props.Thumbnail.OpenReadAsync())
                        {
                            if (stream.Size > 0)
                            {
                                byte[] bytes = new byte[stream.Size];
                                await stream.ReadAsync(bytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);
                                AlbumArtBytes = bytes;
                            }
                            else
                            {
                                AlbumArtBytes = null;
                            }
                        }
                    }
                    else
                    {
                        AlbumArtBytes = null;
                    }
                }
            }
            catch
            {
                Title = "Unknown Title";
                Artist = "Unknown Artist";
                AlbumArtBytes = null;
            }

            MediaPropertiesChanged?.Invoke();
        }

        private void RefreshPlaybackInfo()
        {
            if (_currentSession == null) return;

            var pbInfo = _currentSession.GetPlaybackInfo();
            if (pbInfo != null)
            {
                IsPlaying = pbInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            }
            MediaPropertiesChanged?.Invoke();
        }

        private void RefreshTimeline()
        {
            if (_currentSession == null) return;

            try
            {
                var timeline = _currentSession.GetTimelineProperties();
                if (timeline != null)
                {
                    DurationSeconds = timeline.EndTime.TotalSeconds;
                    PositionSeconds = timeline.Position.TotalSeconds;

                    _lastRawPosition = PositionSeconds;
                    _lastPositionUpdate = DateTime.Now;
                }
            }
            catch
            {
                DurationSeconds = 0;
                PositionSeconds = 0;
            }
            MediaPropertiesChanged?.Invoke();
        }

        public double GetExtrapolatedPosition()
        {
            if (!IsPlaying || DurationSeconds <= 0 || _lastPositionUpdate == DateTime.MinValue)
            {
                return PositionSeconds;
            }

            double elapsed = (DateTime.Now - _lastPositionUpdate).TotalSeconds;
            double pos = Math.Min(DurationSeconds, _lastRawPosition + elapsed);
            return pos;
        }
    }
}
