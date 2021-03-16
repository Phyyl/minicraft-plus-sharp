using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MinicraftPlusSharp.Items.BucketItem;

namespace MinicraftPlusSharp.Levels
{
    public class Structure
    {
        private HashSet<TilePoint> tiles;
        private Dictionary<Point, Furniture> furniture;

        public Structure()
        {
            tiles = new HashSet<>();
            furniture = new();
        }

        public Structure(Structure structure)
        {
            this.tiles = structure.tiles;
            this.furniture = structure.furniture;
        }

        public void SetTile(int x, int y, Tile tile)
        {
            tiles.Add(new TilePoint(x, y, tile));
        }
        public void AddFurniture(int x, int y, Furniture furniture)
        {
            this.furniture.Add(new Point(x, y), furniture);
        }

        public void Draw(Level level, int xt, int yt)
        {
            foreach (TilePoint p in tiles)
            {
                level.SetTile(xt + p.x, yt + p.y, p.t);
            }

            foreach (Point p in furniture.Keys)
            {
                level.Add(furniture[p].Clone(), xt + p.x, yt + p.y, true);
            }
        }

        public void Draw(byte[] map, int xt, int yt, int mapWidth)
        {
            foreach (TilePoint p in tiles)
            {
                map[(xt + p.x) + (yt + p.y) * mapWidth] = p.t.id;
            }
        }

        public void SetData(string keys, string data)
        {
            // so, the keys are single letters, each letter represents a tile
            Dictionary<string, string> keyPairs = new();
            string[] stringKeyPairs = keys.Split(",");

            // puts all the keys in the keyPairs HashMap
            for (int i = 0; i < stringKeyPairs.Length; i++)
            {
                string[] thisKey = stringKeyPairs[i].Split(":");
                keyPairs.Add(thisKey[0], thisKey[1]);
            }

            string[] dataLines = data.Split("\n");
            int width = dataLines[0].Length;
            int height = dataLines.Length;

            for (int i = 0; i < dataLines.Length; i++)
            {
                for (int c = 0; c < dataLines[i].Length; c++)
                {
                    if (dataLines[i][c] != '*')
                    {
                        Tile tile = Tiles.Tiles.Get(keyPairs[dataLines[i][c].ToString()]);
                        this.SetTile(-width / 2 + i, -height / 2 + c, tile);
                    }
                }
            }
        }

        public class TilePoint
        {
            public int x, y;
            public Tile t;

            public TilePoint(int x, int y, Tile tile)
            {
                this.x = x;
                this.y = y;
                this.t = tile;
            }

            public override bool Equals(object o)
            {
                if (o is not TilePoint p)
                {
                    return false;
                }

                return x == p.x && y == p.y && t.id == p.t.id;
            }

            public override int GetHashCode()
            {
                return x + y * 51 + t.id * 131;
            }
        }

        public static readonly Structure dungeonGate;
        public static readonly Structure dungeonLock;
        public static readonly Structure lavaPool;
        // All the "mobDungeon" structures are for the spawner structures
        public static readonly Structure mobDungeonCenter;
        public static readonly Structure mobDungeonNorth;
        public static readonly Structure mobDungeonSouth;
        public static readonly Structure mobDungeonEast;
        public static readonly Structure mobDungeonWest;

        public static readonly Structure airWizardHouse;

        // used for random villages
        public static readonly Structure villageHouseNormal;
        public static readonly Structure villageHouseTwoDoor;

        public static readonly Structure villageRuinedOverlay1;
        public static readonly Structure villageRuinedOverlay2;

