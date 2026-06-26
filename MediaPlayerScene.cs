using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace idotmatrix_gui
{
    public class MediaPlayerScene : IScene
    {
        public string Name => "Media Player";

        private string _cachedTitle = "";
        private string _cachedArtist = "";
        private Color[,] _albumArtPixels = new Color[31, 32]; // 32x31 (excludes progress bar row 0)
        private Color[,] _fallbackArt = new Color[31, 32];

        // Screensaver variables
        private string? _activeDistortion = null;
        private double _distortionDuration = 0;
        private DateTime _distortionEndTime = DateTime.MinValue;
        private DateTime _nextDistortionTime = DateTime.MinValue;
        private readonly Random _rand = new Random();

        public MediaPlayerScene()
        {
            GenerateFallbackArt();
            // Schedule first distortion in 20-30 seconds
            _nextDistortionTime = DateTime.Now.AddSeconds(_rand.Next(20, 31));
        }

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // 1. Sync metadata and decode album art if song changed
            string title = MediaSessionTracker.Instance.Title;
            string artist = MediaSessionTracker.Instance.Artist;

            if (title != _cachedTitle || artist != _cachedArtist)
            {
                // Song changed!
                bool isSubsequentSong = !string.IsNullOrEmpty(_cachedTitle);
                
                _cachedTitle = title;
                _cachedArtist = artist;

                // Decode new art
                var artBytes = MediaSessionTracker.Instance.AlbumArtBytes;
                if (artBytes != null)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(artBytes))
                        {
                            var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            var frame = decoder.Frames[0];
                            
                            // Scale to 32x32 using Nearest Neighbor
                            var scale = new TransformedBitmap(frame, new ScaleTransform(32.0 / frame.PixelWidth, 32.0 / frame.PixelHeight));
                            var writeable = new WriteableBitmap(scale);
                            
                            int stride = 32 * 4;
                            byte[] rawBytes = new byte[32 * stride];
                            writeable.CopyPixels(rawBytes, stride, 0);

                            // Crop top row and store as 32x31
                            Color[,] tempPixels = new Color[31, 32];
                            for (int y = 0; y < 31; y++)
                            {
                                int srcY = y + 1; // Skip top row (y=0)
                                for (int x = 0; x < 32; x++)
                                {
                                    int idx = srcY * stride + x * 4;
                                    byte b = rawBytes[idx];
                                    byte g = rawBytes[idx + 1];
                                    byte r = rawBytes[idx + 2];
                                    byte a = rawBytes[idx + 3];
                                    tempPixels[y, x] = Color.FromRgb(r, g, b);
                                }
                            }
                            _albumArtPixels = tempPixels;
                        }
                    }
                    catch
                    {
                        _albumArtPixels = _fallbackArt;
                    }
                }
                else
                {
                    _albumArtPixels = _fallbackArt;
                }

                // Trigger a transition distortion on song change
                if (isSubsequentSong)
                {
                    string[] trans = new[] { "melt", "glitch", "wave" };
                    _activeDistortion = trans[_rand.Next(trans.Length)];
                    _distortionDuration = 2.5; // 2.5 seconds
                    _distortionEndTime = DateTime.Now.AddSeconds(_distortionDuration);
                }
            }

            // 2. Manage random screensaver distortions
            var now = DateTime.Now;
            if (now >= _nextDistortionTime && _activeDistortion == null)
            {
                string[] effects = new[] { "melt", "glitch", "wave" };
                _activeDistortion = effects[_rand.Next(effects.Length)];
                _distortionDuration = 3.5; // 3.5 seconds
                _distortionEndTime = now.AddSeconds(_distortionDuration);
                _nextDistortionTime = now.AddSeconds(_rand.Next(20, 46));
            }

            // 3. Render base components
            // A. Draw Progress Bar at y=0
            double duration = MediaSessionTracker.Instance.DurationSeconds;
            double position = MediaSessionTracker.Instance.GetExtrapolatedPosition();
            int progressWidth = 0;
            if (duration > 0)
            {
                progressWidth = (int)Math.Round((position / duration) * 32.0);
                progressWidth = Math.Max(0, Math.Min(32, progressWidth));
            }

            // Bass glow progress bar color
            Color progressColor;
            if (AudioCapture.Instance.CurrentBassFactor > 0)
            {
                byte rVal = (byte)(60 + 195 * AudioCapture.Instance.CurrentBassFactor);
                progressColor = Color.FromRgb(rVal, 0, 0);
            }
            else
            {
                progressColor = Color.FromRgb(255, 0, 0); // Standard Red
            }

            Color progressBg = Color.FromRgb(30, 30, 30);
            for (int x = 0; x < 32; x++)
            {
                canvas[0, x] = x < progressWidth ? progressColor : progressBg;
            }

            // B. Apply distortion to the 32x31 album art if active
            Color[,] finalArt;
            if (_activeDistortion != null && now < _distortionEndTime)
            {
                double remaining = (_distortionEndTime - now).TotalSeconds;
                if (_activeDistortion == "wave")
                {
                    double amp = 4.5 * (remaining / _distortionDuration);
                    finalArt = DistortionEffects.ApplyWaveDistortion(_albumArtPixels, frameCount, amp);
                }
                else if (_activeDistortion == "melt")
                {
                    double progress = Math.Sin((1.0 - remaining / _distortionDuration) * Math.PI);
                    progress = Math.Max(0.0, Math.Min(1.0, progress));
                    finalArt = DistortionEffects.ApplyMeltDistortion(_albumArtPixels, progress);
                }
                else // glitch
                {
                    finalArt = DistortionEffects.ApplyGlitchDistortion(_albumArtPixels, frameCount);
                }
            }
            else
            {
                _activeDistortion = null;
                finalArt = _albumArtPixels;
            }

            // Paste finalArt onto canvas starting at y=1
            for (int y = 0; y < 31; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y + 1, x] = finalArt[y, x];
                }
            }

            // C. Draw scrolling text overlay over dimmed bottom band (y=26 to y=31)
            // Dim background by 70% (30% remaining brightness)
            for (int y = 26; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    Color px = canvas[y, x];
                    canvas[y, x] = Color.FromRgb((byte)(px.R * 0.3), (byte)(px.G * 0.3), (byte)(px.B * 0.3));
                }
            }

            // Compile text: "Title - Artist"
            string scrollText;
            if (string.IsNullOrEmpty(title))
            {
                scrollText = "WAITING FOR MUSIC...  ~  ";
            }
            else
            {
                scrollText = $"{title} - {artist}  ~  ";
            }

            int textWidth = PixelFont.MeasureTextWidth(scrollText);
            int scrollRange = textWidth + 8;
            int textX = 32 - (frameCount % scrollRange);
            PixelFont.DrawText(canvas, scrollText, textX, 26, Color.FromRgb(255, 255, 255));

            return canvas;
        }

        private void GenerateFallbackArt()
        {
            // Creates a green Spotify-like logo for fallback when no album art is available
            Color darkBg = Color.FromRgb(18, 18, 18);
            for (int y = 0; y < 31; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    _fallbackArt[y, x] = darkBg;
                }
            }

            // Draw a simple green Spotify circle outline
            Color green = Color.FromRgb(30, 215, 96);
            int centerX = 16;
            int centerY = 15;
            int radius = 8;
            
            // Draw a basic circle
            for (int y = 0; y < 31; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    double dist = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                    if (Math.Abs(dist - radius) < 1.0)
                    {
                        _fallbackArt[y, x] = green;
                    }
                }
            }
            // Draw 3 soundwaves
            _fallbackArt[13, 12] = green; _fallbackArt[13, 13] = green; _fallbackArt[13, 14] = green; _fallbackArt[13, 15] = green;
            _fallbackArt[15, 11] = green; _fallbackArt[15, 12] = green; _fallbackArt[15, 13] = green; _fallbackArt[15, 14] = green;
        }

        public void Stop() { }
    }
}
