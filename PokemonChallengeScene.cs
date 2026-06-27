using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace idotmatrix_gui
{
    public class PokemonChallengeScene : IScene
    {
        public string Name => "Who's That Pokémon?";

        private bool _isLoaded = false;
        private string _pokemonName = "";
        private Color[,] _spritePixels = new Color[24, 24]; // Standard resized size to fit top area

        private DateTime _phaseStartTime = DateTime.MinValue;
        private readonly int _teaseDurationMs = 5000;
        private int _revealStartFrame = -1;
        private bool _isDone = false;

        private readonly HttpClient _httpClient = new HttpClient();

        public bool CustomCompletion => true;
        public bool IsDone => _isDone;

        public static string? ForcedPokemonIdOrName { get; set; } = null;
        public static bool IsForcedMode => !string.IsNullOrEmpty(ForcedPokemonIdOrName);
        public static event Action? RequestReset;

        public static void ResetActiveInstance()
        {
            RequestReset?.Invoke();
        }

        public PokemonChallengeScene()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "iDotMatrix-WPF-Client");
            RequestReset += HandleRequestReset;
        }

        private void HandleRequestReset()
        {
            _pokemonName = "";
            _isLoaded = false;
            _revealStartFrame = -1;
            _isDone = false;
        }

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // 1. Background (Bright Pokémon Yellow: 255, 204, 0)
            Color yellowBg = Color.FromRgb(255, 204, 0);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = yellowBg;
                }
            }

            // 2. Manage loading states
            if (!_isLoaded)
            {
                _isLoaded = true; // Set to true immediately to prevent double fetches
                _phaseStartTime = DateTime.Now;
                Task.Run(FetchRandomPokemonAsync);
            }

            if (string.IsNullOrEmpty(_pokemonName))
            {
                // Loading screen
                PixelFont.DrawText(canvas, "LOADING", 2, 10, Color.FromRgb(0, 0, 150));
                PixelFont.DrawText(canvas, "POKEMON", 2, 16, Color.FromRgb(0, 0, 150));
                return canvas;
            }

            // 3. Determine active phase
            double elapsedMs = (DateTime.Now - _phaseStartTime).TotalMilliseconds;
            bool isRevealPhase = elapsedMs > _teaseDurationMs;

            // Draw Silhouette or colored sprite centered in top 26 rows (y=0 to y=25)
            int spriteH = _spritePixels.GetLength(0);
            int spriteW = _spritePixels.GetLength(1);
            int offsetX = (32 - spriteW) / 2;
            int offsetY = (26 - spriteH) / 2;

            Color silhouetteColor = Color.FromRgb(20, 50, 120); // Dark Blue silhouette

            for (int sy = 0; sy < spriteH; sy++)
            {
                for (int sx = 0; sx < spriteW; sx++)
                {
                    Color px = _spritePixels[sy, sx];
                    if (px.A > 30) // Active pixel (non-transparent)
                    {
                        int targetX = offsetX + sx;
                        int targetY = offsetY + sy;
                        if (targetX >= 0 && targetX < 32 && targetY >= 0 && targetY < 32)
                        {
                            canvas[targetY, targetX] = isRevealPhase ? px : silhouetteColor;
                        }
                    }
                }
            }

            // 4. Draw Scrolling Text Banner at bottom (y=26 to y=31)
            // Dim text background by 50% for readability
            for (int x = 0; x < 32; x++)
            {
                for (int y = 26; y < 32; y++)
                {
                    Color px = canvas[y, x];
                    canvas[y, x] = Color.FromRgb((byte)(px.R * 0.5), (byte)(px.G * 0.5), (byte)(px.B * 0.5));
                }
            }

            string scrollText;
            Color textColor;

            int textWidth = 0;

            if (isRevealPhase)
            {
                // Reveal
                scrollText = $"IT'S {_pokemonName.ToUpper()}!  ~  ";
                textColor = Color.FromRgb(255, 50, 50); // Red reveal
                textWidth = PixelFont.MeasureTextWidth(scrollText);

                if (_revealStartFrame == -1)
                {
                    _revealStartFrame = frameCount;
                }
                else if (frameCount - _revealStartFrame >= 32 + textWidth)
                {
                    _isDone = true;
                }
                
                // Add a flashing border transition in reveal phase
                if ((frameCount / 3) % 2 == 0)
                {
                    Color flashColor = Color.FromRgb(255, 255, 255);
                    for (int x = 0; x < 32; x++) { canvas[0, x] = flashColor; canvas[25, x] = flashColor; }
                    for (int y = 0; y <= 25; y++) { canvas[y, 0] = flashColor; canvas[y, 31] = flashColor; }
                }
            }
            else
            {
                // Tease
                scrollText = "WHO'S THAT POKEMON?  ~  ";
                textColor = Color.FromRgb(255, 255, 255); // White query
                textWidth = PixelFont.MeasureTextWidth(scrollText);
            }

            int scrollRange = textWidth + 8;
            int textX = 32 - (frameCount % scrollRange);
            PixelFont.DrawText(canvas, scrollText, textX, 26, textColor);

            return canvas;
        }

        private async Task FetchRandomPokemonAsync()
        {
            try
            {
                string idOrName;
                if (IsForcedMode)
                {
                    idOrName = ForcedPokemonIdOrName!;
                }
                else
                {
                    Random rand = new Random();
                    idOrName = rand.Next(1, 650).ToString();
                }

                string apiUrl = $"https://pokeapi.co/api/v2/pokemon/{idOrName.ToLower().Trim()}";
                string json = await _httpClient.GetStringAsync(apiUrl);

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    _pokemonName = root.GetProperty("name").GetString() ?? "Unknown";
                    
                    // Extract Gen 5 Black/White sprite if available (clean pixel art), fallback to front_default
                    string? spriteUrl = null;
                    if (root.TryGetProperty("sprites", out var spritesProp))
                    {
                        if (spritesProp.TryGetProperty("versions", out var versionsProp) &&
                            versionsProp.TryGetProperty("generation-v", out var genVProp) &&
                            genVProp.TryGetProperty("black-white", out var bwProp) &&
                            bwProp.TryGetProperty("front_default", out var bwFrontProp))
                        {
                            spriteUrl = bwFrontProp.GetString();
                        }

                        if (string.IsNullOrEmpty(spriteUrl) && spritesProp.TryGetProperty("front_default", out var frontDefaultProp))
                        {
                            spriteUrl = frontDefaultProp.GetString();
                        }
                    }

                    if (!string.IsNullOrEmpty(spriteUrl))
                    {
                        byte[] imageBytes = await _httpClient.GetByteArrayAsync(spriteUrl);
                        using (MemoryStream ms = new MemoryStream(imageBytes))
                        {
                            // Load image via WPF BitmapDecoder
                            var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                            var frame = decoder.Frames[0];

                            // Convert frame to Bgra32 format to guarantee 4 bytes per pixel (prevents Indexed8/Bgr24 layout bugs)
                            var converted = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);

                            int origW = converted.PixelWidth;
                            int origH = converted.PixelHeight;
                            int stride = origW * 4;
                            byte[] pixels = new byte[origH * stride];
                            converted.CopyPixels(pixels, stride, 0);

                            // Find the bounding box of non-transparent pixels (alpha > 10)
                            int minX = origW;
                            int maxX = -1;
                            int minY = origH;
                            int maxY = -1;

                            for (int y = 0; y < origH; y++)
                            {
                                for (int x = 0; x < origW; x++)
                                {
                                    int idx = y * stride + x * 4;
                                    byte a = pixels[idx + 3];
                                    if (a > 10)
                                    {
                                        if (x < minX) minX = x;
                                        if (x > maxX) maxX = x;
                                        if (y < minY) minY = y;
                                        if (y > maxY) maxY = y;
                                    }
                                }
                            }

                            // If no pixels found or completely empty, default to full frame
                            if (maxX < minX || maxY < minY)
                            {
                                minX = 0;
                                minY = 0;
                                maxX = origW - 1;
                                maxY = origH - 1;
                            }

                            int cropW = maxX - minX + 1;
                            int cropH = maxY - minY + 1;

                            // Create CroppedBitmap using bounding box
                            var croppedBitmap = new CroppedBitmap(converted, new Int32Rect(minX, minY, cropW, cropH));

                            // Scale cropped image to fit the top area of the 32x32 display (y=0 to y=25, height 26, width 32)
                            double targetMaxW = 32.0;
                            double targetMaxH = 26.0;

                            double scaleX = targetMaxW / cropW;
                            double scaleY = targetMaxH / cropH;
                            double scaleVal = Math.Min(scaleX, scaleY);

                            if (scaleVal < 0.1) scaleVal = 1.0;

                            // Scale using TransformedBitmap (uses Bilinear filtering for smooth downsampling)
                            var scaleTransform = new ScaleTransform(scaleVal, scaleVal);
                            var scaledBitmap = new TransformedBitmap(croppedBitmap, scaleTransform);

                            int newW = scaledBitmap.PixelWidth;
                            int newH = scaledBitmap.PixelHeight;
                            int scaledStride = newW * 4;
                            byte[] scaledBytes = new byte[newH * scaledStride];
                            scaledBitmap.CopyPixels(scaledBytes, scaledStride, 0);

                            Color[,] scaledPixels = new Color[newH, newW];
                            for (int y = 0; y < newH; y++)
                            {
                                for (int x = 0; x < newW; x++)
                                {
                                    int idx = y * scaledStride + x * 4;
                                    byte b = scaledBytes[idx];
                                    byte g = scaledBytes[idx + 1];
                                    byte r = scaledBytes[idx + 2];
                                    byte a = scaledBytes[idx + 3];
                                    scaledPixels[y, x] = Color.FromArgb(a, r, g, b);
                                }
                            }

                            _spritePixels = scaledPixels;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fallback on request failure
                _pokemonName = "PIKACHU";
                GenerateFallbackPikachu();
            }
        }

        private void GenerateFallbackPikachu()
        {
            // Simple yellow/red pixel placeholder representing Pikachu
            Color[,] pikachu = new Color[24, 24];
            Color yellow = Color.FromRgb(255, 220, 0);
            Color red = Color.FromRgb(255, 0, 0);
            Color black = Color.FromRgb(0, 0, 0);

            // Pikachu cheeks
            pikachu[12, 6] = red; pikachu[12, 7] = red;
            pikachu[12, 16] = red; pikachu[12, 17] = red;

            // Eyes
            pikachu[8, 8] = black; pikachu[8, 15] = black;

            // Simple mouth
            pikachu[10, 11] = black; pikachu[10, 12] = black;
            
            _spritePixels = pikachu;
        }

        public void Stop()
        {
            _pokemonName = "";
            _isLoaded = false;
            _revealStartFrame = -1;
            _isDone = false;
        }
    }
}
