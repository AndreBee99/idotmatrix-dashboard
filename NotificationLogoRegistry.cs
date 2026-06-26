using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public static class NotificationLogoRegistry
    {
        private static readonly Dictionary<char, Color> Palette = new Dictionary<char, Color>
        {
            { 'w', Color.FromRgb(255, 255, 255) }, // White
            { 'k', Color.FromRgb(0, 0, 0) },       // Black
            { 'd', Color.FromRgb(88, 101, 242) },   // Discord Blurple
            { 'g', Color.FromRgb(30, 215, 96) },    // Spotify Green
            { 'b', Color.FromRgb(0, 120, 215) },   // Outlook Blue
            { 'p', Color.FromRgb(112, 117, 250) },  // Teams Purple
            { 's', Color.FromRgb(37, 211, 102) },   // WhatsApp Green
            { 'c', Color.FromRgb(0, 220, 255) },    // Cyan
            { 'r', Color.FromRgb(224, 62, 74) },    // Slack Red
            { 'y', Color.FromRgb(236, 178, 46) },   // Slack Yellow
            { 'a', Color.FromRgb(46, 182, 125) }    // Slack Green
        };

        // 1. Generic Speech Bubble
        private static readonly string[] GenericLarge = {
            "................",
            "................",
            "..cccccccccccc..",
            ".cccccccccccccc.",
            ".ccwwwwwwwwwwcc.",
            "ccwwwwwwwwwwwwcc",
            "ccwwwwwwwwwwwwcc",
            "ccwwwwwwwwwwwwcc",
            "ccwwwwwwwwwwwwcc",
            "ccwwwwwwwwwwwwcc",
            "ccwwwwwwwwwwwwcc",
            ".ccwwwwwwwwwwcc.",
            ".cccccccccccccc.",
            "..cccccccccc....",
            "....cccccc......",
            "......cc........"
        };
        private static readonly string[] GenericSmall = {
            "cccccc..",
            "cwwwwc..",
            "cwwwwc..",
            "cwwwwc..",
            "ccwwcc..",
            ".cccc...",
            "..cc....",
            "........"
        };

        // 2. Discord mascot
        private static readonly string[] DiscordLarge = {
            "................",
            "................",
            "....dddddddd....",
            "..dddddddddddd..",
            ".dddddddddddddd.",
            ".ddd..dddd..ddd.",
            ".ddd..dddd..ddd.",
            ".dddddddddddddd.",
            ".dddd......dddd.",
            "..dddd....dddd..",
            "...dddddddddd...",
            "....dddddddd....",
            "................",
            "................",
            "................",
            "................"
        };
        private static readonly string[] DiscordSmall = {
            "........",
            ".dddddd.",
            "d.dd.ddd",
            "dddddddd",
            "dd....dd",
            ".dddddd.",
            "..dddd..",
            "........"
        };

        // 3. Spotify
        private static readonly string[] SpotifyLarge = {
            "................",
            "....gggggggg....",
            "..gggggggggggg..",
            ".ggggkkkkkkgggg.",
            ".gggkk....kkggg.",
            "ggkkkkkkkkkkkkgg",
            "ggkk........kkgg",
            "gg...kkkkkk...gg",
            "gg..kk....kk..gg",
            "gg............gg",
            "gg....kkkk....gg",
            "gg...kk..kk...gg",
            ".g............g.",
            "..gggggggggggg..",
            "....gggggggg....",
            "................"
        };
        private static readonly string[] SpotifySmall = {
            ".gggggg.",
            "g.kk.g.g",
            "g....ggg",
            "g.kk.ggg",
            "g....ggg",
            "g.kk.ggg",
            ".gggggg.",
            "........"
        };

        // 4. Slack
        private static readonly string[] SlackLarge = {
            "................",
            "....rr....bb....",
            "....rr....bb....",
            "..rrrrrrbbbbbb..",
            "..rrrrrrbbbbbb..",
            "....rr....bb....",
            "....rr....bb....",
            "....yy....a.....",
            "....yy....a.....",
            "..yyyyyyaaaaaa..",
            "..yyyyyyaaaaaa..",
            "....yy....a.....",
            "....yy....a.....",
            "................",
            "................",
            "................"
        };
        private static readonly string[] SlackSmall = {
            "..r..b..",
            "rrrrbbbb",
            "..r..b..",
            "..y..a..",
            "yyyyaaaa",
            "..y..a..",
            "........",
            "........"
        };

        // 5. Outlook
        private static readonly string[] OutlookLarge = {
            "................",
            "................",
            ".bbbbbbbbbbbbbb.",
            ".bwwwwwwwwwwwwb.",
            ".bw.w......w.wb.",
            ".bw..w....w..wb.",
            ".bw...w..w...wb.",
            ".bw....ww....wb.",
            ".bw....ww....wb.",
            ".bw...w..w...wb.",
            ".bw..w....w..wb.",
            ".bw.w......w.wb.",
            ".bwwwwwwwwwwwwb.",
            ".bbbbbbbbbbbbbb.",
            "................",
            "................"
        };
        private static readonly string[] OutlookSmall = {
            "bbbbbbbb",
            "bwwwwwwb",
            "bw.ww.wb",
            "bw.ww.wb",
            "bww..wwb",
            "bwwwwwwb",
            "bbbbbbbb",
            "........"
        };

        // 6. WhatsApp
        private static readonly string[] WhatsAppLarge = {
            "................",
            ".....ssssss.....",
            "...ssssssssss...",
            "..ssssssssssss..",
            ".ssssswwwwwwsss.",
            ".ssssww....wwss.",
            "ssssww......wwss",
            "ssssw........wss",
            "ssssw........wss",
            "ssssw........wss",
            "ssssww......wwss",
            ".ssssww....wwss.",
            ".ssssswwwwwwss..",
            "..sssssssssss...",
            "...sssssssss....",
            ".....sssss......"
        };
        private static readonly string[] WhatsAppSmall = {
            ".ssssss.",
            "s.www.ss",
            "s.w.w.ss",
            "s.www.ss",
            "s....sss",
            ".ssssss.",
            "..sss...",
            "........"
        };

        public static (Color[,] Large, Color[,] Small) GetLogos(string appName)
        {
            string name = appName.ToLowerInvariant();

            if (name.Contains("discord"))
                return (ParseArt(DiscordLarge), ParseArt(DiscordSmall));
            
            if (name.Contains("spotify"))
                return (ParseArt(SpotifyLarge), ParseArt(SpotifySmall));
            
            if (name.Contains("slack"))
                return (ParseArt(SlackLarge), ParseArt(SlackSmall));
            
            if (name.Contains("outlook") || name.Contains("mail") || name.Contains("email"))
                return (ParseArt(OutlookLarge), ParseArt(OutlookSmall));
            
            if (name.Contains("whatsapp"))
                return (ParseArt(WhatsAppLarge), ParseArt(WhatsAppSmall));

            // Default fallback
            return (ParseArt(GenericLarge), ParseArt(GenericSmall));
        }

        private static Color[,] ParseArt(string[] lines)
        {
            int h = lines.Length;
            int w = lines[0].Length;
            Color[,] pixels = new Color[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    char c = lines[y][x];
                    if (Palette.TryGetValue(c, out Color col))
                    {
                        pixels[y, x] = col;
                    }
                    else
                    {
                        pixels[y, x] = Color.FromArgb(0, 0, 0, 0); // Transparent
                    }
                }
            }
            return pixels;
        }
    }
}
