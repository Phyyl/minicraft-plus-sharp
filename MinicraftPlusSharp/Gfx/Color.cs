using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Gfx
{
    public class Color
    {

        /* To explain this class, you have to know how Bit-Shifting works.
			David made a small post, so go here if you don't already know: http://minicraftforums.com/viewtopic.php?f=9&t=2256

			Note: this class still confuses me a bit, lol. -David

			On bit shifting: the new bits will always be zero, UNLESS it is a negative number aka the left-most bit is 1. Then, shifting right will fill with 1's, it seems.
		*/

        /*
			To provide a method to the madness, all methods ending in "Color" are copies of the methods of the same name without the "Color" part, except they convert the given value to 24bit Java RGB rather than the normal, minicraft 13-bit total RGB.
			Methods containing the word "get" deal with converting the minicraft rgb to something else, and unGet for the other way.

			Another point:

			rgbByte = int using 8 bits to store the r, g, and b.
			rgbInt = int using 24 bits to store r, g, and b, with 8 bits each; this is the classic 0-255 scale for each color component, compacted into one integer variable.
			rgb4Sprite = int using the 4 bytes in an int to store each of the 4 rgbBytes used for coloring a sprite.
			rgbReadable = int representing rgb in a readable decimal format, by having the values 0-5 in the 100s, 10s, and 1s place for r, g, and b respectively.
			rgbMinicraft = int using 12 bits for r, g, and b, 4 bits each, and one for transparency

			So, the methods ending in "Color" deal with rgbInts, while their counterparts deal with rgbBytes.
		*/

        public static readonly int TRANSPARENT = Color.Get(0, 0);
        public static readonly int WHITE = Color.Get(1, 255);
        public static readonly int GRAY = Color.Get(1, 153);
        public static readonly int DARK_GRAY = Color.Get(1, 51);
        public static readonly int BLACK = Color.Get(1, 0);
        public static readonly int RED = Color.Get(1, 255, 0, 0);
        public static readonly int GREEN = Color.Get(1, 0, 255, 0);
        public static readonly int BLUE = Color.Get(1, 0, 0, 255);
        public static readonly int YELLOW = Color.Get(1, 255, 255, 0);
        public static readonly int MAGENTA = Color.Get(1, 255, 0, 255);
        public static readonly int CYAN = Color.Get(1, 0, 255, 255);

        /** This returns a minicraftrgb.
		 * a should be between 0-1, r,g,and b should be 0-255 */
        public static int Get(int a, int r, int g, int b)
        {
            return (a << 24) + (r << 16) + (g << 8) + (b);
        }
        public static int Get(int a, int copy)
        {
            return Get(a, copy, copy, copy);
        }

        private static int Limit(int num, int min, int max)
        {
            if (num < min) num = min;
            if (num > max) num = max;
            return num;
        }

        // this makes an int that you would pass to the get(a,b,c,d), or get(d), method, from three separate 8-bit r,g,b values.
        public static int Rgb(int red, int green, int blue)
        { // rgbInt array -> rgbReadable
            red = Limit(red, 0, 250);
            green = Limit(green, 0, 250);
            blue = Limit(blue, 0, 250);

            return red / 50 * 100 + green / 50 * 10 + blue / 50; // this is: rgbReadable
        }

        /** This method darkens or lightens a color by the specified amount. */
        public static int Tint(int color, int amount, bool isSpriteCol)
        {
            if (isSpriteCol)
            {
                int[] rgbBytes = SeparateEncodedSprite(color); // this just separates the four 8-bit sprite colors; they are still in base-6 added form.
                for (int i = 0; i < rgbBytes.Length; i++)
                {
                    rgbBytes[i] = Tint(rgbBytes[i], amount);
                }
                return rgbBytes[0] << 24 | rgbBytes[1] << 16 | rgbBytes[2] << 8 | rgbBytes[3]; // this is: rgb4Sprite
            }
            else
            {
                return Tint(color, amount); // this is: rgbByte
            }
        }
        private static int Tint(int rgbByte, int amount)
        {
            if (rgbByte == 255) return 255; // see description of bit shifting above; it will hold the 255 value, not -1  

            int[] rgb = DecodeRGB(rgbByte); // this returns the rgb values as 0-5 numbers.
            for (int i = 0; i < rgb.Length; i++)
                rgb[i] = Limit(rgb[i] + amount, 0, 5);

            return rgb[0] * 36 + rgb[1] * 6 + rgb[2]; // this is: rgbByte
        }

        /** seperates a 4-part sprite color (rgb4Sprite) into it's original 4 component colors (which are each rgbBytes) */
        /// reverse of Color.get(a, b, c, d).
        public static int[] SeparateEncodedSprite(int rgb4Sprite) { return SeparateEncodedSprite(rgb4Sprite, false); }
        public static int[] SeparateEncodedSprite(int rgb4Sprite, bool convertToReadable)
        {
            // the numbers are stored, most to least shifted, as d, c, b, a.
            int a = (rgb4Sprite >> 24) & 0xFF; // See note at top; this is to remove left-hand 1's.
            int b = (rgb4Sprite & 0x00_FF_00_00) >> 16;
            int c = (rgb4Sprite & 0x00_00_FF_00) >> 8;
            int d = (rgb4Sprite & 0x00_00_00_FF);

            if (convertToReadable)
            {
                // they become rgbReadable
                a = UnGet(a);
                b = UnGet(b);
                c = UnGet(c);
                d = UnGet(d);
            } // else, they are rgbByte

            return new int[] { a, b, c, d };
        }

        /** This turns a 216 scale rgb int into a 0-5 scale "concatenated" rgb int. (aka rgbByte -> r/g/b Readables) */
        public static int[] DecodeRGB(int rgbByte)
        {
            int r = (rgbByte / 36) % 6;
            int g = (rgbByte / 6) % 6;
            int b = rgbByte % 6;
            return new int[] { r, g, b };
        }

        public static int UnGet(int rgbByte)
        { // rgbByte -> rgbReadable
            int[] cols = DecodeRGB(rgbByte);
            return cols[0] * 100 + cols[1] * 10 + cols[2];
        }

        /// this turns a 25-bit minicraft color into a 24-bit rgb color.
        public static int Upgrade(int rgbMinicraft)
        {

            return rgbMinicraft & 0xFF_FF_FF;
        }

        public static int TintColor(int rgbInt, int amount)
        {
            if (rgbInt < 0) return rgbInt; // this is "transparent".

            int[] comps = DecodeRGBColor(rgbInt);

            for (int i = 0; i < comps.Length; i++)
                comps[i] = Limit(comps[i] + amount, 0, 255);

            return comps[0] << 16 | comps[1] << 8 | comps[2];
        }

        public static int[] DecodeRGBColor(int rgbInt)
        {
            int r = (rgbInt & 0xFF_00_00) >> 16;
            int g = (rgbInt & 0x00_FF_00) >> 8;
            int b = (rgbInt & 0x00_00_FF);

            return new int[] { r, g, b };
        }

        /// for sprite colors
        public static string ToString(int col)
        {
            return string.Join(", ", Color.SeparateEncodedSprite(col, true));
        }
    }

}
