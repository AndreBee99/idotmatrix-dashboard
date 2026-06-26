using System;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class ClockScene : IScene
    {
        public string Name => "Clock";

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];
            
            // Background: Dark gradient/solid
            Color bgColor = Color.FromRgb(15, 15, 15);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = bgColor;
                }
            }

            var now = DateTime.Now;
            
            // Format time: "HH:mm" or "HH mm" depending on blink
            bool showColon = (frameCount / 10) % 2 == 0;
            string timeStr = now.ToString(showColon ? "HH:mm" : "HH mm");
            
            // Draw Time (centered)
            int timeWidth = PixelFont.MeasureTextWidth(timeStr) - 1;
            int timeX = (32 - timeWidth) / 2;
            Color timeColor = Color.FromRgb(0, 255, 255); // Neon Cyan
            PixelFont.DrawText(canvas, timeStr, timeX, 9, timeColor);

            // Draw Date: "MMM dd" (centered)
            string dateStr = now.ToString("MMM dd").ToUpper();
            int dateWidth = PixelFont.MeasureTextWidth(dateStr) - 1;
            int dateX = (32 - dateWidth) / 2;
            Color dateColor = Color.FromRgb(255, 0, 128); // Neon Pink
            PixelFont.DrawText(canvas, dateStr, dateX, 17, dateColor);

            // Add a little bottom pixel art decoration (e.g. 1px line showing a status glow)
            Color glowColor = Color.FromRgb(30, 215, 96);
            for (int x = 4; x < 28; x++)
            {
                canvas[28, x] = Color.FromRgb(40, 40, 40);
            }
            // Pulsing dot at center
            double pulse = (Math.Sin(frameCount * 0.15) * 0.5 + 0.5);
            byte r = (byte)(pulse * 30 + 10);
            byte g = (byte)(pulse * 215 + 40);
            byte b = (byte)(pulse * 96 + 20);
            canvas[28, 15] = Color.FromRgb(r, g, b);
            canvas[28, 16] = Color.FromRgb(r, g, b);

            return canvas;
        }

        public void Stop() { }
    }
}
