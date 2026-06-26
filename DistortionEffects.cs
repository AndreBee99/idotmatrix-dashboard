using System;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public static class DistortionEffects
    {
        public static Color[,] ApplyWaveDistortion(Color[,] pixels, int frameCount, double amplitude = 3.5, double frequency = 0.4)
        {
            int h = pixels.GetLength(0);
            int w = pixels.GetLength(1);
            Color[,] distorted = new Color[h, w];

            for (int y = 0; y < h; y++)
            {
                int shift = (int)Math.Round(amplitude * Math.Sin(y * frequency + frameCount * 0.3));
                for (int x = 0; x < w; x++)
                {
                    int targetX = (x + shift) % w;
                    if (targetX < 0) targetX += w;
                    distorted[y, targetX] = pixels[y, x];
                }
            }
            return distorted;
        }

        public static Color[,] ApplyMeltDistortion(Color[,] pixels, double progress)
        {
            int h = pixels.GetLength(0);
            int w = pixels.GetLength(1);
            Color[,] distorted = new Color[h, w];

            for (int x = 0; x < w; x++)
            {
                // Generate column-specific offset limit (deeper melt)
                int colOffsetMax = 8 + (int)(22 * (Math.Sin(x * 0.7) * 0.5 + 0.5));
                int offset = (int)(colOffsetMax * progress);

                if (offset > 0)
                {
                    // Shift column down
                    for (int y = 0; y < h; y++)
                    {
                        int targetY = y + offset;
                        if (targetY < h)
                        {
                            distorted[targetY, x] = pixels[y, x];
                        }
                    }

                    // Stretch the top pixel (dripping paint effect)
                    Color topPixel = pixels[0, x];
                    for (int y = 0; y < Math.Min(h, offset); y++)
                    {
                        distorted[y, x] = topPixel;
                    }
                }
                else
                {
                    // Copy column directly
                    for (int y = 0; y < h; y++)
                    {
                        distorted[y, x] = pixels[y, x];
                    }
                }
            }
            return distorted;
        }

        public static Color[,] ApplyGlitchDistortion(Color[,] pixels, int frameCount)
        {
            int h = pixels.GetLength(0);
            int w = pixels.GetLength(1);
            Color[,] distorted = new Color[h, w];
            Array.Copy(pixels, distorted, pixels.Length);

            Random rand = new Random(frameCount);

            // Shift 1-2 random horizontal bands (larger shifts)
            for (int i = 0; i < 2; i++)
            {
                int bandY = (int)((Math.Sin(frameCount * 0.5 + i) * 0.5 + 0.5) * (h - 4));
                bandY = Math.Max(0, Math.Min(h - 4, bandY));
                int bandH = 2 + (frameCount % 3);
                int shift = (int)(5 * Math.Cos(frameCount * 0.8 + i));

                if (shift != 0)
                {
                    for (int y = bandY; y < Math.Min(h, bandY + bandH); y++)
                    {
                        Color[] tempRow = new Color[w];
                        for (int x = 0; x < w; x++)
                        {
                            int targetX = (x + shift) % w;
                            if (targetX < 0) targetX += w;
                            tempRow[targetX] = distorted[y, x];
                        }
                        for (int x = 0; x < w; x++)
                        {
                            distorted[y, x] = tempRow[x];
                        }
                    }
                }
            }

            // Chromatic aberration (2px channel shift for visibility)
            if (frameCount % 4 < 2)
            {
                Color[,] shifted = new Color[h, w];
                Array.Copy(distorted, shifted, distorted.Length);

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        // Shift red channel left by 2px
                        int rX = (x - 2 + w) % w;
                        // Shift blue channel right by 2px
                        int bX = (x + 2) % w;

                        byte r = distorted[y, rX].R;
                        byte g = distorted[y, x].G;
                        byte b = distorted[y, bX].B;

                        shifted[y, x] = Color.FromRgb(r, g, b);
                    }
                }
                distorted = shifted;
            }

            return distorted;
        }
    }
}
