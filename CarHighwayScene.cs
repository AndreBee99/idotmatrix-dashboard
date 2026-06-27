using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace idotmatrix_gui
{
    public class TunerCar
    {
        public string Name { get; set; } = "";
        public Color BodyColor { get; set; }
        public Color AccentColor { get; set; } // Rim color
        public string[] Grid { get; set; } = new string[0];
    }

    public class CarHighwayScene : IScene
    {
        public string Name => "Highway Outrun";

        private readonly List<TunerCar> _cars = new List<TunerCar>();

        // Parallax mountain heights
        private readonly int[] _mountainHeights = {
            2, 3, 4, 3, 2, 1, 2, 3, 5, 4, 3, 2, 3, 4, 2, 1,
            2, 3, 4, 3, 2, 1, 3, 5, 6, 5, 4, 3, 2, 1, 2, 3
        };

        public CarHighwayScene()
        {
            // 1. Nissan Skyline GT-R R34 (240x20 Bayside Blue)
            _cars.Add(new TunerCar {
                Name = "SKYLINE R34",
                BodyColor = Color.FromRgb(0, 65, 210),
                AccentColor = Color.FromRgb(200, 200, 200),
                Grid = new string[] {
                    "............................................................................................................BBBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBBBB.......................................................................",
                    "......................................................................................................BBBBBGGGGGGGGGGGBBBBBBBBBBBBBBBBBBBBBBBBGGGGBBBBBBBBBBGGGGKKKGGGGGGGGBBBBBB...............................................................",
                    "...............................................................................................BBBBBBGGGGGGGBBB...............................BKGGG............BBBGGGGGGGGGGGGGGGGBBBBBBBBBBBB...............................HHHHHHBBGGGGGGH....",
                    ".......................................................................................BBBBBBBBGGGGGGGBBB......................................BGGGG................BBBGGGGGGGGGGGGGGGGGBBBBBB.BBBBBBB.......................HGGGGGGGGGGGBH.....",
                    "................................................................................BBBBBBBBBBGGGGGGGGGBB...........................................GGGGG.....................BGGGGGGGGGGGGGGGGGGGGBBBBBBBBBBHHHHHHHHHHHHHHHH...HGGGGGGGGG..........",
                    "...................................................................BBBB...BBBBBB..BBBGGGGGGGGGGGGGGGKG..........................................BGGGGB.BBBBBBBBBBBBBBBBBBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBGGGGGH.....",
                    "....................................BBBBBBBBBBBBBBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBBBBBBBBBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBGGKGHGH....",
                    ".................BBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG.HG....",
                    "........BBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBBBBBGGGGGGGGGGGGGGGGGGGGGGGGGBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGHGB...",
                    "...BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB...",
                    "...BKBB......BB.BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBH",
                    ".BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBHHHBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                    "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB..BBBB.BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.BBBKBBBBBBBB..BBBB.BBBBBBBBBBBBHHBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                    "BBBBBBBBBBBBBBBBBBBBBBB..BBBB..BKKBBBBBBBBB..BBBKB..BBBBBBBBBBBBB.BBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.BBBBBBBBBBBB..BBBKB...BBBBBBBBBKBB.HBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.",
                    ".BBBBBBBBBBBBBBBBBBBB...BBBB..BBBBBBB.BBBBBBBBBBBBBBBBBBB.BKBBBBKB.BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.BKBBBBB..BBBBBBBBBBBBBBBBBB.HKBBBBKH.BBBBBBBBBBBBBBBBBBBBBBBBBBBBBH",
                    "..BBBBBBBKBBBBBBBBBBBBBBBBBB..BBBBBKB...BBBKBBBBBBBBBBBB..BKBBBBKB..BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB..BBBBBKB...BBBKBBBBBBBBKBBB...BBBBBKB..BBBBBBBBBBBBBBBBBBBBBBBBBBBKB",
                    "...BBBBBBBBBBBBBBBBBBBBBBBBB..BBBBBBBBBBBBBBBBBKKBBBBBBBBBBBBBBBB...BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB..BBBBBBBBBBBBBBBBBKKBBBBBBBBBBBBBBKB...BBBBBBBBBBBBBBBBBBBBBBBBBHHH.",
                    ".BBBBBBBBBBBBBBBBBBBBBBBBBBB....BBKBBBBBBB....BBBB...BBBBBBBKBBB....BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB....BBKBBBBBBB....BBBB....BBBBBBKBBH....HBBBBBBBBBBHHHHHH............",
                    ".....BBBBBBBBBBBBBBBBBBBBBBB......BBBBBBBBBBBBBBBBBBBBBBBBBBBB.......BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.......BBBBBBBBBBBBBBBBBBBBBBBBBBBH...................................",
                    ".....................................BBBBBBBBBBBBBBBBBBBBB...........................................................................................................................BBBBBBBBBBBBBBBBBBBB.......................................",
                }
            });

            // 2. Toyota Supra MK4 (240x20 Candy Orange)
            _cars.Add(new TunerCar {
                Name = "SUPRA MK4",
                BodyColor = Color.FromRgb(255, 85, 0),
                AccentColor = Color.FromRgb(190, 190, 195),
                Grid = new string[] {
                    "....................................................................................................................BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.........................................................................",
                    ".............................................................................................................BBBBGGGKBBBBBBBKKKBBBGGGGGGGGGGGGGGBBBBBBBBBBBBBBBBBBBBBBBBBBBBGGGBBBBBBB....................................HHBBBBBBBBBBBBBBH.....",
                    "......................................................................................................BBBBGGGGBKKKKKKKKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKKGGGGGGBBBBBBBBBBBBBBBBBBBBBGGGGGGGBBBBB...................HBBBKKKKBBBBBBBBBBBH.......",
                    "..............................................................................................BBBBBGGGGGBBBBKKKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKKGGGGGGGGGGGGKKKKBBBBBBBBBBBBBBBBBBBBBBGGGGGGGGBBHHHHHHBBBBKKKBBBBBBBBBBBBBH..........",
                    ".......................................................................................BBBBGGGGGGGBBBBBBBBBKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKKKKKKKKKKKGGGGGGGGGGKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKBBBBBBBBBBBBBBBKBBHH........",
                    "........................................................................BBBBBBBBBBBBGGGGGGGGGBBBBKKBBBBBBBBBKGKKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKKKKKKKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.......",
                    "........................................BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKG......",
                    ".........................BBBBBBLLLLLLLLLBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKGH....",
                    "...............BBBBBLLLLLLLLLLLLLLLLLLLLBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                    "........BBBBBBBBBBKLLLLLLLLLLLLLLLLLLLLLBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBK",
                    "....BBBBBBBBBBBBBLLLLLLLLLLLLLLLLLLLLKKKKKKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                    ".BLLLLBBBBBBBBLLLLLLLLLLLLLLLLLLKKKKKKKKKKKKKKKBBKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKBBKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                    "KLLLLLLLLLLLLLLLLLLLLLLLLLLLLLKKKKKKKKKKKKKKKKKBBKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKKBKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKB",
                    "BLLLKBBBBBLLLLLLLLLLLLLLLLLLLKKKKKKKKBKKKBBBKKKBKBKKBKBBBKBBKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKBBBBBBBBKKKKKKKKBKKKBBBBKKBKBKKKBKBBKBBKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                    ".BKLLLLLKKLLLLLLLLLLLLLLLLLLKKKKKKBKKKKBBBKBKKBKKKBKKBBBBBKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBKKKKKKKKKKKKBBKBKKBKKKBKKBBBBBKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKH",
                    ".BKLLLLLLLLLLLLLLLLLLLLLLLLKKKKKKKKKKKKKBBKKBBKKBKBBBKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKBBKKBBKKBKKBBKKBKKKKKBKKKKKKKKKKKKBBBBBBBKKKKKKKBBBBBBBBBBKKK",
                    ".BKKKKKKLKKKKKKKKKKKKLLLLLLKKKKKKKKKBBKKKKBBBBBBKBBBBBBKKKKBBKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBKKKKKKKKBBKKKKBBBBBBKBBBBBBKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKBKBBBBBBBBBBBH",
                    ".BKKKKKKKKKKKKKKKKKKKKKKKKKKBBBBBBKKKKBKKBBKKKKKKKKKKKBBKBKKKKKBBBBBBBKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKBBBBKKKKKKKBBKKKKKKKKKKKBBKBBKKKKBBBBBBBBBBBBBBBBBBBBHHHHHHHHH.........",
                    "...........BBBBBBBBBBBBBBBBB......BBBKKKKKKKKKKKKKKKKKKKKKKKBBB.......BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB....BBBKKKKKKKKKKKKKKKKKKKKKKKBBH......................................",
                    "...BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBHHHH...",
                }
            });

            // 3. Mazda RX-7 FD (240x20 Vintage Red)
            _cars.Add(new TunerCar {
                Name = "RX-7 FD",
                BodyColor = Color.FromRgb(215, 0, 25),
                AccentColor = Color.FromRgb(180, 180, 185),
                Grid = new string[] {
                    ".................................................................................................................BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.B........................................................................",
                    ".........................................................................................................BBBBBBBBBBBBBBKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBB.............................................................",
                    "...................................................................................................BBBBBB...BBBBBBBBBBBBBBBB.............................BBBBBBBBBBBBBBBBBBBBBBB...BBBBBBBBBB...................................................",
                    "............................................................................................BBBBBB.....BBBBBBBBBBBBBB.....................................BKKKBBBBBBBBBBBBBBBBKBBBB..........BBBBBBBB...........................................",
                    "......................................................................................BBBBBBB.....BBBBBBBBBBBBB...........................................KKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBB......BBBBBBBHHHHHH...............HHHHHHHHHBBB......",
                    "...............................................................................BBBBBBB.......BBBBBBBBBBBBBBBBBBB.........................................BKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBHHHBBBBBBBBBBBBKBBBBBBH......",
                    "....................................................BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB......",
                    "..............................BBBBBLLLLLBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKBH.BB......",
                    "..................BBBLLLLLLLLLLLLLLLLLLLBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKB.HBH......",
                    "...........BBLLLLLLLLLLLLLLLLLLLLLLLLLLLBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB......",
                    ".....BLLLLLLLLLLLLLLLLLLLLLLLLLLLLLKKKKLBBBBBBBBBBBBKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKBBBBBBBBBBBBBKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBH.",
                    "..BLLLLLLLLLLLLLLLLLLLLLLLLLLLLKKKKLLLLLBBBBBBBBBBBBBBBBBKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKBBBBBBBBBBBBBBBBBBBBBBKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB",
                    "BLLLB...BLLLLLLLLLLLLLLLLLLLKKKKLLLBLLLLBBBBBKKBBBBBBBBBBBBBKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKBBBBBBBBBBBBBBKBBBBBBKBBBBBKKKKKBBBBBBBBBBBHHHHHHHBBBBBBBBBBBBBBBHHB",
                    "LKLLLLLLKKLLLLLLLLLLLLLLLLKKKKKLLLLLKKLLBBBBBBBBBBBBBBKKBBBBBKKKKKBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKBBBBKKKBBBBBBBBBBBBBBBKKKBBBBBKKKKKBHBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBH",
                    ".LKLLLLLLLLLLLLLLLLLLLKLBKKKKKLLBLKKLLLLBBBBBBBB.BBBBBBKKKBBBKKKKKKBBKBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBKKKKKBBBBKKBBBBBBBBBBBBBBKBBBBKKKBBBKKKKKKBHBBBBBBBBBBBBBBBBBBBBBBBBBBBBH..",
                    "..LLLLLLLLLLLLLLLLLLLLLLLKKKKKKLLBKKKB..BBBBBBBBBBB...BKKBBBBKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKBBBBKKBB..BBBBBBBBBBB...BKKBBBBKKKKKKKHBBBBBBBBBBBBBBBBBBBBBKKKKKKBHH.",
                    "...LLLLLLLLLLLLLLLLLLLLBLKKKKKKKLLBLLKLLBKBBB..BBBKBBKKBBBBBKKKKKKKKBBKBKKKKKBBBBBKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKBKBBKBBKKKBBBBBKKBBBBBB...BBBKBBKKBBBBBKKKKKKKKBBBBBBBBBBKKKKKKKKKBBKBBBBBBBH..",
                    ".BLLLLLLLLLLLLLLLLKKKKKLBLLLLLLKKKLLLLLLBBBBBBBBBBBBBBBBBKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB.....BBKKKBBBBBBBBBBBBBBBBBBBBBBBKKKKBHHHHHHHBBBBBBBBBHHHHHHHHHH............",
                    "...............................BBLLKKKLLBBBBBBBBBBBBBBKKKBBB...............................................................................................................BBBBKKKBBBBBBBBBBBBBBBBKKKBBB........................................",
                    "....................................BBLLBBKKKKKKKKBBBBBB.......................................................................................................................BBBBBBBKKKKKKKKBBBBBB............................................",
                }
            });

            // 4. Subaru WRX STI (240x20 Rally Blue)
            _cars.Add(new TunerCar {
                Name = "WRX STI",
                BodyColor = Color.FromRgb(0, 90, 190),
                AccentColor = Color.FromRgb(218, 165, 32),
                Grid = new string[] {
                    "...........................................................................................................BBBBBBBBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBBGGGGGGGGGG...........................................................",
                    ".................................................................................................BBBGGGGGGGGGGGGGKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKGGGGGGGGGGGGGGGGBBBB............................HHBGGBGGGGGGGBHHH...",
                    "........................................................................................BBBGGGGGKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKGGGGGGGGGGGGGGGBBB............HHGGKKKKKKKGGGGGGGGGGKGH...",
                    "................................................................................BBBGGGGGKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKGGGGGGGGGGKKKGGGGGGBHHBGGKKKKKKGGGGGGGGGGGGGGGH.....",
                    ".........................................................................BBBGGGGKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKGGGGGGGGGGGGGGGGKKKKKKKGGGGGGGGGGGGGGKGKGH.......",
                    ".................................................................BBBGGGGGKKKKKKKKKKKKKKKKGGGGGGGKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGH......",
                    "............................BBBBBBBBBBBBBBBBBBBBBBBBBGGGGGGGGGGGGGKKGGGGKKKKKKKKKKKKKKKKKKKKKKKGKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKGGKKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKGGGGGGGGGGGGGGGGGGGGGGGGGGKBBBBBBBBBBBBBBBKG......",
                    ".....................BBBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKGGKGGGGGGGGGKKKKKKKKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKKKKKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBBBBBBBBBBBBBKBBKG.....",
                    "..........BBBBGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGKKKKKKKKGKGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGBKBBBBBBBGK.....",
                    "....BBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB...",
                    "...BKBKKKKKKBBBBBBBBBBBLBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBH.",
                    "..BKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKB",
                    ".BKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKBBBBBBKKKKKKKBBBBBKKKKKKKKBBBBBKBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKBBBBBKKKKKKKKBBBBBKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBB",
                    ".BBBKKKKKKKBBBBBBBBBBBBBBBBBBBBKKKKKKKBBBKKKBBBBKKKKBBBBBKKKBBBKKKKKKBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKBBBKKKBBBBKKKKBBBBKKKBBBKKKKKKKBBBBBBBBBBBBBBBBBBBBBBKKBBBBKB",
                    ".BKBKKKKKKKKBBKBBBBBBBBBBBBBBBKKKKKKKBBBKKKBBKBBBBKBBBBKBBBKKBBBKKKKKKBBBBKBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKBBBKKKKKKKBBKKKBBBBBBBBBBBBBKBBKKKBBBKKKKKKBBBBBBBBBBBBBBBBBBBBKKKKKKKB.",
                    ".BBBBBKKKKKKKKKBBBBBBBBBBBBBBBKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKBBBBBBBBBBBBBBBBBKKKKKKKBBB..",
                    "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKBBBKKKKBBBBBBBBBBBKKKKKKBBBKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKBBBKKKKKBBBBBBBBBBKKKKKBBBKKKKBBKBBBBBBBBBBBBBBBBBBBHHHHHHH...",
                    "............BBBBBBBBBBBBBBBBBBBBBBBKKKKKBBBKBBBKKKKKKKBBBKBBBKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKBBBKBBBBKKKKKKBBBBKBBBKKKKBH................................",
                    "....................................BBBBKKKKBBBBBBBBBBBBBKKKKBBB.................................................................................................................BBBKKKKKBKKBBBBBBKKBBKKKKBBH...................................",
                    ".....BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBKKKKKKKKKKKKKKKKKKBBBBBBBBBBBBBBBBBBBBBBBBBHHHHHHHHHH.....",
                }
            });
        }

        public Color[,] DrawFrame(int frameCount)
        {
            Color[,] canvas = new Color[32, 32];

            // 300 frame cycle (15 seconds per car at 20 FPS)
            int carCycle = frameCount / 300;
            int activeCarIndex = carCycle % _cars.Count;
            TunerCar activeCar = _cars[activeCarIndex];

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

            // 8. Calculate Scrolling position for Car
            // xPos goes from -240 (offscreen left) to 32 (offscreen right) over 300 frame cycle
            int localFrame = frameCount % 300;
            int xPos = -240;

            if (localFrame < 80)
            {
                // Entry: Slowly roll in from -240 to -120 (reveals front & cabin)
                double t = (double)localFrame / 80.0;
                t = t * (2.0 - t); // Ease out
                xPos = (int)(-240.0 + 120.0 * t);
            }
            else if (localFrame < 220)
            {
                // Cruise/Pass: Scroll slowly across showing full body profile from -120 to -150
                double t = (double)(localFrame - 80) / 140.0;
                xPos = (int)(-120.0 - 30.0 * t);
            }
            else if (localFrame < 265)
            {
                // Accelerate: Drop gear and blast off offscreen from -150 to 32
                double t = (double)(localFrame - 220) / 45.0;
                t = t * t; // Ease in (accelerate)
                xPos = (int)(-150.0 + 182.0 * t);
            }
            else
            {
                // Offscreen wait
                xPos = 32;
            }

            // 9. Draw Volumetric Headlight Glow (shining right)
            DrawBeamsAndCar(canvas, activeCar, xPos, 8, frameCount);

            // 10. Draw HUD overlay showing car name for first 60 frames of car pass
            if (localFrame < 60 && xPos > -210)
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
            for (int dx = 0; dx < 12; dx++)
            {
                int x = startX + dx;
                if (x >= 32) break;

                int spread = dx / 2;
                for (int dy = -spread; dy <= spread; dy++)
                {
                    int y = startY + dy;
                    if (y >= 0 && y < 32)
                    {
                        Color current = canvas[y, x];
                        double intensity = 0.35 * (1.0 - (double)dx / 12.0);

                        canvas[y, x] = Color.FromRgb(
                            (byte)(current.R * (1.0 - intensity) + glow.R * intensity),
                            (byte)(current.G * (1.0 - intensity) + glow.G * intensity),
                            (byte)(current.B * (1.0 - intensity) + glow.B * intensity)
                        );
                    }
                }
            }
        }

        private void DrawBeamsAndCar(Color[,] canvas, TunerCar car, int xPos, int yPos, int frameCount)
        {
            Color body = car.BodyColor;
            Color black = Color.FromRgb(10, 10, 15);
            Color glass = Color.FromRgb(30, 40, 65); // Dark blue glass
            Color headlight = Color.FromRgb(255, 255, 180);
            Color taillight = Color.FromRgb(255, 0, 0);
            Color exhaustFlame = Color.FromRgb(255, 120, 0);
            Color intercooler = Color.FromRgb(200, 200, 205);

            // Phase 1: Draw Headlight Beams by scanning for 'H' in the grid
            for (int r = 0; r < car.Grid.Length; r++)
            {
                string rowStr = car.Grid[r];
                for (int c = 0; c < rowStr.Length; c++)
                {
                    if (rowStr[c] == 'H')
                    {
                        int targetX = xPos + c;
                        int targetY = yPos + r;
                        if (targetX >= 0 && targetX < 32)
                        {
                            DrawHeadlightBeam(canvas, targetX, targetY);
                        }
                    }
                }
            }

            // Phase 2: Draw the car body pixels
            for (int r = 0; r < car.Grid.Length; r++)
            {
                string rowStr = car.Grid[r];
                for (int c = 0; c < rowStr.Length; c++)
                {
                    char ch = rowStr[c];
                    int targetX = xPos + c;
                    int targetY = yPos + r;

                    if (ch == '.') continue; // Transparent

                    Color pxColor = black;
                    if (ch == 'B') pxColor = body;
                    else if (ch == 'K') pxColor = black;
                    else if (ch == 'G') pxColor = glass;
                    else if (ch == 'H') pxColor = headlight;
                    else if (ch == 'L') pxColor = taillight;
                    else if (ch == 'S') pxColor = intercooler;

                    DrawPixel(canvas, targetX, targetY, pxColor);
                }
            }

            // Phase 3: Draw exhaust flame (tailpipe is at the left-most edge of the bottom rows)
            int exhaustX = xPos;
            int exhaustY = yPos + 17; // Exhaust at lower body level (row 17)
            string bottomRow = car.Grid[17];
            for (int c = 0; c < bottomRow.Length; c++)
            {
                if (bottomRow[c] != '.')
                {
                    exhaustX = xPos + c - 1;
                    break;
                }
            }

            int exhaustOffset = frameCount % 6;
            if (exhaustOffset < 2)
            {
                DrawPixel(canvas, exhaustX, exhaustY, exhaustFlame);
            }
            else if (exhaustOffset == 2)
            {
                DrawPixel(canvas, exhaustX, exhaustY, Color.FromRgb(255, 200, 0));
                DrawPixel(canvas, exhaustX - 1, exhaustY, exhaustFlame);
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
        }
    }
}
