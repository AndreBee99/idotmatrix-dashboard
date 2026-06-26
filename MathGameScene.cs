using System;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class MathGameScene : IScene
    {
        public string Name => "Math Challenge";

        private bool _isLoaded = false;
        private string _equationTease = "";
        private string _equationReveal = "";
        private int _answer = 0;

        private DateTime _phaseStartTime = DateTime.MinValue;
        private readonly int _teaseDurationMs = 5000;
        private int _revealStartFrame = -1;
        private bool _isDone = false;

        private static readonly Random _rand = new Random();

        public bool CustomCompletion => true;
        public bool IsDone => _isDone;

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // 1. Background: Dark Chalkboard Green (15, 45, 25)
            Color chalkboardGreen = Color.FromRgb(15, 45, 25);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = chalkboardGreen;
                }
            }

            // Chalkboard frame: Brown wood border (y=0..25)
            Color borderWood = Color.FromRgb(139, 90, 43);
            for (int x = 0; x < 32; x++)
            {
                canvas[0, x] = borderWood;
                canvas[25, x] = borderWood;
            }
            for (int y = 0; y <= 25; y++)
            {
                canvas[y, 0] = borderWood;
                canvas[y, 31] = borderWood;
            }

            // 2. Manage loading states
            if (!_isLoaded)
            {
                _isLoaded = true;
                _phaseStartTime = DateTime.Now;
                GenerateProblem();
            }

            // 3. Determine active phase
            double elapsedMs = (DateTime.Now - _phaseStartTime).TotalMilliseconds;
            bool isRevealPhase = elapsedMs > _teaseDurationMs;

            // 4. Render equation centered on the chalkboard (y=10)
            string eqText = isRevealPhase ? _equationReveal : _equationTease;
            int textW = PixelFont.MeasureTextWidth(eqText);
            int textX = (32 - textW) / 2;
            int textY = 10;

            Color chalkColor = Color.FromRgb(240, 240, 240); // Chalk White
            PixelFont.DrawText(canvas, eqText, textX, textY, chalkColor);

            // 5. Draw Scrolling Text Banner at bottom (y=26 to y=31)
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
            int bannerTextWidth = 0;

            if (isRevealPhase)
            {
                scrollText = $"ANSWER IS {_answer}!  ~  ";
                textColor = Color.FromRgb(30, 215, 96); // Neon Green
                bannerTextWidth = PixelFont.MeasureTextWidth(scrollText);

                if (_revealStartFrame == -1)
                {
                    _revealStartFrame = frameCount;
                }
                else if (frameCount - _revealStartFrame >= 32 + bannerTextWidth)
                {
                    _isDone = true;
                }

                // Flashing green border inside wood frame on reveal
                if ((frameCount / 3) % 2 == 0)
                {
                    Color flashColor = Color.FromRgb(50, 230, 100);
                    for (int x = 1; x < 31; x++) { canvas[1, x] = flashColor; canvas[24, x] = flashColor; }
                    for (int y = 1; y <= 24; y++) { canvas[y, 1] = flashColor; canvas[y, 30] = flashColor; }
                }
            }
            else
            {
                scrollText = "SOLVE THE EQUATION!  ~  ";
                textColor = Color.FromRgb(255, 255, 0); // Yellow
                bannerTextWidth = PixelFont.MeasureTextWidth(scrollText);
            }

            int scrollRange = bannerTextWidth + 8;
            int bannerX = 32 - (frameCount % scrollRange);
            PixelFont.DrawText(canvas, scrollText, bannerX, 26, textColor);

            return canvas;
        }

        public void Stop()
        {
            _isLoaded = false;
            _revealStartFrame = -1;
            _isDone = false;
        }

        private void GenerateProblem()
        {
            int op = _rand.Next(3);
            if (op == 0)
            {
                // Addition: sum up to ~90
                int a = _rand.Next(5, 50);
                int b = _rand.Next(5, 50);
                _answer = a + b;
                _equationTease = $"{a}+{b}=?";
                _equationReveal = $"{a}+{b}={_answer}";
            }
            else if (op == 1)
            {
                // Subtraction: positive result under 100
                int a = _rand.Next(10, 99);
                int b = _rand.Next(5, a);
                _answer = a - b;
                _equationTease = $"{a}-{b}=?";
                _equationReveal = $"{a}-{b}={_answer}";
            }
            else
            {
                // Multiplication: simple times tables 2 to 9
                int a = _rand.Next(2, 10);
                int b = _rand.Next(2, 10);
                _answer = a * b;
                _equationTease = $"{a}*{b}=?";
                _equationReveal = $"{a}*{b}={_answer}";
            }
        }
    }
}
