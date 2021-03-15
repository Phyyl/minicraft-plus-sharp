using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Levels.Tiles
{
    public abstract class Tile
    {
        public static int tickCount = 0; // A global tickCount used in the Lava & water tiles.
        protected JavaRandom random = new();

        public enum Material
        {
            Wood, Stone, Obsidian
        }

        public readonly string name;
        public byte id;

        public bool connectsToGrass = false;
        public bool connectsToSand = false;
        public bool connectsToFluid = false;
        public int light;
        protected bool maySpawn;

        protected Sprite sprite;
        protected ConnectorSprite csprite;

        private Tile()
        {
            light = 1;
            maySpawn = false;
            sprite = null;
            csprite = null;
        }

        protected Tile(String name, Sprite sprite)
        {
            this.name = name.ToUpper();
            this.sprite = sprite;
        }
        protected Tile(String name, ConnectorSprite sprite)
        {
            this.name = name.ToUpper();
            csprite = sprite;
        }


        /** This method is used by tiles to specify the default "data" they have in a level's data array.
            Used for starting health, color/type of tile, etc. */
        // at least, that was the idea at first...
        public virtual int getDefaultData()
        {
            return 0;
        }

        /** Render method, used in sub-classes */
        public virtual void render(Screen screen, Level level, int x, int y)
        {
            if (sprite != null)
            {
                sprite.render(screen, x << 4, y << 4);
            }

            if (csprite != null)
            {
                csprite.render(screen, level, x, y);
            }
        }

        public virtual bool MaySpawn() { return maySpawn; }

        /** Returns if the player can walk on it, overrides in sub-classes  */
        public virtual bool MayPass(Level level, int x, int y, Entity e)
        {
            return true;
        }

        /** Gets the light radius of a tile, Bigger number = bigger circle */
        public virtual int GetLightRadius(Level level, int x, int y)
        {
            return 0;
        }

        public virtual bool Hurt(Level level, int x, int y, Mob source, int dmg, Direction attackDir) { return false; }
        public virtual void Hurt(Level level, int x, int y, int dmg) { }

        /** What happens when you run into the tile (ex: run into a cactus) */
        public virtual void BumpedInto(Level level, int xt, int yt, Entity entity) { }

        /** Update method */
        public virtual bool Tick(Level level, int xt, int yt) { return false; }

        /** What happens when you are inside the tile (ex: lava) */
        public virtual void SteppedOn(Level level, int xt, int yt, Entity entity) { }

        /**
         * Called when you hit an item on a tile (ex: Pickaxe on rock).
         * @param level The level the player is on.
         * @param xt X position of the player in tile coordinates (32x per tile).
         * @param yt Y position of the player in tile coordinates (32px per tile).
         * @param player The player who called this method.
         * @param item The item the player is currently holding.
         * @param attackDir The direction of the player attacking.
         * @return Was the operation successful?
         */
        public virtual bool Interact(Level level, int xt, int yt, Player player, Item item, Direction attackDir)
        {
            return false;
        }

        /** Sees if the tile connects to Water or Lava. */
        public virtual bool ConnectsToLiquid() { return connectsToFluid; }

        public virtual int GetData(string data)
        {
            return int.TryParse(data, out int value) ? value : 0;
        }

        public virtual bool Matches(int thisData, string tileInfo)
        {
            return name.Equals(tileInfo.Split("_")[0]);
        }

        public string GetName(int data)
        {
            return name;
        }

        public static string GetData(int depth, int x, int y)
        {
            try
            {
                byte lvlidx = (byte)World.lvlIdx(depth);
                Level curLevel = World.evels[lvlidx];
                int pos = x + curLevel.w * y;

                int tileid = curLevel.tiles[pos];
                int tiledata = curLevel.data[pos];

                return lvlidx + ";" + pos + ";" + tileid + ";" + tiledata;
            }
            catch (NullReferenceException)
            {

            }
            catch (IndexOutOfRangeException)
            {
            }

            return "";
        }

        public override bool Equals(object other)
        {
            if (other is not Tile tile)
            {
                return false;
            }

            return name.Equals(tile.name);
        }

        public override int GetHashCode() { return name.GetHashCode(); }
    }
}
