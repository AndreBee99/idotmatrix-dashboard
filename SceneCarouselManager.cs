using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class CarouselItem
    {
        public IScene Scene { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int DurationSeconds { get; set; } = 10;

        public CarouselItem(IScene scene, bool isEnabled = true, int durationSeconds = 10)
        {
            Scene = scene;
            IsEnabled = isEnabled;
            DurationSeconds = durationSeconds;
        }
    }

    public enum TransitionType
    {
        None,
        Melt,
        Glitch,
        Wave
    }

    public class SceneCarouselManager
    {
        public List<CarouselItem> Items { get; } = new List<CarouselItem>();
        
        private int _currentIndex = -1;
        private DateTime _sceneStartTime = DateTime.MinValue;
        private int _frameCountInScene = 0;

        // Transition tracking
        private int _transitionFramesLeft = 0;
        private const int TotalTransitionFrames = 20; // 1 second at 20 FPS
        private TransitionType _currentTransition = TransitionType.None;
        private Color[,] _outgoingBuffer = new Color[32, 32];
        private static readonly Random _rand = new Random();
        private NotificationAlert? _activeAlert = null;

        public void TriggerNotification(NotificationAlert alert)
        {
            _activeAlert = alert;
        }

        public SceneCarouselManager()
        {
            // Initialize default scenes
            Items.Add(new CarouselItem(new ClockScene(), true, 10));
            Items.Add(new CarouselItem(new WeatherScene(), true, 10));
            Items.Add(new CarouselItem(new MediaPlayerScene(), true, 15));
            Items.Add(new CarouselItem(new SystemMonitorScene(), true, 10));
            Items.Add(new CarouselItem(new DvdScreensaverScene(), true, 10));
            Items.Add(new CarouselItem(new VisualizerScene(), true, 10));
            Items.Add(new CarouselItem(new CalendarSyncScene(), false, 10)); // Disabled by default until URL is set
            Items.Add(new CarouselItem(new PokemonChallengeScene(), true, 10));
            Items.Add(new CarouselItem(new CountryGuessScene(), true, 10));
            Items.Add(new CarouselItem(new MathGameScene(), true, 10));
            Items.Add(new CarouselItem(new WebcamScene(), false, 10)); // Disabled by default
        }

        public CarouselItem? GetCurrentItem()
        {
            if (_currentIndex >= 0 && _currentIndex < Items.Count)
            {
                var item = Items[_currentIndex];
                if (item.IsEnabled) return item;
            }

            // Find first enabled item
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].IsEnabled)
                {
                    _currentIndex = i;
                    _sceneStartTime = DateTime.Now;
                    _frameCountInScene = 0;
                    return Items[i];
                }
            }

            return null;
        }

        public void SelectNextScene()
        {
            if (Items.Count == 0) return;

            // Save the current scene frame buffer for transition outgoing phase
            var currentItem = GetCurrentItem();
            if (currentItem != null)
            {
                _outgoingBuffer = currentItem.Scene.DrawFrame(_frameCountInScene);
                // Call Stop on the outgoing scene
                currentItem.Scene.Stop();
            }

            // Find next enabled scene
            int startIndex = _currentIndex;
            int nextIndex = _currentIndex;
            bool found = false;

            for (int i = 1; i <= Items.Count; i++)
            {
                int checkIdx = (startIndex + i) % Items.Count;
                if (Items[checkIdx].IsEnabled)
                {
                    nextIndex = checkIdx;
                    found = true;
                    break;
                }
            }

            if (found)
            {
                _currentIndex = nextIndex;
                _sceneStartTime = DateTime.Now;
                _frameCountInScene = 0;

                // Pick a random transition type
                var values = (TransitionType[])Enum.GetValues(typeof(TransitionType));
                _currentTransition = values[_rand.Next(1, values.Length)]; // Skip 'None'
                _transitionFramesLeft = TotalTransitionFrames;
            }
        }

        public Color[,] DrawFrame(int globalFrameCount)
        {
            if (_activeAlert != null)
            {
                double elapsed = (DateTime.Now - _activeAlert.ReceivedTime).TotalSeconds;
                if (elapsed < 6.0)
                {
                    return DrawNotificationAlert(_activeAlert, globalFrameCount);
                }
                else
                {
                    _activeAlert = null;
                }
            }

            var currentItem = GetCurrentItem();
            if (currentItem == null)
            {
                // Blank canvas if no scenes enabled
                Color[,] blank = new Color[32, 32];
                for (int y = 0; y < 32; y++)
                    for (int x = 0; x < 32; x++)
                        blank[y, x] = Color.FromRgb(0, 0, 0);
                return blank;
            }

            _frameCountInScene++;

            // Check if scene duration has elapsed
            double elapsedSeconds = (DateTime.Now - _sceneStartTime).TotalSeconds;
            if (elapsedSeconds >= currentItem.DurationSeconds && _transitionFramesLeft == 0)
            {
                SelectNextScene();
                currentItem = GetCurrentItem();
                if (currentItem == null) return new Color[32, 32];
            }

            // Get current active scene frame
            Color[,] currentFrame = currentItem.Scene.DrawFrame(_frameCountInScene);

            // Handle transition drawing
            if (_transitionFramesLeft > 0)
            {
                _transitionFramesLeft--;
                double progress = (double)(TotalTransitionFrames - _transitionFramesLeft) / TotalTransitionFrames;

                switch (_currentTransition)
                {
                    case TransitionType.Melt:
                        // Melt transition: first half melts outgoing, second half melts incoming
                        if (_transitionFramesLeft >= TotalTransitionFrames / 2)
                        {
                            double meltOutProgress = (progress * 2.0); // 0.0 to 1.0
                            return DistortionEffects.ApplyMeltDistortion(_outgoingBuffer, meltOutProgress);
                        }
                        else
                        {
                            double meltInProgress = 1.0 - ((progress - 0.5) * 2.0); // 1.0 to 0.0
                            return DistortionEffects.ApplyMeltDistortion(currentFrame, meltInProgress);
                        }

                    case TransitionType.Glitch:
                        // Apply Glitch to the incoming frame
                        return DistortionEffects.ApplyGlitchDistortion(currentFrame, _transitionFramesLeft);

                    case TransitionType.Wave:
                        // Wave transition that dampens over time
                        double waveAmp = 5.0 * (1.0 - progress);
                        return DistortionEffects.ApplyWaveDistortion(currentFrame, _transitionFramesLeft, waveAmp, 0.5);

                    default:
                        break;
                }
            }

            return currentFrame;
        }

        public void ForceChangeScene(int index)
        {
            if (index >= 0 && index < Items.Count && Items[index].IsEnabled)
            {
                var currentItem = GetCurrentItem();
                if (currentItem != null)
                {
                    _outgoingBuffer = currentItem.Scene.DrawFrame(_frameCountInScene);
                    currentItem.Scene.Stop();
                }

                _currentIndex = index;
                _sceneStartTime = DateTime.Now;
                _frameCountInScene = 0;
                
                // Set a quick transition
                _currentTransition = TransitionType.Glitch;
                _transitionFramesLeft = 10; 
            }
        }

        private Color[,] DrawNotificationAlert(NotificationAlert alert, int globalFrameCount)
        {
            Color[,] canvas = new Color[32, 32];
            
            Color bgColor = Color.FromRgb(10, 10, 22);
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                    canvas[y, x] = bgColor;

            double elapsedSeconds = (DateTime.Now - alert.ReceivedTime).TotalSeconds;

            // Phase 1 (0 to 2.5s): Large App Logo + App Name
            if (elapsedSeconds < 2.5)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        Color px = alert.LogoLarge[y, x];
                        if (px.R > 0 || px.G > 0 || px.B > 0)
                        {
                            canvas[3 + y, 8 + x] = px;
                        }
                    }
                }

                string name = alert.AppName.ToUpper();
                int nameWidth = PixelFont.MeasureTextWidth(name);
                int textX;
                if (nameWidth > 32)
                {
                    int range = nameWidth + 8;
                    textX = 32 - ((int)(elapsedSeconds * 40) % range);
                }
                else
                {
                    textX = (32 - nameWidth) / 2;
                }
                PixelFont.DrawText(canvas, name, textX, 23, Color.FromRgb(0, 240, 255));
            }
            // Phase 2 (2.5s to 6.0s): Small logo + Sender/Title + Message
            else
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        Color px = alert.LogoSmall[y, x];
                        if (px.R > 0 || px.G > 0 || px.B > 0)
                        {
                            canvas[2 + y, 1 + x] = px;
                        }
                    }
                }

                string title = alert.Title.ToUpper();
                int titleWidth = PixelFont.MeasureTextWidth(title);
                int titleX = 11;
                if (titleWidth > 21)
                {
                    int range = titleWidth + 8;
                    titleX = 32 - ((int)((elapsedSeconds - 2.5) * 30) % range);
                }
                
                Color[,] titleCanvas = new Color[32, 32];
                PixelFont.DrawText(titleCanvas, title, titleX, 3, Color.FromRgb(255, 0, 127));
                for (int y = 2; y < 10; y++)
                {
                    for (int x = 10; x < 32; x++)
                    {
                        if (titleCanvas[y, x].R > 0 || titleCanvas[y, x].G > 0 || titleCanvas[y, x].B > 0)
                        {
                            canvas[y, x] = titleCanvas[y, x];
                        }
                    }
                }

                Color lineCol = Color.FromRgb(40, 40, 60);
                for (int x = 0; x < 32; x++) canvas[12, x] = lineCol;

                string message = alert.Body.ToUpper();
                int msgWidth = PixelFont.MeasureTextWidth(message);
                int msgX = 2;
                if (msgWidth > 30)
                {
                    int range = msgWidth + 8;
                    msgX = 32 - ((int)((elapsedSeconds - 2.5) * 30) % range);
                }
                PixelFont.DrawText(canvas, message, msgX, 17, Color.FromRgb(255, 255, 255));
            }

            double pulse = Math.Sin(elapsedSeconds * Math.PI * 2) * 0.5 + 0.5;
            Color borderColor = Color.FromRgb((byte)(255 * pulse), (byte)(128 * pulse), 0);
            for (int x = 0; x < 32; x++) { canvas[0, x] = borderColor; canvas[31, x] = borderColor; }
            for (int y = 0; y < 32; y++) { canvas[y, 0] = borderColor; canvas[y, 31] = borderColor; }

            return canvas;
        }

        public void ApplyConfig(List<SceneConfig> sceneConfigs)
        {
            if (sceneConfigs == null || sceneConfigs.Count == 0) return;

            var newItemsList = new List<CarouselItem>();
            foreach (var sc in sceneConfigs)
            {
                var existingItem = Items.Find(item => item.Scene.Name == sc.Name);
                if (existingItem != null)
                {
                    existingItem.IsEnabled = sc.IsEnabled;
                    existingItem.DurationSeconds = sc.DurationSeconds;
                    newItemsList.Add(existingItem);
                    Items.Remove(existingItem);
                }
            }

            // Append any remaining items that were not specified in config
            newItemsList.AddRange(Items);

            Items.Clear();
            Items.AddRange(newItemsList);
        }

        public List<SceneConfig> GetConfig()
        {
            var list = new List<SceneConfig>();
            foreach (var item in Items)
            {
                list.Add(new SceneConfig
                {
                    Name = item.Scene.Name,
                    IsEnabled = item.IsEnabled,
                    DurationSeconds = item.DurationSeconds
                });
            }
            return list;
        }
    }
}
