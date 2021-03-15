using MinicraftPlusSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Levels.Tiles
{
    public class Tiles
    {
        public static List<string> oldids = new();

        private static List<Tile> tiles = new();

        public static void initTileList()
        {
            if (Game.debug)
            {
                Console.WriteLine("Initializing tile list...");
            }

            for (int i = 0; i < 256; i++)
            {
                tiles.Add(null);
            }

            tiles[0] = new GrassTile("Grass");
            tiles[1] = new DirtTile("Dirt");
            tiles[2] = new FlowerTile("Flower");
            tiles[3] = new HoleTile("Hole");
            tiles[4] = new StairsTile("Stairs Up", true);
            tiles[5] = new StairsTile("Stairs Down", false);
            tiles[6] = new WaterTile("Water");
            // this is out of order because of lava buckets
            tiles[17] = new LavaTile("Lava");

            tiles[7] = new RockTile("Rock");
            tiles[8] = new TreeTile("Tree");
            tiles[9] = new SaplingTile("Tree Sapling", Tiles.Get("Grass"), Tiles.Get("Tree"));
            tiles[10] = new SandTile("Sand");
            tiles[11] = new CactusTile("Cactus");
            tiles[12] = new SaplingTile("Cactus Sapling", Tiles.Get("Sand"), Tiles.Get("Cactus"));
            tiles[13] = new OreTile(OreTile.OreType.Iron);
            tiles[14] = new OreTile(OreTile.OreType.Gold);
            tiles[15] = new OreTile(OreTile.OreType.Gem);
            tiles[16] = new OreTile(OreTile.OreType.Lapis);
            tiles[18] = new LavaBrickTile("Lava Brick");
            tiles[19] = new ExplodedTile("Explode");
            tiles[20] = new FarmTile("Farmland");
            tiles[21] = new WheatTile("Wheat");
            tiles[22] = new HardRockTile("Hard Rock");
            tiles[23] = new InfiniteFallTile("Infinite Fall");
            tiles[24] = new CloudTile("Cloud");
            tiles[25] = new CloudCactusTile("Cloud Cactus");
            tiles[26] = new DoorTile(Tile.Material.Wood);
            tiles[27] = new DoorTile(Tile.Material.Stone);
            tiles[28] = new DoorTile(Tile.Material.Obsidian);
            tiles[29] = new FloorTile(Tile.Material.Wood);
            tiles[30] = new FloorTile(Tile.Material.Stone);
            tiles[31] = new FloorTile(Tile.Material.Obsidian);
            tiles[32] = new WallTile(Tile.Material.Wood);
            tiles[33] = new WallTile(Tile.Material.Stone);
            tiles[34] = new WallTile(Tile.Material.Obsidian);
            tiles[35] = new NormalWoolTile("Wool");
            tiles[36] = new PathTile("Path");
            tiles[37] = new RedWoolTile("Red Wool");
            tiles[38] = new BlueWoolTile("Blue Wool");
            tiles[39] = new GreenWoolTile("Green Wool");
            tiles[40] = new YellowWoolTile("Yellow Wool");
            tiles[41] = new BlackWoolTile("Black Wool");
            tiles[42] = new PotatoTile("Potato");

            // WARNING: don't use this tile for anything!
            tiles[255] = new ConnectTile();

            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] == null)
                {
                    continue;
                }

                tiles[i].id = (byte)i;
            }
        }

        protected static void Add(int id, Tile tile)
        {
            tiles[id] = tile;
            Console.WriteLine("Adding " + tile.name + " to tile list with id " + id);
            tile.id = (byte)id;
        }

        static Tiles()
        {
            for (int i = 0; i < 256; i++)
            {
                oldids.Add(null);
            }

            oldids[0] = "grass";
            oldids[1] = "rock";
            oldids[2] = "water";
            oldids[3] = "flower";
            oldids[4] = "tree";
            oldids[5] = "dirt";
            oldids[41] = "wool";
            oldids[42] = "red wool";
            oldids[43] = "blue wool";
            oldids[45] = "green wool";
            oldids[127] = "yellow wool";
            oldids[56] = "black wool";
            oldids[6] = "sand";
            oldids[7] = "cactus";
            oldids[8] = "hole";
            oldids[9] = "tree Sapling";
            oldids[10] = "cactus Sapling";
            oldids[11] = "farmland";
            oldids[12] = "wheat";
            oldids[13] = "lava";
            oldids[14] = "stairs Down";
            oldids[15] = "stairs Up";
            oldids[17] = "cloud";
            oldids[30] = "explode";
            oldids[31] = "Wood Planks";
            oldids[33] = "plank wall";
            oldids[34] = "stone wall";
            oldids[35] = "wood door";
            oldids[36] = "wood door";
            oldids[37] = "stone door";
            oldids[38] = "stone door";
            oldids[39] = "lava brick";
            oldids[32] = "Stone Bricks";
            oldids[120] = "Obsidian";
            oldids[121] = "Obsidian wall";
            oldids[122] = "Obsidian door";
            oldids[123] = "Obsidian door";
            oldids[18] = "hard Rock";
            oldids[19] = "iron Ore";
            oldids[24] = "Lapis";
            oldids[20] = "gold Ore";
            oldids[21] = "gem Ore";
            oldids[22] = "cloud Cactus";
            oldids[16] = "infinite Fall";

            // light/torch versions, for compatibility with before 1.9.4-dev3. (were removed in making dev3)
            oldids[100] = "grass";
            oldids[101] = "sand";
            oldids[102] = "tree";
            oldids[103] = "cactus";
            oldids[104] = "water";
            oldids[105] = "dirt";
            oldids[107] = "flower";
            oldids[108] = "stairs Up";
            oldids[109] = "stairs Down";
            oldids[110] = "Wood Planks";
            oldids[111] = "Stone Bricks";
            oldids[112] = "wood door";
            oldids[113] = "wood door";
            oldids[114] = "stone door";
            oldids[115] = "stone door";
            oldids[116] = "Obsidian door";
            oldids[117] = "Obsidian door";
            oldids[119] = "hole";
            oldids[57] = "wool";
            oldids[58] = "red wool";
            oldids[59] = "blue wool";
            oldids[60] = "green wool";
            oldids[61] = "yellow wool";
            oldids[62] = "black wool";
            oldids[63] = "Obsidian";
            oldids[64] = "tree Sapling";
            oldids[65] = "cactus Sapling";

            oldids[44] = "torch grass";
            oldids[40] = "torch sand";
            oldids[46] = "torch dirt";
            oldids[47] = "torch wood planks";
            oldids[48] = "torch stone bricks";
            oldids[49] = "torch Obsidian";
            oldids[50] = "torch wool";
            oldids[51] = "torch red wool";
            oldids[52] = "torch blue wool";
            oldids[53] = "torch green wool";
            oldids[54] = "torch yellow wool";
            oldids[55] = "torch black wool";
        }

        private static int overflowCheck = 0;

        public static Tile Get(string name)
        {
            //System.out.println("Getting from tile list: " + name);

            name = name.ToUpper();

            overflowCheck++;

            if (overflowCheck > 50)
            {
                Console.WriteLine("STACKOVERFLOW prevented in Tiles.get(), on: " + name);
                Environment.Exit(1);
            }

            //System.out.println("Fetching tile " + name);

            Tile getting = null;

            bool isTorch = false;
            if (name.StartsWith("TORCH"))
            {
                isTorch = true;
                name = name.Substring(6); // cuts off torch prefix.
            }

            if (name.Contains("_"))
            {
                name = name.Substring(0, name.IndexOf("_"));
            }

            foreach (Tile t in tiles)
            {
                if (t == null)
                {
                    continue;
                }

                if (t.name.Equals(name))
                {
                    getting = t;
                    break;
                }
            }

            if (getting == null)
            {
                Console.WriteLine("TILES.GET: Invalid tile requested: " + name);
                getting = tiles[0];
            }

            if (isTorch)
            {
                getting = TorchTile.getTorchTile(getting);
            }

            overflowCheck = 0;
            return getting;
        }

        public static Tile Get(int id)
        {
            //System.out.println("Requesting tile by id: " + id);
            if (id < 0)
            {
                id += 256;
            }

            if (tiles[id] != null)
            {
                return tiles[id];
            }
            else if (id >= 128)
            {
                return TorchTile.getTorchTile(Get(id - 128));
            }
            else
            {
                Console.WriteLine("TILES.GET: Unknown tile id requested: " + id);
                return tiles[0];
            }
        }

        public static bool ContainsTile(int id)
        {
            return tiles[id] != null;
        }

        public static string GetName(String descriptName)
        {
            if (!descriptName.Contains("_"))
            {
                return descriptName;
            }

            int data;
            string[] parts = descriptName.Split("_");
            
            descriptName = parts[0];
            data = int.Parse(parts[1]);

            return Get(descriptName).GetName(data);
        }
    }
}
