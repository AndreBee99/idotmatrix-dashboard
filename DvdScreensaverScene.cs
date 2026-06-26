using System;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class DvdScreensaverScene : IScene
    {
        public string Name => "DVD Screensaver";

        private double _x = 5;
        private double _y = 5;
        private double _vx = 0.4;
        private double _vy = 0.3;

        private readonly int _logoWidth = 11; // "DVD" width
        private readonly int _logoHeight = 5;
        
        private int _colorIndex = 0;
        private readonly Color[] _colors = new Color[]
        {
            Color.FromRgb(255, 0, 0),     // Red
            Color.FromRgb(0, 255, 0),     // Green
            Color.FromRgb(0, 0, 255),     // Blue
            Color.FromRgb(255, 255, 0),   // Yellow
            Color.FromRgb(255, 0, 255),   // Magenta
            Color.FromRgb(0, 255, 255),   // Cyan
            Color.FromRgb(255, 128, 0)    // Orange
        };

        private int _flashFrames = 0;

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // If corner hit flash is active, fill background with white briefly
            if (_flashFrames > 0)
            {
                _flashFrames--;
                Color flashColor = Color.FromRgb(255, 255, 255);
                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        canvas[y, x] = flashColor;
                    }
                }
                
                // Draw DVD text in black during flash
                PixelFont.DrawText(canvas, "DVD", (int)_x, (int)_y, Color.FromRgb(0, 0, 0));
                return canvas;
            }

            // Move
            _x += _vx;
            _y += _vy;

            bool hitX = false;
            bool hitY = false;

            // Bounce X
            if (_x <= 0)
            {
                _x = 0;
                _vx = -_vx;
                hitX = true;
            }
            else if (_x >= 32 - _logoWidth)
            {
                _x = 32 - _logoWidth;
                _vx = -_vx;
                hitX = true;
            }

            // Bounce Y
            if (_y <= 0)
            {
                _y = 0;
                _vy = -_vy;
                hitY = true;
            }
            else if (_y >= 32 - _logoHeight)
            {
                _y = 32 - _logoHeight;
                _vy = -_vy;
                hitY = true;
            }

            // Change color and check for corner hit on bounce
            if (hitX || hitY)
            {
                _colorIndex = (_colorIndex + 1) % _colors.Length;

                // Check for exact corner hit
                bool atCornerX = (_x == 0 || _x == 32 - _logoWidth);
                bool atCornerY = (_y == 0 || _y == 32 - _logoHeight);
                if (atCornerX && atCornerY)
                {
                    _flashFrames = 8; // Flash for 8 frames!
                }
            }

            // Draw DVD logo
            PixelFont.DrawText(canvas, "DVD", (int)_x, (int)_y, _colors[_colorIndex]);

            return canvas;
        }

        public void Stop() { }
    }
}
