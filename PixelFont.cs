using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public static class PixelFont
    {
        // Custom 3x5 Pixel Font Map
        private static readonly Dictionary<char, string[]> Font5X3 = new Dictionary<char, string[]>
        {
            { 'A', new[] { "###", "# #", "###", "# #", "# #" } },
            { 'B', new[] { "## ", "# #", "## ", "# #", "## " } },
            { 'C', new[] { "###", "#  ", "#  ", "#  ", "###" } },
            { 'D', new[] { "## ", "# #", "# #", "# #", "## " } },
            { 'E', new[] { "###", "#  ", "## ", "#  ", "###" } },
            { 'F', new[] { "###", "#  ", "## ", "#  ", "#  " } },
            { 'G', new[] { "###", "#  ", "# #", "# #", "###" } },
            { 'H', new[] { "# #", "# #", "###", "# #", "# #" } },
            { 'I', new[] { "###", " # ", " # ", " # ", "###" } },
            { 'J', new[] { "  #", "  #", "  #", "# #", "###" } },
            { 'K', new[] { "# #", "# #", "## ", "# #", "# #" } },
            { 'L', new[] { "#  ", "#  ", "#  ", "#  ", "###" } },
            { 'M', new[] { "# #", "###", "###", "# #", "# #" } },
            { 'N', new[] { "###", "# #", "# #", "# #", "# #" } },
            { 'O', new[] { "###", "# #", "# #", "# #", "###" } },
            { 'P', new[] { "###", "# #", "###", "#  ", "#  " } },
            { 'Q', new[] { "###", "# #", "# #", "###", "  #" } },
            { 'R', new[] { "###", "# #", "## ", "# #", "# #" } },
            { 'S', new[] { "###", "#  ", "###", "  #", "###" } },
            { 'T', new[] { "###", " # ", " # ", " # ", " # " } },
            { 'U', new[] { "# #", "# #", "# #", "# #", "###" } },
            { 'V', new[] { "# #", "# #", "# #", "# #", " # " } },
            { 'W', new[] { "# #", "# #", "# #", "###", "# #" } },
            { 'X', new[] { "# #", "# #", " # ", "# #", "# #" } },
            { 'Y', new[] { "# #", "# #", " # ", " # ", " # " } },
            { 'Z', new[] { "###", "  #", " # ", "#  ", "###" } },
            { '0', new[] { "###", "# #", "# #", "# #", "###" } },
            { '1', new[] { " # ", "## ", " # ", " # ", "###" } },
            { '2', new[] { "###", "  #", "###", "#  ", "###" } },
            { '3', new[] { "###", "  #", "###", "  #", "###" } },
            { '4', new[] { "# #", "# #", "###", "  #", "  #" } },
            { '5', new[] { "###", "#  ", "###", "  #", "###" } },
            { '6', new[] { "###", "#  ", "###", "# #", "###" } },
            { '7', new[] { "###", "  #", "  #", "  #", "  #" } },
            { '8', new[] { "###", "# #", "###", "# #", "###" } },
            { '9', new[] { "###", "# #", "###", "  #", "###" } },
            { ' ', new[] { "   ", "   ", "   ", "   ", "   " } },
            { '.', new[] { "   ", "   ", "   ", "   ", " # " } },
            { ',', new[] { "   ", "   ", "   ", " # ", "#  " } },
            { '!', new[] { " # ", " # ", " # ", "   ", " # " } },
            { '?', new[] { "###", "  #", " # ", "   ", " # " } },
            { '-', new[] { "   ", "   ", "###", "   ", "   " } },
            { '_', new[] { "   ", "   ", "   ", "   ", "###" } },
            { '+', new[] { "   ", " # ", "###", " # ", "   " } },
            { '=', new[] { "   ", "###", "   ", "###", "   " } },
            { '(', new[] { " # ", "#  ", "#  ", "#  ", " # " } },
            { ')', new[] { " # ", "  #", "  #", "  #", " # " } },
            { '/', new[] { "  #", "  #", " # ", "#  ", "#  " } },
            { '\\', new[] { "#  ", "#  ", " # ", "  #", "  #" } },
            { ':', new[] { "   ", " # ", "   ", " # ", "   " } },
            { ';', new[] { "   ", " # ", "   ", " # ", "#  " } },
            { '\'', new[] { " # ", " # ", "   ", "   ", "   " } },
            { '"', new[] { "# #", "# #", "   ", "   ", "   " } },
            { '*', new[] { "# #", " # ", "# #", "   ", "   " } },
            { '&', new[] { " # ", "# #", " # ", "# #", " ##" } },
            { '~', new[] { "   ", " # ", "# #", "   ", "   " } }
        };

        public static void DrawText(Color[,] canvas, string text, int startX, int startY, Color textColor, bool wrap = false)
        {
            int h = canvas.GetLength(0);
            int w = canvas.GetLength(1);

            int charWidth = 3;
            int spacing = 1;
            int xOffset = startX;

            foreach (char c in text.ToUpper())
            {
                if (!Font5X3.TryGetValue(c, out var pixels))
                {
                    pixels = Font5X3[' '];
                }

                for (int row = 0; row < 5; row++)
                {
                    int y = startY + row;
                    if (y < 0 || y >= h) continue;

                    string rowStr = pixels[row];
                    for (int col = 0; col < charWidth; col++)
                    {
                        int x = xOffset + col;
                        
                        if (wrap)
                        {
                            x = (x % w + w) % w;
                        }
                        else if (x < 0 || x >= w)
                        {
                            continue;
                        }

                        if (rowStr[col] == '#')
                        {
                            canvas[y, x] = textColor;
                        }
                    }
                }

                xOffset += charWidth + spacing;
            }
        }

        public static int MeasureTextWidth(string text)
        {
            return text.Length * (3 + 1);
        }
    }
}
