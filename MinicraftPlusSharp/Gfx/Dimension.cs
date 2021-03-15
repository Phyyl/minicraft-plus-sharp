using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Gfx
{
    public class Dimension
    {
        public int width, height;

        public Dimension()
            : this(0, 0)
        {
        }

        public Dimension(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public Dimension(Dimension model)
        {
            width = model.width;
            height = model.height;
        }

        public override string ToString()
        {
            return width + "x" + height;
        }
    }
}