        // ok, because of the way the system works, these structures are rotated 90 degrees clockwise when placed
        // then it's flipped on the vertical
        static Structure()
        {
            dungeonGate = new Structure();
            dungeonGate.SetData("O:Obsidian,D:Obsidian Door,W:Obsidian Wall",
                            "WWDWW\n" +
                            "WOOOW\n" +
                            "DOOOD\n" +
                            "WOOOW\n" +
                            "WWDWW"
                );
            dungeonGate.AddFurniture(-1, -1, new Lantern(Lantern.Type.IRON));

            dungeonLock = new Structure();
            dungeonLock.SetData("O:Obsidian,W:Obsidian Wall",
                            "WWWWW\n" +
                            "WOOOW\n" +
                            "WOOOW\n" +
                            "WOOOW\n" +
                            "WWWWW"
                );

            lavaPool = new Structure();
            lavaPool.SetData("L:Lava",
                            "LL\n" +
                            "LL"
                );

            mobDungeonCenter = new Structure();
            mobDungeonCenter.SetData("B:Stone Bricks,W:Stone Wall",
                            "WWBWW\n" +
                            "WBBBW\n" +
                            "BBBBB\n" +
                            "WBBBW\n" +
                            "WWBWW"
                );
            mobDungeonNorth = new Structure();
            mobDungeonNorth.SetData("B:Stone Bricks,W:Stone Wall",
                            "WWWWW\n" +
                            "WBBBB\n" +
                            "BBBBB\n" +
                            "WBBBB\n" +
                            "WWWWW"
                );
            mobDungeonSouth = new Structure();
            mobDungeonSouth.SetData("B:Stone Bricks,W:Stone Wall",
                            "WWWWW\n" +
                            "BBBBW\n" +
                            "BBBBB\n" +
                            "BBBBW\n" +
                            "WWWWW"
                );
            mobDungeonEast = new Structure();
            mobDungeonEast.SetData("B:Stone Bricks,W:Stone Wall",
                            "WBBBW\n" +
                            "WBBBW\n" +
                            "WBBBW\n" +
                            "WBBBW\n" +
                            "WWBWW"
                );
            mobDungeonWest = new Structure();
            mobDungeonWest.SetData("B:Stone Bricks,W:Stone Wall",
                            "WWBWW\n" +
                            "WBBBW\n" +
                            "WBBBW\n" +
                            "WBBBW\n" +
                            "WBBBW"
                );

            airWizardHouse = new Structure();
            airWizardHouse.SetData("F:Wood Planks,W:Wood Wall,D:Wood Door",
                            "WWWWWWW\n" +
                            "WFFFFFW\n" +
                            "DFFFFFW\n" +
                            "WFFFFFW\n" +
                            "WWWWWWW"
                );
            airWizardHouse.AddFurniture(-2, 0, new Lantern(Lantern.Type.GOLD));
            airWizardHouse.AddFurniture(0, 0, new Crafter(Crafter.Type.Enchanter));

            villageHouseNormal = new Structure();
            villageHouseNormal.SetData("F:Wood Planks,W:Wood Wall,D:Wood Door,G:Grass",
                            "WWWWW\n" +
                            "WFFFW\n" +
                            "WFFFD\n" +
                            "WFFFG\n" +
                            "WWWWW"
                );

            villageHouseTwoDoor = new Structure();
            villageHouseTwoDoor.SetData("F:Wood Planks,W:Wood Wall,D:Wood Door,G:Grass",
                            "WWWWW\n" +
                            "WFFFW\n" +
                            "DFFFW\n" +
                            "WFFFW\n" +
                            "WWDWW"
                );

            villageRuinedOverlay1 = new Structure();
            villageRuinedOverlay1.SetData("G:Grass,F:Wood Planks",
                            "**FG*\n" +
                            "F*GG*\n" +
                            "*G**F\n" +
                            "G*G**\n" +
                            "***G*"
                );

            villageRuinedOverlay2 = new Structure();
            villageRuinedOverlay2.SetData("G:Grass,F:Wood Planks",
                            "F**G*\n" +
                            "*****\n" +
                            "*GG**\n" +
                            "F**G*\n" +
                            "*F**G"
                );
        }
    }
}
