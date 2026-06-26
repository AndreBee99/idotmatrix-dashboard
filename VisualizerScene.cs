using System;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class VisualizerScene : IScene
    {
        public string Name => "Audio Visualizer";

        public static string Theme { get; set; } = "synthwave";

        private readonly double[] _fftIndices;
        private readonly double _nyquist = 44100 / 2.0;

        private readonly double[] _smoothHeights = new double[32];
        private readonly double[] _peakHeights = new double[32];
        private readonly int[] _peakTicks = new int[32];

        private double _runningMax = 0.1;
        private readonly double _agcDecay = 0.995;

        public VisualizerScene()
        {
            // Precompute log-spaced indices for 32 columns
            double lowFreq = 40;
            double highFreq = 12000;
            _fftIndices = new double[32];
            for (int i = 0; i < 32; i++)
            {
                double pct = (double)i / 31.0;
                double freq = lowFreq * Math.Pow(highFreq / lowFreq, pct);
                _fftIndices[i] = (freq / _nyquist) * (1024 / 2);
            }
        }

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // Background: Black
            Color black = Color.FromRgb(0, 0, 0);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    canvas[y, x] = black;
                }
            }

            // Downsample FFT results to 32 bands
            float[] fftData = AudioCapture.Instance.FftResults;
            double[] bandMagnitudes = new double[32];
            double maxVal = 0;

            for (int i = 0; i < 32; i++)
            {
                double idx = _fftIndices[i];
                int lowIdx = (int)Math.Floor(idx);
                int highIdx = (int)Math.Ceiling(idx);
                
                double t = idx - lowIdx;
                double val = 0;

                if (lowIdx < fftData.Length && highIdx < fftData.Length)
                {
                    val = fftData[lowIdx] * (1.0 - t) + fftData[highIdx] * t;
                }
                
                bandMagnitudes[i] = val;
                if (val > maxVal) maxVal = val;
            }

            // Automatic Gain Control (AGC)
            if (maxVal > _runningMax)
            {
                _runningMax = maxVal;
            }
            else
            {
                _runningMax = Math.Max(0.001, _runningMax * _agcDecay);
            }

            // Render columns
            for (int x = 0; x < 32; x++)
            {
                // Normalize and scale to height (32 pixels max)
                double normalized = bandMagnitudes[x] / _runningMax;
                double targetHeight = normalized * 31.0;

                // Smooth decay
                if (targetHeight > _smoothHeights[x])
                {
                    _smoothHeights[x] = targetHeight;
                }
                else
                {
                    _smoothHeights[x] = Math.Max(0.0, _smoothHeights[x] - 0.7); // Fall speed
                }

                int height = (int)Math.Round(_smoothHeights[x]);
                height = Math.Max(0, Math.Min(32, height));

                // Peak tracking
                if (targetHeight >= _peakHeights[x])
                {
                    _peakHeights[x] = targetHeight;
                    _peakTicks[x] = 12; // Hold for 12 frames
                }
                else
                {
                    if (_peakTicks[x] > 0)
                    {
                        _peakTicks[x]--;
                    }
                    else
                    {
                        _peakHeights[x] = Math.Max(0.0, _peakHeights[x] - 0.4); // Peak fall speed
                    }
                }

                // Draw bar columns
                for (int y = 0; y < height; y++)
                {
                    int targetY = 31 - y; // Bottom-up
                    canvas[targetY, x] = GetColorForPixel(x, y);
                }

                // Draw peak dot
                int peakY = 31 - (int)Math.Round(_peakHeights[x]);
                if (peakY >= 0 && peakY < 32)
                {
                    canvas[peakY, x] = Color.FromRgb(255, 255, 255); // White peak dot
                }
            }

            return canvas;
        }

        private Color GetColorForPixel(int x, int y)
        {
            double pos = (double)y / 31.0;

            if (Theme == "synthwave")
            {
                // Magenta (bottom) to Cyan (top)
                byte r = (byte)(255 * (1.0 - pos));
                byte g = (byte)(255 * pos);
                byte b = (byte)(128 * (1.0 - pos) + 255 * pos);
                return Color.FromRgb(r, g, b);
            }
            else if (Theme == "rainbow")
            {
                // Simple hue-based vertical rainbow gradient
                double hue = 120.0 * (1.0 - pos); // Green to Red
                return HSVToColor(hue, 1.0, 1.0);
            }
            else // cyan
            {
                return Color.FromRgb(0, 220, 255);
            }
        }

        private Color HSVToColor(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0) return Color.FromRgb(v, t, p);
            else if (hi == 1) return Color.FromRgb(q, v, p);
            else if (hi == 2) return Color.FromRgb(p, v, t);
            else if (hi == 3) return Color.FromRgb(p, q, v);
            else if (hi == 4) return Color.FromRgb(t, p, v);
            else return Color.FromRgb(v, p, q);
        }

        public void Stop() { }
    }
}
