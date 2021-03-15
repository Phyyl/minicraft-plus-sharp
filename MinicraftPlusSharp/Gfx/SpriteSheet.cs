using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Gfx
{
    public class SpriteSheet
    {
        public static readonly int boxWidth = 8;

        public int width, height; // width and height of the sprite sheet
        public int[] pixels; // integer array of the image's pixels

        public unsafe SpriteSheet(Bitmap image)
        {
            //sets width and height to that of the image
            width = image.Width;
            height = image.Height;

            BitmapData data = image.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Span<Color> colors = new(data.Scan0.ToPointer(), width * height * 4);
            pixels = new int[colors.Length];

            for (int i = 0; i < pixels.Length; i++)
            { // loops through all the pixels

                Color color = colors[i];
                int red = color.R << 16;
                int green = color.G << 8;
                int blue = color.B;
                int transparent = color.A == 0 ? 0 : 1;

                // actually put the data in the array
                // uses 25 bits to store everything (8 for red, 8 for green, 8 for blue, and 1 for alpha)
                pixels[i] = (transparent << 24) + red + green + blue;
            }
        }
    }

}
