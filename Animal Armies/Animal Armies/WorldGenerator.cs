using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game
{
    class WorldGenerator
    {

        /**
         * Tile Group are basicly Biomes and contain TileSets of the different tiles for the biome.
         */
        private class Biome
        {
            public Dictionary<string, TileSet> tileSets = new Dictionary<string, TileSet>();
            public string name;

            public Biome(string name, params TileSet[] tileSets)
            {
                foreach (TileSet ts in tileSets)
                {
                    this.tileSets.Add(ts.name, ts);
                }
            }

            public string getTile(string tileName)
            {
                try
                {
                    return tileSets[tileName].random();
                }
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine("Key Not Found in TileGroup: " + name + " : " + tileName);
                }
                return null; //Perhaps a defaulted blank tile?
            }

            public TileSet getTileSet(string tileName)
            {
                try
                {
                    return tileSets[tileName];
                }
                catch (KeyNotFoundException e)
                {
                    Console.WriteLine("Key Not Found for TileSet in TileGroup: " + name + " : " + tileName);
                }
                return null; //Perhaps a defaulted blank tile?
            }
        }

        /**
         * A Tile set is a group of tiles of the same feature. We get a random tile from the group.
         * This is inorder to diverify the terrian, while keeping it the same.
         * Ex. Forest1.png Forest2.png Forest3.png
         *     random() -> Forest2.png
         *     random() -> Forest1.png
         *     random() -> Forest3.png
         */
        private class TileSet
        {
            public string name;

            private List<string> tiles = new List<string>();
            private Random rand;

            public TileSet(string name, params string[] tilePaths)
            {
                this.name = name;
                tiles.AddRange(tilePaths);
                rand = new Random();
            }

            public string random()
            {
                return tiles[rand.Next(tiles.Count)];
            }
        }

        private static Biome prairie = new Biome("prairie",

            new TileSet("grass",
                        "Tiles\\Tiles\\Prairie\\grass.png",
                        "Tiles\\Tiles\\Prairie\\grass1.png",
                        "Tiles\\Tiles\\Prairie\\grass2.png",
                        "Tiles\\Tiles\\Prairie\\grass3.png",
                        "Tiles\\Tiles\\Prairie\\grass4.png"),

            new TileSet("hills",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills1-0.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills1-1.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills1-2.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills1-3.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills1-4.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills2-0.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills2-1.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills2-2.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills2-3.png",
                        "Tiles\\Tiles\\Prairie\\Hills\\Hills2-4.png"),

            new TileSet("forest",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest1-0.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest1-1.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest1-2.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest1-3.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest1-4.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest2-0.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest2-1.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest2-2.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest2-3.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest2-4.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest3-0.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest3-1.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest3-2.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest3-3.png",
                        "Tiles\\Tiles\\Prairie\\Forest\\Forest3-4.png"),

            new TileSet("mountain",
                        "Tiles\\Tiles\\Prairie\\Mountain\\Mountain1-0.png",
                        "Tiles\\Tiles\\Prairie\\Mountain\\Mountain1-1.png",
                        "Tiles\\Tiles\\Prairie\\Mountain\\Mountain1-2.png",
                        "Tiles\\Tiles\\Prairie\\Mountain\\Mountain1-3.png",
                        "Tiles\\Tiles\\Prairie\\Mountain\\Mountain1-4.png"),

            new TileSet("water",
                        "Tiles\\Tiles\\Prairie\\water\\water.png",
                        "Tiles\\Tiles\\Prairie\\water\\water1.png",
                        "Tiles\\Tiles\\Prairie\\water\\water2.png",
                        "Tiles\\Tiles\\Prairie\\water\\water3.png")
        );

        private static Biome snow = new Biome("snow",

            new TileSet("grass",
                        "Tiles\\Tiles\\Snow\\grass0.png",
                        "Tiles\\Tiles\\Snow\\grass1.png",
                        "Tiles\\Tiles\\Snow\\grass2.png",
                        "Tiles\\Tiles\\Snow\\grass3.png"),

            new TileSet("hills",
                        "Tiles\\Tiles\\Snow\\Hills\\hills1-0.png",
                        "Tiles\\Tiles\\Snow\\Hills\\hills1-1.png",
                        "Tiles\\Tiles\\Snow\\Hills\\hills1-2.png"),

            new TileSet("forest",
                        "Tiles\\Tiles\\Snow\\Forest\\forest1-0.png",
                        "Tiles\\Tiles\\Snow\\Forest\\forest1-1.png",
                        "Tiles\\Tiles\\Snow\\Forest\\forest1-2.png",
                        "Tiles\\Tiles\\Snow\\Forest\\forest2-0.png",
                        "Tiles\\Tiles\\Snow\\Forest\\forest2-1.png",
                        "Tiles\\Tiles\\Snow\\Forest\\forest2-2.png",
                        "Tiles\\Tiles\\Snow\\Forest\\forest3-0.png",
                        "Tiles\\Tiles\\Snow\\Forest\\forest3-1.png",
                        "Tiles\\Tiles\\Snow\\Forest\\forest3-2.png"),

            new TileSet("mountain",
                        "Tiles\\Tiles\\Snow\\Mountain\\Mountain1-0.png",
                        "Tiles\\Tiles\\Snow\\Mountain\\Mountain1-1.png",
                        "Tiles\\Tiles\\Snow\\Mountain\\Mountain1-2.png"),

            new TileSet("water",
                        "Tiles\\Tiles\\Snow\\water\\water.png",
                        "Tiles\\Tiles\\Snow\\water\\water1.png",
                        "Tiles\\Tiles\\Snow\\water\\water2.png",
                        "Tiles\\Tiles\\Snow\\water\\water3.png")
        );

        private static Biome temperate = new Biome("temperate",
            new TileSet("grass",
                        "Tiles\\Tiles\\Temperate\\Grass\\grass0.png",
                        "Tiles\\Tiles\\Temperate\\Grass\\grass1.png",
                        "Tiles\\Tiles\\Temperate\\Grass\\grass2.png",
                        "Tiles\\Tiles\\Temperate\\Grass\\grass4.png",
                        "Tiles\\Tiles\\Temperate\\Grass\\grass5.png"),

            new TileSet("hills",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills1-0.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills1-1.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills1-2.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills1-3.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills1-4.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills1-5.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills2-0.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills2-1.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills2-2.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills2-3.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills2-4.png",
                        "Tiles\\Tiles\\Temperate\\Hills\\Hills2-5.png"),

            new TileSet("forest",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest1-0.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest1-1.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest1-2.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest1-3.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest1-4.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest1-5.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest2-0.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest2-1.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest2-2.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest2-3.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest2-4.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest2-5.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest3-0.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest3-1.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest3-2.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest3-3.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest3-4.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest3-5.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest4-0.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest4-1.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest4-2.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest4-3.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest4-4.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest4-5.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest5-0.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest5-1.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest5-2.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest5-3.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest5-4.png",
                        "Tiles\\Tiles\\Temperate\\Forest\\Forest5-5.png"),


            new TileSet("mountain",
                        "Tiles\\Tiles\\Temperate\\Mountain\\Mountain1-0.png",
                        "Tiles\\Tiles\\Temperate\\Mountain\\Mountain1-1.png",
                        "Tiles\\Tiles\\Temperate\\Mountain\\Mountain1-2.png",
                        "Tiles\\Tiles\\Temperate\\Mountain\\Mountain1-3.png",
                        "Tiles\\Tiles\\Temperate\\Mountain\\Mountain1-4.png",
                        "Tiles\\Tiles\\Temperate\\Mountain\\Mountain1-5.png"),

            new TileSet("water",
                        "Tiles\\Tiles\\Temperate\\water\\water.png",
                        "Tiles\\Tiles\\Temperate\\water\\water1.png",
                        "Tiles\\Tiles\\Temperate\\water\\water2.png",
                        "Tiles\\Tiles\\Temperate\\water\\water3.png")


        );

        /**
         * Returns the type of tile, based on filepath
         * Ha.. ha.. ha.. hackkishhh
         */
        public String typeLookUp(String s) 
        {
            if (s.ToLower().Contains("grass"))
                return "grass";
            if (s.ToLower().Contains("water"))
                return "water";
            if (s.ToLower().Contains("mountain"))
                return "mountain";
            if (s.ToLower().Contains("forest"))
                return "forest";
            if (s.ToLower().Contains("hills"))
                return "hills";

            return "grass"; //default..
        }

        private Noise height;
        //private Noise forest;
        private Noise tempature;
        private Noise rainfall;
        private Noise forest;

        public WorldGenerator(int seed, int scale)
        {
            this.masterseed = seed;
            this.masterscale = scale;
        }

        /**
         * Get Tile at Location.
         * Return a tile with overridden properties from the Map.
         * 
         * Very Hackish..
         */
        public GameTile getTile(int x, int y, GameWorld world, ref Engine.Mapfile.TileData tileData)
        {
            //Engine.Mapfile.TileData newTileData = tileData;

            TileSet genTile = generateTile(x,y);

            tileData.texture = genTile.random();

            //newTileData.texture = genTile.random(); //Get the tile from the TileSet.
            //tileData.texture = newTileData.texture;

            GameTile gameTile = new GameTile(world, x, y, tileData);
            gameTile.type = genTile.name;

            return gameTile;
        }

        /**
         * Contains Algorithms for generating the map.
         * Returns tilePath for the for the tile.
         * 
         * Also sets the type and Biome of the GameTile.
         */

        private TileSet generateTile(int x, int y)
        {
            Biome biome = temperate;

            int height = this.height.getTile(x, y);
            int tempature = this.tempature.getTile(x, y);
            int rainfall = this.rainfall.getTile(x, y);
            int forest = this.forest.getTile(x, y);

            //Tundra
            if ((0 <= tempature && tempature < 25) && (0 <= rainfall && rainfall < 50))
            {
                biome = snow; //TODO Tundra
            }
            if ((0 <= tempature && tempature < 25) && (50 <= rainfall && rainfall < 100))
            {
                biome = snow;
            }
            //Desert
            if ((25 <= tempature && tempature < 100) && (0 <= rainfall && rainfall < 25))
            {
                biome = prairie; //TODO Desert
            }
            //Prairie
            if ((25 <= tempature && tempature < 75) && (25 <= rainfall && rainfall < 50))
            {
                biome = prairie;
            }
            //Temperate
            if ((25 <= tempature && tempature < 75) && (50 <= rainfall && rainfall < 100))
            {
                biome = temperate;
            }
            //Jungle
            if ((75 <= tempature && tempature < 100) && (50 <= rainfall && rainfall < 100))
            {
                biome = temperate; //TODO Jungle
            }

            string tileType = "grass"; //Default
            
            //water
            if (0 <= height && height < 20)
                tileType = "water";
            
            //grass
            if (20 <= height && height < 80)
            {
                tileType = "grass";
                if ((rainfall > 75 && biome == temperate) || ((75 <= tempature && tempature < 100) && (50 <= rainfall && rainfall < 100)))
                    tileType = "forest";
            }
            
            //hills
            if (80 <= height && height < 90)
                tileType = "hills";
            
            //mountains
            if (90 <= height && height <= 100)
                tileType = "mountain";
            
            return biome.getTileSet(tileType); //Default.
        }

        private int _masterseed;
        public int masterseed
        {
            get
            {
                return _masterseed;
            }
            set
            {
                _masterseed = value;

                Random r = new Random(value);
                height = new Noise(r.Next());
                tempature = new Noise(r.Next());
                rainfall = new Noise(r.Next());
                forest = new Noise(r.Next());
            }
        }

        private int _masterscale;
        public int masterscale
        {
            get
            {
                return _masterscale;
            }
            set
            {
                _masterscale = value;

                //Manual Scale factor
                height.scale = value * 3;
                tempature.scale = value * 10;
                rainfall.scale = value * 10;

            }
        }

        private class Noise
        {
            public Noise(int seed, int scale = 40)
            {
                this.seed = seed;
                this.scale = scale;
            }

            public int scale;

            /**
             * We repoplutate the perm array with our seed value. 
             */
            private int _seed;
            public int seed
            {
                get
                {
                    return _seed;
                }
                set
                {
                    _seed = value;

                    Random r = new Random(_seed);
                    for (int i = 0; i < perm.Length; i++)
                        perm[i] = (byte)r.Next(256);
                }

            }

            public int getTile(int x, int y)
            {
                float gen = Generate(x / ((float)scale), y / ((float)scale));
                return (int)(gen * 50 + 50); //Scale it from 0 to 100
            }

            /// SimplexNoise for C#
            /// Author: Heikki Törmälä
            /// <summary>
            /// Implementation of the Perlin simplex noise, an improved Perlin noise algorithm.
            /// Based loosely on SimplexNoise1234 by Stefan Gustavson <http://staffwww.itn.liu.se/~stegu/aqsis/aqsis-newnoise/>
            ///
            /// </summary>
            /// <summary>
            /// 2D simplex noise
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns>Returns a value from -1 to 1</returns>
            public float Generate(float x, float y)
            {
                const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
                const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

                float n0, n1, n2; // Noise contributions from the three corners

                // Skew the input space to determine which simplex cell we're in
                float s = (x + y) * F2; // Hairy factor for 2D
                float xs = x + s;
                float ys = y + s;
                int i = FastFloor(xs);
                int j = FastFloor(ys);

                float t = (float)(i + j) * G2;
                float X0 = i - t; // Unskew the cell origin back to (x,y) space
                float Y0 = j - t;
                float x0 = x - X0; // The x,y distances from the cell origin
                float y0 = y - Y0;

                // For the 2D case, the simplex shape is an equilateral triangle.
                // Determine which simplex we are in.
                int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
                if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
                else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)

                // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
                // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
                // c = (3-sqrt(3))/6

                float x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
                float y1 = y0 - j1 + G2;
                float x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
                float y2 = y0 - 1.0f + 2.0f * G2;

                // Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
                int ii = i % 256;
                int jj = j % 256;

                // Calculate the contribution from the three corners
                float t0 = 0.5f - x0 * x0 - y0 * y0;
                if (t0 < 0.0f) n0 = 0.0f;
                else
                {
                    t0 *= t0;
                    n0 = t0 * t0 * grad(perm[ii + perm[jj]], x0, y0);
                }

                float t1 = 0.5f - x1 * x1 - y1 * y1;
                if (t1 < 0.0f) n1 = 0.0f;
                else
                {
                    t1 *= t1;
                    n1 = t1 * t1 * grad(perm[ii + i1 + perm[jj + j1]], x1, y1);
                }

                float t2 = 0.5f - x2 * x2 - y2 * y2;
                if (t2 < 0.0f) n2 = 0.0f;
                else
                {
                    t2 *= t2;
                    n2 = t2 * t2 * grad(perm[ii + 1 + perm[jj + 1]], x2, y2);
                }

                // Add contributions from each corner to get the final noise value.
                // The result is scaled to return values in the interval [-1,1].
                return 40.0f * (n0 + n1 + n2); // TODO: The scale factor is preliminary!
            }

            private byte[] perm = new byte[512] { 151,160,137,91,90,15,
              131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
              190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
              88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
              77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
              102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
              135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
              5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
              223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
              129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
              251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
              49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
              138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
              151,160,137,91,90,15,
              131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
              190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
              88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
              77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
              102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
              135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
              5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
              223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
              129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
              251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
              49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
              138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180 
            };

            private int FastFloor(float x)
            {
                return (x > 0) ? ((int)x) : (((int)x) - 1);
            }

            private float grad(int hash, float x, float y)
            {
                int h = hash & 7;      // Convert low 3 bits of hash code
                float u = h < 4 ? x : y;  // into 8 simple gradient directions,
                float v = h < 4 ? y : x;  // and compute the dot product with (x,y).
                return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
            }


        }



    }
}
