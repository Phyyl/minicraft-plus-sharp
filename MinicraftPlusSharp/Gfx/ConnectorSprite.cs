using MinicraftPlusSharp.Levels;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Gfx
{
    public class ConnectorSprite
    {
        /**
			This class is meant for those tiles that look different when they are touching other tiles of their type; aka, they "connect" to them.
			
			Since I think connecting tile sprites tend to have three color categories, maybe this should have two extra colors..?
			
			This class will need to keep rack of the following sprites:
			-a sprite for each kind of intersection; aka a 3x3 grid of sprite pixels, that show the sprite for each position, totally surrounded, nothing of left, etc.
			
		*/

        public Sprite sparse, sides, full;
        private Type owner;
        private bool checkCorners;

        public ConnectorSprite(Type owner, Sprite sparse, Sprite sides, Sprite full)
            : this(owner, sparse, sides, full, true)
        {
        }

        public ConnectorSprite(Type owner, Sprite sparse, Sprite sides, Sprite full, bool cornersMatter)
        {
            if (!typeof(Tile).IsAssignableFrom(owner))
            {
                throw new Exception($"Invalid tile type {owner?.GetType().Name}");
            }

            this.owner = owner;
            this.sparse = sparse;
            this.sides = sides;
            this.full = full;
            this.checkCorners = cornersMatter;
        }

        public ConnectorSprite(Type owner, Sprite sparse, Sprite full)
            : this(owner, sparse, sparse, full, false)
        {
        }

        public void Render(Screen screen, Level level, int x, int y)
        {
            Render(screen, level, x, y, -1);
        }

        public void Render(Screen screen, Level level, int x, int y, int whiteTint)
        {
            //Console.WriteLine("rendering sprite for tile " + owner);

            Tile ut = level.GetTile(x, y - 1);
            Tile dt = level.GetTile(x, y + 1);
            Tile lt = level.GetTile(x - 1, y);
            Tile rt = level.GetTile(x + 1, y);

            bool u = ConnectsToDoEdgeCheck(ut, true);
            bool d = ConnectsToDoEdgeCheck(dt, true);
            bool l = ConnectsToDoEdgeCheck(lt, true);
            bool r = ConnectsToDoEdgeCheck(rt, true);

            bool ul = ConnectsToDoEdgeCheck(level.GetTile(x - 1, y - 1), false);
            bool dl = ConnectsToDoEdgeCheck(level.GetTile(x - 1, y + 1), false);
            bool ur = ConnectsToDoEdgeCheck(level.GetTile(x + 1, y - 1), false);
            bool dr = ConnectsToDoEdgeCheck(level.GetTile(x + 1, y + 1), false);

            x = x << 4;
            y = y << 4;


            if (u && l)
            {
                if (ul || !checkCorners)
                {
                    full.RenderPixel(1, 1, screen, x, y);
                }
                else
                {
                    sides.RenderPixel(0, 0, screen, x, y);
                }
            }
            else
            {
                sparse.RenderPixel(l ? 1 : 2, u ? 1 : 2, screen, x, y);
            }

            if (u && r)
            {
                if (ur || !checkCorners)
                {
                    full.RenderPixel(0, 1, screen, x + 8, y);
                }
                else
                {
                    sides.RenderPixel(1, 0, screen, x + 8, y);
                }
            }
            else
            {
                sparse.RenderPixel(r ? 1 : 0, u ? 1 : 2, screen, x + 8, y);
            }

            if (d && l)
            {
                if (dl || !checkCorners)
                {
                    full.RenderPixel(1, 0, screen, x, y + 8);
                }
                else
                {
                    sides.RenderPixel(0, 1, screen, x, y + 8);
                }
            }
            else
            {
                sparse.RenderPixel(l ? 1 : 2, d ? 1 : 0, screen, x, y + 8);
            }

            if (d && r)
            {
                if (dr || !checkCorners)
                {
                    full.RenderPixel(0, 0, screen, x + 8, y + 8);
                }
                else
                {
                    sides.RenderPixel(1, 1, screen, x + 8, y + 8);
                }
            }
            else
            {
                sparse.RenderPixel(r ? 1 : 0, d ? 1 : 0, screen, x + 8, y + 8);
            }
        }

        // it is expected that some tile classes will override this on class instantiation.
        public bool ConnectsTo(Tile tile, bool isSide)
        {
            //Console.WriteLine("original connection check");
            return tile.GetType() == owner;
        }


        public bool ConnectsToDoEdgeCheck(Tile tile, bool isSide)
        {
            if (tile.GetType() == typeof(ConnectTile))
            {
                return true;
            }
            else
            {
                return ConnectsTo(tile, isSide);
            }
        }

        public static Sprite MakeSprite(int w, int h, int mirror, bool repeat, params int[] coords)
        {
            return MakeSprite(w, h, mirror, 1, repeat, coords);
        }
        public static Sprite MakeSprite(int w, int h, int mirror, int sheet, bool repeat, params int[] coords)
        {
            Px[,] pixels = new Px[w, h];

            int i = 0;

            for (int r = 0; r < h && i < coords.GetLength(1); r++)
            {
                for (int c = 0; c < w && i < coords.GetLength(0); c++)
                {
                    int pos = coords[i];

                    pixels[c, r] = new Px(pos % 32, pos / 32, mirror, sheet);

                    i++;

                    if (i == coords.Length && repeat)
                    {
                        i = 0;
                    }
                }
            }

            return new Sprite(pixels);
        }
    }

}
