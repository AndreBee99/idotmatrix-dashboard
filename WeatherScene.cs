using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class WeatherScene : IScene
    {
        public string Name => "Weather";

        public static string City { get; set; } = "London";
        public static double Latitude { get; set; } = 51.5074;
        public static double Longitude { get; set; } = -0.1278;

        private float _temperature = 0;
        private int _weatherCode = 0;
        private bool _isLoaded = false;
        private DateTime _lastFetch = DateTime.MinValue;

        private readonly HttpClient _httpClient = new HttpClient();

        public WeatherScene()
        {
            // Set User-Agent as required by Open-Meteo
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "iDotMatrix-WPF-Client");
        }

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // Background
            Color bgColor = Color.FromRgb(10, 12, 18);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = bgColor;
                }
            }

            // Fetch in background once every 15 minutes
            if (!_isLoaded || (DateTime.Now - _lastFetch).TotalMinutes > 15)
            {
                _lastFetch = DateTime.Now;
                Task.Run(FetchWeatherAsync);
            }

            if (!_isLoaded)
            {
                // Draw loading text
                PixelFont.DrawText(canvas, "LOADING", 2, 10, Color.FromRgb(255, 255, 0));
                PixelFont.DrawText(canvas, "WEATHER", 2, 16, Color.FromRgb(255, 255, 0));
                return canvas;
            }

            // A. Draw Weather Icon in top area (y=1 to y=18)
            DrawWeatherIcon(canvas, _weatherCode, frameCount);

            // B. Draw Temp text: e.g. "24C" (centered at y=20)
            string tempStr = $"{Math.Round(_temperature)}C";
            int tempWidth = PixelFont.MeasureTextWidth(tempStr) - 1;
            int tempX = (32 - tempWidth) / 2;
            PixelFont.DrawText(canvas, tempStr, tempX, 20, Color.FromRgb(255, 255, 255));

            // C. Draw City Name (scrolling at bottom y=26)
            string cityLabel = City.ToUpper();
            int cityWidth = PixelFont.MeasureTextWidth(cityLabel);
            int cityX = 0;
            if (cityWidth > 32)
            {
                // Scroll
                int scrollRange = cityWidth + 8;
                cityX = 32 - (frameCount % scrollRange);
            }
            else
            {
                // Center
                cityX = (32 - cityWidth) / 2;
            }
            PixelFont.DrawText(canvas, cityLabel, cityX, 26, Color.FromRgb(30, 215, 96), wrap: false);

            return canvas;
        }

        private void DrawWeatherIcon(Color[,] canvas, int code, int frameCount)
        {
            // Parse Weather code classes
            if (code == 0) // Clear sky (Sunny)
            {
                DrawSun(canvas, frameCount);
            }
            else if (code >= 1 && code <= 3) // Partly cloudy / Overcast
            {
                DrawCloud(canvas, frameCount, false);
            }
            else if ((code >= 51 && code <= 67) || (code >= 80 && code <= 82)) // Drizzle / Rain
            {
                DrawCloud(canvas, frameCount, false);
                DrawRain(canvas, frameCount);
            }
            else if (code >= 71 && code <= 77 || code == 85 || code == 86) // Snow
            {
                DrawCloud(canvas, frameCount, false);
                DrawSnow(canvas, frameCount);
            }
            else if (code >= 95) // Thunderstorm
            {
                DrawCloud(canvas, frameCount, true);
            }
            else // Default: Cloud
            {
                DrawCloud(canvas, frameCount, false);
            }
        }

        private void DrawSun(Color[,] canvas, int frameCount)
        {
            Color sunColor = Color.FromRgb(255, 200, 0);
            Color rayColor = Color.FromRgb(255, 120, 0);

            // Sun Center (glowing circle at 15, 9)
            double glow = Math.Sin(frameCount * 0.1) * 0.3 + 0.7;
            byte r = (byte)(sunColor.R * glow);
            byte g = (byte)(sunColor.G * glow);

            // 4x4 Core
            for (int y = 7; y <= 10; y++)
            {
                for (int x = 14; x <= 17; x++)
                {
                    canvas[y, x] = Color.FromRgb(r, g, 0);
                }
            }

            // Rays (pulsing)
            int rayLen = (frameCount / 5) % 2;
            if (rayLen == 0)
            {
                canvas[5, 15] = rayColor; canvas[5, 16] = rayColor; // Top
                canvas[12, 15] = rayColor; canvas[12, 16] = rayColor; // Bottom
                canvas[8, 11] = rayColor; canvas[9, 11] = rayColor; // Left
                canvas[8, 20] = rayColor; canvas[9, 20] = rayColor; // Right
            }
            else
            {
                canvas[6, 12] = rayColor; // Top-Left
                canvas[6, 19] = rayColor; // Top-Right
                canvas[11, 12] = rayColor; // Bottom-Left
                canvas[11, 19] = rayColor; // Bottom-Right
            }
        }

        private void DrawCloud(Color[,] canvas, int frameCount, bool lightning)
        {
            Color cloudColor = Color.FromRgb(150, 155, 165);
            Color darkCloud = Color.FromRgb(80, 85, 95);

            Color currentCloud = lightning ? darkCloud : cloudColor;

            // Draw a basic cute cloud shape at center-left
            // Base line
            for (int x = 8; x <= 23; x++) canvas[11, x] = currentCloud;
            for (int x = 7; x <= 24; x++) canvas[12, x] = currentCloud;
            for (int x = 8; x <= 23; x++) canvas[13, x] = currentCloud;
            
            // Puffs
            for (int y = 9; y <= 10; y++)
            {
                for (int x = 10; x <= 15; x++) canvas[y, x] = currentCloud;
                for (int x = 16; x <= 21; x++) canvas[y, x] = currentCloud;
            }
            for (int x = 12; x <= 18; x++) canvas[8, x] = currentCloud;

            // If lightning is active, draw a yellow flash
            if (lightning && (frameCount / 8) % 2 == 0)
            {
                Color boltColor = Color.FromRgb(255, 255, 0);
                canvas[14, 15] = boltColor;
                canvas[15, 14] = boltColor;
                canvas[16, 14] = boltColor;
                canvas[17, 13] = boltColor;
            }
        }

        private void DrawRain(Color[,] canvas, int frameCount)
        {
            Color dropColor = Color.FromRgb(0, 128, 255);
            
            // Alternate raindrops over 2 states
            int state = (frameCount / 4) % 2;
            if (state == 0)
            {
                canvas[15, 10] = dropColor;
                canvas[17, 12] = dropColor;
                canvas[15, 17] = dropColor;
                canvas[17, 19] = dropColor;
            }
            else
            {
                canvas[16, 11] = dropColor;
                canvas[18, 13] = dropColor;
                canvas[16, 18] = dropColor;
                canvas[18, 20] = dropColor;
            }
        }

        private void DrawSnow(Color[,] canvas, int frameCount)
        {
            Color snowColor = Color.FromRgb(240, 248, 255);

            int state = (frameCount / 6) % 2;
            if (state == 0)
            {
                canvas[15, 11] = snowColor;
                canvas[17, 15] = snowColor;
                canvas[15, 20] = snowColor;
            }
            else
            {
                canvas[16, 11] = snowColor;
                canvas[16, 16] = snowColor;
                canvas[17, 19] = snowColor;
            }
        }

        public static async Task<bool> ResolveCityCoordinatesAsync(string cityName)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "iDotMatrix-WPF-Client");
                    string url = $"https://geocoding-api.open-meteo.com/v1/search?name={Uri.EscapeDataString(cityName)}&count=1&language=en&format=json";
                    string json = await client.GetStringAsync(url);
                    
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                        {
                            var first = results[0];
                            City = first.GetProperty("name").GetString() ?? cityName;
                            Latitude = first.GetProperty("latitude").GetDouble();
                            Longitude = first.GetProperty("longitude").GetDouble();
                            return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private async Task FetchWeatherAsync()
        {
            try
            {
                string url = $"https://api.open-meteo.com/v1/forecast?latitude={Latitude}&longitude={Longitude}&current_weather=true";
                string json = await _httpClient.GetStringAsync(url);

                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("current_weather", out var currentWeather))
                    {
                        _temperature = (float)currentWeather.GetProperty("temperature").GetDouble();
                        _weatherCode = currentWeather.GetProperty("weathercode").GetInt32();
                        _isLoaded = true;
                    }
                }
            }
            catch (Exception)
            {
                // Keep old values on connection failure
            }
        }

        public void Stop() { }
    }
}
