using System;
using System.IO;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace idotmatrix_gui
{
    public class CountryGuessScene : IScene
    {
        public string Name => "Guess the Country";

        private bool _isLoaded = false;
        private string _countryName = "";
        private string _countryCode = "";
        private Color[,] _flagPixels = new Color[16, 24]; // Standard resized flag shape: 24x16

        private DateTime _phaseStartTime = DateTime.MinValue;
        private readonly int _teaseDurationMs = 5000;
        private int _revealStartFrame = -1;
        private bool _isDone = false;

        private readonly HttpClient _httpClient = new HttpClient();

        public bool CustomCompletion => true;
        public bool IsDone => _isDone;

        private static readonly (string Code, string Name)[] Countries = new[]
        {
            // Europe
            ("us", "United States"),
            ("gb", "United Kingdom"),
            ("ca", "Canada"),
            ("mx", "Mexico"),
            ("br", "Brazil"),
            ("fr", "France"),
            ("de", "Germany"),
            ("it", "Italy"),
            ("es", "Spain"),
            ("se", "Sweden"),
            ("ch", "Switzerland"),
            ("nl", "Netherlands"),
            ("be", "Belgium"),
            ("gr", "Greece"),
            ("pl", "Poland"),
            ("ua", "Ukraine"),
            ("ie", "Ireland"),
            ("no", "Norway"),
            ("fi", "Finland"),
            ("dk", "Denmark"),
            ("at", "Austria"),
            ("pt", "Portugal"),
            ("cz", "Czechia"),
            ("hu", "Hungary"),
            ("ro", "Romania"),
            ("bg", "Bulgaria"),
            ("hr", "Croatia"),
            ("sk", "Slovakia"),
            ("si", "Slovenia"),
            ("ee", "Estonia"),
            ("lv", "Latvia"),
            ("lt", "Lithuania"),
            ("is", "Iceland"),
            ("lu", "Luxembourg"),
            ("mc", "Monaco"),
            ("mt", "Malta"),
            ("cy", "Cyprus"),
            ("al", "Albania"),
            ("ba", "Bosnia"),
            ("rs", "Serbia"),
            ("me", "Montenegro"),
            ("mk", "North Macedonia"),
            ("md", "Moldova"),
            ("by", "Belarus"),
            ("ru", "Russia"),

            // Asia
            ("jp", "Japan"),
            ("cn", "China"),
            ("kr", "South Korea"),
            ("in", "India"),
            ("sg", "Singapore"),
            ("th", "Thailand"),
            ("ph", "Philippines"),
            ("id", "Indonesia"),
            ("my", "Malaysia"),
            ("vn", "Vietnam"),
            ("pk", "Pakistan"),
            ("bd", "Bangladesh"),
            ("lk", "Sri Lanka"),
            ("np", "Nepal"),
            ("kh", "Cambodia"),
            ("la", "Laos"),
            ("mm", "Myanmar"),
            ("mn", "Mongolia"),
            ("tw", "Taiwan"),
            ("kz", "Kazakhstan"),
            ("uz", "Uzbekistan"),
            ("kg", "Kyrgyzstan"),
            ("tj", "Tajikistan"),
            ("tm", "Turkmenistan"),

            // Middle East
            ("sa", "Saudi Arabia"),
            ("ae", "United Arab Emirates"),
            ("tr", "Turkey"),
            ("ir", "Iran"),
            ("iq", "Iraq"),
            ("il", "Israel"),
            ("jo", "Jordan"),
            ("lb", "Lebanon"),
            ("sy", "Syria"),
            ("ye", "Yemen"),
            ("om", "Oman"),
            ("qa", "Qatar"),
            ("kw", "Kuwait"),
            ("bh", "Bahrain"),

            // Americas
            ("ar", "Argentina"),
            ("co", "Colombia"),
            ("cl", "Chile"),
            ("pe", "Peru"),
            ("ve", "Venezuela"),
            ("ec", "Ecuador"),
            ("bo", "Bolivia"),
            ("py", "Paraguay"),
            ("uy", "Uruguay"),
            ("cr", "Costa Rica"),
            ("pa", "Panama"),
            ("gt", "Guatemala"),
            ("hn", "Honduras"),
            ("sv", "El Salvador"),
            ("ni", "Nicaragua"),
            ("cu", "Cuba"),
            ("jm", "Jamaica"),
            ("do", "Dominican Republic"),

            // Africa
            ("za", "South Africa"),
            ("eg", "Egypt"),
            ("ng", "Nigeria"),
            ("ke", "Kenya"),
            ("ma", "Morocco"),
            ("dz", "Algeria"),
            ("tn", "Tunisia"),
            ("gh", "Ghana"),
            ("et", "Ethiopia"),
            ("tz", "Tanzania"),
            ("ug", "Uganda"),
            ("sn", "Senegal"),
            ("ci", "Ivory Coast"),
            ("cm", "Cameroon"),
            ("ao", "Angola"),
            ("mz", "Mozambique"),
            ("mg", "Madagascar"),
            ("zw", "Zimbabwe"),

            // Oceania
            ("au", "Australia"),
            ("nz", "New Zealand"),
            ("fj", "Fiji"),
            ("pg", "Papua New Guinea")
        };

        public CountryGuessScene()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BeeMatrix-Client");
        }

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // 1. Background: Dark Indigo/Violet (10, 10, 20)
            Color bg = Color.FromRgb(10, 10, 20);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = bg;
                }
            }

            // 2. Manage loading states
            if (!_isLoaded)
            {
                _isLoaded = true;
                _phaseStartTime = DateTime.Now;
                Task.Run(FetchRandomCountryAsync);
            }

            if (string.IsNullOrEmpty(_countryName))
            {
                PixelFont.DrawText(canvas, "LOADING", 2, 10, Color.FromRgb(0, 240, 255));
                PixelFont.DrawText(canvas, "COUNTRY", 2, 16, Color.FromRgb(0, 240, 255));
                return canvas;
            }

            // 3. Determine active phase
            double elapsedMs = (DateTime.Now - _phaseStartTime).TotalMilliseconds;
            bool isRevealPhase = elapsedMs > _teaseDurationMs;

            // Draw flag centered in top 26 rows (y=0 to y=25)
            // Sized at 24x16. Center offsetX = 4, offsetY = 5
            int flagH = _flagPixels.GetLength(0);
            int flagW = _flagPixels.GetLength(1);
            int offsetX = (32 - flagW) / 2;
            int offsetY = (26 - flagH) / 2;

            // Draw a 1px border around the flag box
            Color borderColor = isRevealPhase ? Color.FromRgb(30, 215, 96) : Color.FromRgb(0, 240, 255);
            for (int x = offsetX - 1; x <= offsetX + flagW; x++)
            {
                canvas[offsetY - 1, x] = borderColor;
                canvas[offsetY + flagH, x] = borderColor;
            }
            for (int y = offsetY - 1; y <= offsetY + flagH; y++)
            {
                canvas[y, offsetX - 1] = borderColor;
                canvas[y, offsetX + flagW] = borderColor;
            }

            // Paste flag pixels
            for (int fy = 0; fy < flagH; fy++)
            {
                for (int fx = 0; fx < flagW; fx++)
                {
                    canvas[offsetY + fy, offsetX + fx] = _flagPixels[fy, fx];
                }
            }

            // 4. Draw Scrolling Text Banner at bottom (y=26 to y=31)
            for (int x = 0; x < 32; x++)
            {
                for (int y = 26; y < 32; y++)
                {
                    Color px = canvas[y, x];
                    canvas[y, x] = Color.FromRgb((byte)(px.R * 0.3), (byte)(px.G * 0.3), (byte)(px.B * 0.3));
                }
            }

            string scrollText;
            Color textColor;
            int textWidth = 0;

            if (isRevealPhase)
            {
                scrollText = $"IT'S {_countryName.ToUpper()}!  ~  ";
                textColor = Color.FromRgb(30, 215, 96); // Neon Green
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
                    Color flashColor = Color.FromRgb(30, 215, 96);
                    for (int x = 0; x < 32; x++) { canvas[0, x] = flashColor; canvas[25, x] = flashColor; }
                    for (int y = 0; y <= 25; y++) { canvas[y, 0] = flashColor; canvas[y, 31] = flashColor; }
                }
            }
            else
            {
                scrollText = "GUESS THE COUNTRY!  ~  ";
                textColor = Color.FromRgb(255, 255, 255); // White
                textWidth = PixelFont.MeasureTextWidth(scrollText);
            }

            int scrollRange = textWidth + 8;
            int textX = 32 - (frameCount % scrollRange);
            PixelFont.DrawText(canvas, scrollText, textX, 26, textColor);

            return canvas;
        }

        public void Stop()
        {
            _countryName = "";
            _isLoaded = false;
            _revealStartFrame = -1;
            _isDone = false;
        }

        private async Task FetchRandomCountryAsync()
        {
            try
            {
                Random rand = new Random();
                var country = Countries[rand.Next(Countries.Length)];

                string url = $"https://flagcdn.com/w80/{country.Code}.png";
                byte[] data = await _httpClient.GetByteArrayAsync(url);
                using (MemoryStream ms = new MemoryStream(data))
                {
                    var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    var frame = decoder.Frames[0];
                    var converted = new FormatConvertedBitmap(frame, PixelFormats.Bgra32, null, 0);

                    int origW = converted.PixelWidth;
                    int origH = converted.PixelHeight;
                    int stride = origW * 4;
                    byte[] rawPixels = new byte[origH * stride];
                    converted.CopyPixels(rawPixels, stride, 0);

                    // We want to scale down to 24x16.
                    int targetW = 24;
                    int targetH = 16;
                    Color[,] flagPixels = new Color[targetH, targetW];

                    for (int y = 0; y < targetH; y++)
                    {
                        int origY = (int)((y / (double)targetH) * origH);
                        origY = Math.Max(0, Math.Min(origH - 1, origY));

                        for (int x = 0; x < targetW; x++)
                        {
                            int origX = (int)((x / (double)targetW) * origW);
                            origX = Math.Max(0, Math.Min(origW - 1, origX));

                            int idx = origY * stride + origX * 4;
                            byte b = rawPixels[idx];
                            byte g = rawPixels[idx + 1];
                            byte r = rawPixels[idx + 2];
                            byte a = rawPixels[idx + 3];

                            flagPixels[y, x] = Color.FromArgb(a, r, g, b);
                        }
                    }

                    _flagPixels = flagPixels;
                    _countryName = country.Name;
                    _countryCode = country.Code;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching country flag: {ex.Message}");
                // Fallback flag (Red cross on White)
                Color[,] fallback = new Color[16, 24];
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 24; x++)
                    {
                        fallback[y, x] = (x == 12 || y == 8) ? Color.FromRgb(255, 0, 0) : Color.FromRgb(255, 255, 255);
                    }
                }
                _flagPixels = fallback;
                _countryName = "United Kingdom";
                _countryCode = "gb";
            }
        }
    }
}
