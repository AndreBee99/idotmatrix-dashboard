using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class TunerCar
    {
        public string Name { get; set; } = "";
        public Color BodyColor { get; set; }
        public bool HasGoldWheels { get; set; }
        public bool HasSpoiler { get; set; }
        public int Type { get; set; } // 0: GT-R, 1: Supra, 2: RX-7, 3: WRX STI
    }

    public class CarHighwayScene : IScene
    {
        public string Name => "Highway Outrun";

        private int _currentCarIndex = 0;
        private readonly List<TunerCar> _cars = new List<TunerCar>();
        private readonly Random _rand = new Random();

        // Parallax mountain heights
        private readonly int[] _mountainHeights = {
            2, 3, 4, 3, 2, 1, 2, 3, 5, 4, 3, 2, 3, 4, 2, 1,
            2, 3, 4, 3, 2, 1, 3, 5, 6, 5, 4, 3, 2, 1, 2, 3
        };

        public CarHighwayScene()
        {
            // 1. Nissan Skyline GT-R R34 (Bayside Blue)
            _cars.Add(new TunerCar {
                Name = "SKYLINE R34",
                BodyColor = Color.FromRgb(0, 70, 220),
                HasGoldWheels = false,
                HasSpoiler = true,
                Type = 0
            });

            // 2. Toyota Supra MK4 (Candy Orange)
            _cars.Add(new TunerCar {
                Name = "SUPRA MK4",
                BodyColor = Color.FromRgb(255, 90, 0),
                HasGoldWheels = false,
                HasSpoiler = true,
                Type = 1
            });

            // 3. Mazda RX-7 FD (Vintage Red)
            _cars.Add(new TunerCar {
                Name = "RX-7 FD",
                BodyColor = Color.FromRgb(220, 0, 30),
                HasGoldWheels = false,
                HasSpoiler = false,
                Type = 2
            });

            // 4. Subaru WRX STI (Rally Blue)
            _cars.Add(new TunerCar {
                Name = "WRX STI",
                BodyColor = Color.FromRgb(0, 95, 195),
                HasGoldWheels = true,
                HasSpoiler = true,
                Type = 3
            });
        }

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // Get current active car
            TunerCar activeCar = _cars[_currentCarIndex];

            // 1. Draw Sky Gradient (y=0 to y=14)
            Color skyTop = Color.FromRgb(15, 0, 30);      // Deep Indigo
            Color skyBottom = Color.FromRgb(65, 0, 55);   // Synthwave Violet/Pink
            for (int y = 0; y <= 14; y++)
            {
                double ratio = y / 14.0;
                Color skyColor = Color.FromRgb(
                    (byte)(skyTop.R * (1.0 - ratio) + skyBottom.R * ratio),
                    (byte)(skyTop.G * (1.0 - ratio) + skyBottom.G * ratio),
                    (byte)(skyTop.B * (1.0 - ratio) + skyBottom.B * ratio)
                );
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = skyColor;
                }
            }

            // 2. Draw Twinkling Star Pixels
            DrawStars(canvas, frameCount);

            // 3. Draw Synthwave Retro Sun (Centered at x=16, y=14)
            DrawRetroSun(canvas);

            // 4. Draw Parallax Mountain Silhouette (y=11 to y=14)
            // Scroll offset moves 1 pixel every 4 frames
            int mountainScroll = (frameCount / 4) % 32;
            Color mountainDark = Color.FromRgb(30, 8, 40); // Dark purple
            Color mountainCrest = Color.FromRgb(255, 0, 127); // Neon pink outline

            for (int x = 0; x < 32; x++)
            {
                int hIdx = (x + mountainScroll) % 32;
                int h = _mountainHeights[hIdx];
                int baseLine = 14;
                for (int y = baseLine - h; y <= baseLine; y++)
                {
                    if (y >= 0 && y < 32)
                    {
                        canvas[y, x] = (y == baseLine - h) ? mountainCrest : mountainDark;
                    }
                }
            }

            // 5. Draw Highway Road Surface (y=15 to y=31)
            Color roadColor = Color.FromRgb(25, 25, 30);
            for (int y = 15; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = roadColor;
                }
            }

            // 6. Draw Scrolling Guardrail (y=15)
            Color guardrailBg = Color.FromRgb(40, 40, 45);
            Color guardrailDot = Color.FromRgb(0, 240, 255); // Neon Cyan dots
            for (int x = 0; x < 32; x++)
            {
                canvas[15, x] = guardrailBg;
            }
            int railScroll = (frameCount) % 6;
            for (int x = railScroll; x < 32; x += 6)
            {
                canvas[15, x] = guardrailDot;
            }

            // 7. Draw Scrolling Road Lane Markings (y=23)
            Color laneColor = Color.FromRgb(255, 230, 0); // Golden yellow dashes
            int laneScroll = (frameCount * 2) % 8; // Scrolls fast left
            for (int x = 0; x < 32; x++)
            {
                int localX = (x + laneScroll) % 8;
                if (localX < 4) // Dash length 4, space 4
                {
                    canvas[23, x] = laneColor;
                }
            }

            // 8. Draw Volumetric Headlight Glow (shining right)
            // Car starts at xPos=4, headlight is at relative x=22
            DrawHeadlightBeam(canvas, 4 + 22, 17 + 5);

            // 9. Draw Tuner Car (at x=4, y=17)
            DrawCar(canvas, activeCar, 4, 17, frameCount);

            // 10. Draw HUD overlay showing car name for first 50 frames
            if (frameCount < 50)
            {
                // Draw a sleek HUD backing bar
                for (int y = 1; y < 8; y++)
                {
                    for (int x = 1; x < 31; x++)
                    {
                        Color c = canvas[y, x];
                        canvas[y, x] = Color.FromRgb((byte)(c.R * 0.4), (byte)(c.G * 0.4), (byte)(c.B * 0.6));
                    }
                }
                PixelFont.DrawText(canvas, activeCar.Name, 3, 2, Color.FromRgb(0, 240, 255));
            }

            return canvas;
        }

        private void DrawStars(Color[,] canvas, int frameCount)
        {
            // Twinkling stars at fixed positions
            int[][] stars = {
                new int[] { 3, 2 },
                new int[] { 10, 4 },
                new int[] { 28, 1 },
                new int[] { 22, 5 }
            };

            for (int i = 0; i < stars.Length; i++)
            {
                int x = stars[i][0];
                int y = stars[i][1];
                
                // Twinkle cycle
                int offset = i * 13;
                int brightness = (int)(155 + 100 * Math.Sin((frameCount + offset) * 0.3));
                if (brightness < 50) brightness = 50;

                canvas[y, x] = Color.FromRgb((byte)brightness, (byte)brightness, (byte)(brightness + 20));
            }
        }

        private void DrawRetroSun(Color[,] canvas)
        {
            int cx = 16;
            int cy = 14;
            int radius = 7;

            Color sunTop = Color.FromRgb(255, 230, 0); // Yellow
            Color sunBottom = Color.FromRgb(255, 0, 100); // Pink/Magenta

            for (int dy = -radius; dy <= 0; dy++)
            {
                int y = cy + dy;
                if (y < 0 || y >= 32) continue;

                // Synthwave split lines (skip drawing at horizontal gaps)
                if (y == 8 || y == 10 || y == 12 || y == 13) continue;

                double ratio = (dy + radius) / (double)radius;
                Color sunColor = Color.FromRgb(
                    (byte)(sunTop.R * (1.0 - ratio) + sunBottom.R * ratio),
                    (byte)(sunTop.G * (1.0 - ratio) + sunBottom.G * ratio),
                    (byte)(sunTop.B * (1.0 - ratio) + sunBottom.B * ratio)
                );

                int width = (int)Math.Sqrt(radius * radius - dy * dy);
                for (int dx = -width; dx <= width; dx++)
                {
                    int x = cx + dx;
                    if (x >= 0 && x < 32)
                    {
                        canvas[y, x] = sunColor;
                    }
                }
            }
        }

        private void DrawHeadlightBeam(Color[,] canvas, int startX, int startY)
        {
            Color glow = Color.FromRgb(255, 255, 140);
            for (int dx = 0; dx < 10; dx++)
            {
                int x = startX + dx;
                if (x >= 32) break;

                // Beam spreads vertically as it moves away
                int spread = dx / 2;
                for (int dy = -spread; dy <= spread; dy++)
                {
                    int y = startY + dy;
                    if (y >= 0 && y < 32)
                    {
                        Color current = canvas[y, x];
                        double intensity = 0.35 * (1.0 - (double)dx / 10.0); // Fades out with distance

                        canvas[y, x] = Color.FromRgb(
                            (byte)(current.R * (1.0 - intensity) + glow.R * intensity),
                            (byte)(current.G * (1.0 - intensity) + glow.G * intensity),
                            (byte)(current.B * (1.0 - intensity) + glow.B * intensity)
                        );
                    }
                }
            }
        }

        private void DrawCar(Color[,] canvas, TunerCar car, int xPos, int yPos, int frameCount)
        {
            Color body = car.BodyColor;
            Color black = Color.FromRgb(10, 10, 15);
            Color glass = Color.FromRgb(20, 25, 45); // Dark blue windows
            Color headlight = Color.FromRgb(255, 255, 180);
            Color taillight = Color.FromRgb(255, 0, 0);
            Color wheelColor = car.HasGoldWheels ? Color.FromRgb(215, 165, 30) : Color.FromRgb(95, 95, 100);
            Color exhaustFlame = Color.FromRgb(255, 120, 0);

            // 1. Spoiler
            if (car.HasSpoiler)
            {
                if (car.Type == 0) // GT-R spoiler: blocky / vertical fins
                {
                    DrawPixel(canvas, xPos + 1, yPos + 1, black);
                    DrawPixel(canvas, xPos + 1, yPos + 2, body);
                    DrawPixel(canvas, xPos + 2, yPos + 1, body);
                }
                else if (car.Type == 1) // Supra spoiler: curved loops
                {
                    DrawPixel(canvas, xPos + 1, yPos + 1, body);
                    DrawPixel(canvas, xPos + 2, yPos + 1, body);
                    DrawPixel(canvas, xPos + 1, yPos + 2, body);
                    DrawPixel(canvas, xPos + 3, yPos + 2, body);
                }
                else if (car.Type == 3) // WRX STI wing: tall, prominent
                {
                    DrawPixel(canvas, xPos + 1, yPos + 1, body);
                    DrawPixel(canvas, xPos + 2, yPos + 1, body);
                    DrawPixel(canvas, xPos + 1, yPos + 2, black);
                    DrawPixel(canvas, xPos + 3, yPos + 2, black);
                }
            }

            // 2. Cabin / Roof (yPos + 2)
            int roofStart = (car.Type == 2) ? 7 : 6; // FD is sleek and shifted back
            int roofEnd = 13;
            for (int x = roofStart; x <= roofEnd; x++)
            {
                DrawPixel(canvas, xPos + x, yPos + 2, body);
            }

            // 3. Side Windows / Pillars (yPos + 3)
            for (int x = roofStart - 1; x <= roofEnd + 1; x++)
            {
                if (x >= roofStart + 1 && x <= roofEnd - 1)
                {
                    DrawPixel(canvas, xPos + x, yPos + 3, glass);
                }
                else
                {
                    DrawPixel(canvas, xPos + x, yPos + 3, body);
                }
            }
            // Add windshield glass slant
            DrawPixel(canvas, xPos + roofEnd + 1, yPos + 3, glass);
            DrawPixel(canvas, xPos + roofStart - 1, yPos + 3, glass);

            // 4. Beltline (yPos + 4)
            // Bumper to hood scoop
            for (int x = 1; x <= 20; x++)
            {
                if (car.Type == 3 && x == 17) // Subaru hood scoop
                {
                    DrawPixel(canvas, xPos + x, yPos + 4, black);
                }
                else
                {
                    DrawPixel(canvas, xPos + x, yPos + 4, body);
                }
            }

            // 5. Main Body Belt (yPos + 5)
            for (int x = 0; x <= 21; x++)
            {
                DrawPixel(canvas, xPos + x, yPos + 5, body);
            }

            // Headlights
            if (car.Type == 2) // RX-7 popups are lower profile
            {
                DrawPixel(canvas, xPos + 20, yPos + 4, headlight);
            }
            else
            {
                DrawPixel(canvas, xPos + 21, yPos + 5, headlight);
            }

            // Taillights
            if (car.Type == 0) // GT-R round rings
            {
                DrawPixel(canvas, xPos, yPos + 4, taillight);
                DrawPixel(canvas, xPos, yPos + 5, taillight);
            }
            else
            {
                DrawPixel(canvas, xPos, yPos + 5, taillight);
            }

            // 6. Bottom body / Side skirt (yPos + 6)
            // Leaving gaps for wheels at col 4-6 and 14-16
            for (int x = 0; x <= 21; x++)
            {
                bool isWheelArch = (x >= 4 && x <= 6) || (x >= 14 && x <= 16);
                if (!isWheelArch)
                {
                    DrawPixel(canvas, xPos + x, yPos + 6, body);
                }
            }

            // 7. Draw Wheels
            DrawWheel(canvas, xPos + 5, yPos + 6, wheelColor, frameCount);
            DrawWheel(canvas, xPos + 15, yPos + 6, wheelColor, frameCount);

            // 8. Animated Exhaust Flame
            int exhaustOffset = frameCount % 6;
            if (exhaustOffset < 2)
            {
                DrawPixel(canvas, xPos - 1, yPos + 6, exhaustFlame);
            }
            else if (exhaustOffset == 2)
            {
                DrawPixel(canvas, xPos - 1, yPos + 6, Color.FromRgb(255, 200, 0));
                DrawPixel(canvas, xPos - 2, yPos + 6, exhaustFlame);
            }
        }

        private void DrawWheel(Color[,] canvas, int cx, int cy, Color metalColor, int frameCount)
        {
            Color darkTire = Color.FromRgb(15, 15, 20);

            // Tire outline
            DrawPixel(canvas, cx, cy - 1, darkTire);
            DrawPixel(canvas, cx + 1, cy - 1, darkTire);
            DrawPixel(canvas, cx - 1, cy, darkTire);
            DrawPixel(canvas, cx + 2, cy, darkTire);
            DrawPixel(canvas, cx - 1, cy + 1, darkTire);
            DrawPixel(canvas, cx + 2, cy + 1, darkTire);
            DrawPixel(canvas, cx, cy + 2, darkTire);
            DrawPixel(canvas, cx + 1, cy + 2, darkTire);

            // Spin animation
            int spin = (frameCount / 2) % 4;
            if (spin == 0)
            {
                DrawPixel(canvas, cx, cy, metalColor);
                DrawPixel(canvas, cx + 1, cy + 1, metalColor);
            }
            else if (spin == 1)
            {
                DrawPixel(canvas, cx + 1, cy, metalColor);
                DrawPixel(canvas, cx, cy + 1, metalColor);
            }
            else if (spin == 2)
            {
                DrawPixel(canvas, cx, cy + 1, metalColor);
                DrawPixel(canvas, cx + 1, cy, metalColor);
            }
            else
            {
                DrawPixel(canvas, cx + 1, cy + 1, metalColor);
                DrawPixel(canvas, cx, cy, metalColor);
            }
        }

        private void DrawPixel(Color[,] canvas, int x, int y, Color c)
        {
            if (x >= 0 && x < 32 && y >= 0 && y < 32)
            {
                canvas[y, x] = c;
            }
        }

        public void Stop()
        {
            // Move to next car design on scene reset
            _currentCarIndex = (_currentCarIndex + 1) % _cars.Count;
        }
    }
}
