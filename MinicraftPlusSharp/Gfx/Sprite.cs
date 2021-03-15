using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Gfx
{
    public class Sprite
    {
        private static JavaRandom ran = new();

        public static Sprite MissingTexture(int w, int h)
        {
            return new Sprite(30, 30, w, h, 1);
        }

        public static Sprite Blank(int w, int hm, int col)
        {
            return new Sprite(7, 2, w, hm, Color.Get(col, col));
        }

        public static Sprite Repeat(int sx, int sy, int w, int h)
        {
            return ConnectorSprite.MakeSprite(w, h, 0, true, sx + sy * 32);
        }

        public static Sprite Dots(int col)
        {
            return ConnectorSprite.MakeSprite(2, 2, 0, false, 0, 1, 2, 3);
        }

        public static Sprite RandomDots(long seed, int offset)
        {
            ran.SetSeed(seed);
            return ConnectorSprite.makeSprite(
                2,
                2,
                ran.NextInt(4),
                1,
                false,
                (2 + ran.NextInt(4)) + offset * 32,
                (2 + ran.NextInt(4)) + offset * 32,
                (2 + ran.NextInt(4)) + offset * 32,
                (2 + ran.NextInt(4)) + offset * 32);
        }

        protected Px[,] spritePixels;
        public int color = -1;
        protected Rectangle sheetLoc;

        public Sprite(int pos, int sheet)
            : this(pos % 32, pos / 32, 1, 1, sheet)
        {
        }

        public Sprite(int sx, int sy, int sheet)
            : this(sx, sy, 1, 1, sheet)
        {
        }

        public Sprite(int sx, int sy, int sw, int sh)
            : this(sx, sy, sw, sh, 0, 0)
        {
        }

        public Sprite(int sx, int sy, int sw, int sh, int sheet)
            : this(sx, sy, sw, sh, sheet, 0)
        {
        }

        public Sprite(int sx, int sy, int sw, int sh, int sheet, int mirror)
            : this(sx, sy, sw, sh, sheet, mirror, false)
        {
        }

        public Sprite(int sx, int sy, int sw, int sh, int sheet, int mirror, bool onepixel)
        {
            sheetLoc = new Rectangle(sx, sy, sw, sh);

            spritePixels = new Px[sw, sh];
            for (int r = 0; r < sh; r++)
            {
                for (int c = 0; c < sw; c++)
                {
                    spritePixels[c, r] = new Px(sx + (onepixel ? 0 : c), sy + (onepixel ? 0 : r), mirror, sheet);
                }
            }
        }
        public Sprite(int sx, int sy, int sw, int sh, int sheet, bool onepixel, int[][] mirrors)
        {
            sheetLoc = new Rectangle(sx, sy, sw, sh);

            spritePixels = new Px[sw, sh];
            for (int r = 0; r < sh; r++)
            {
                for (int c = 0; c < sw; c++)
                {
                    spritePixels[c, r] = new Px(sx + (onepixel ? 0 : c), sy + (onepixel ? 0 : r), mirrors[r][c], sheet);
                }
            }
        }

        public Sprite(Px[,] pixels)
        {
            spritePixels = pixels;
        }









        public int GetPos()
        {
            return sheetLoc.X + sheetLoc.Y * 32;
        }

        public Size GetSize()
        {
            return sheetLoc.Size;
        }

        public void render(Screen screen, int x, int y)
        {
            // here, x and y are screen coordinates.
            for (int row = 0; row < spritePixels.GetLength(1); row++)
            { // loop down through each row
                renderRow(row, screen, x, y + row * 8);
            }
        }
        public void render(Screen screen, int x, int y, int mirror)
        {
            for (int row = 0; row < spritePixels.GetLength(1); row++)
            {
                renderRow(row, screen, x, y + row * 8, mirror);
            }
        }
        public void render(Screen screen, int x, int y, int mirror, int whiteTint)
        {
            for (int row = 0; row < spritePixels.GetLength(1); row++)
            {
                renderRow(row, screen, x, y + row * 8, mirror, whiteTint);
            }
        }

        public void renderRow(int r, Screen screen, int x, int y)
        {
            for (int c = 0; c < spritePixels.GetLength(0); c++)
            { // loop across through each column
                screen.render(x + c * 8, y, spritePixels[c, r].sheetPos, spritePixels[c, r].mirror, spritePixels[c, r].sheetNum, this.color); // render the sprite pixel.
            }
        }
        public void renderRow(int r, Screen screen, int x, int y, int mirror)
        {
            for (int c = 0; c < spritePixels.GetLength(0); c++)
            { // loop across through each column
                screen.render(x + c * 8, y, spritePixels[c, r].sheetPos, mirror, spritePixels[c, r].sheetNum, this.color); // render the sprite pixel.
            }
        }
        public void renderRow(int r, Screen screen, int x, int y, int mirror, int whiteTint)
        {
            for (int c = 0; c < spritePixels.GetLength(0); c++)
            {
                screen.render(x + c * 8, y, spritePixels[c, r].sheetPos, (mirror != -1 ? mirror : spritePixels[c, r].mirror), spritePixels[c, r].sheetNum, whiteTint);
            }
        }

        protected void renderPixel(int c, int r, Screen screen, int x, int y)
        {
            renderPixel(c, r, screen, x, y, spritePixels[c, r].mirror);
        }
        protected void renderPixel(int c, int r, Screen screen, int x, int y, int mirror)
        {
            renderPixel(c, r, screen, x, y, mirror, this.color);
        }
        protected void renderPixel(int c, int r, Screen screen, int x, int y, int mirror, int whiteTint)
        {
            screen.render(x, y, spritePixels[c, r].sheetPos, mirror, spritePixels[c, r].sheetNum, whiteTint); // render the sprite pixel.
        }

        public override string ToString()
        {
            StringBuilder @out = new(GetType().Name + "; pixels:");

            for (int y = 0; y < spritePixels.GetLength(1); y++)
            {
                for (int x = 0; x < spritePixels.GetLength(0); x++)
                {

                    @out.AppendLine();
                    @out.Append(spritePixels[x, y].ToString());
                }
            }

            @out.AppendLine();

            return @out.ToString();
        }
    }

    public struct Px
    {
        public int sheetPos, mirror, sheetNum;

        public Px(int sheetX, int sheetY, int mirroring)
            : this(sheetX, sheetY, mirroring, 0)
        {

        }

        public Px(int sheetX, int sheetY, int mirroring, int sheetNum)
        {
            sheetPos = sheetX + 32 * sheetY;
            mirror = mirroring;
            this.sheetNum = sheetNum;
        }

        public override string ToString()
        {
            return $"SpritePixel:x={sheetPos % 32};y={sheetPos / 32};mirror={mirror}";
        }
    }
}
