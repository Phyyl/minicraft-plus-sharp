using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Gfx
{
    public class Point
    {
        public int x, y;

        public Point()
            : this(0, 0)
        {
        }
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point(Point model)
        {
            x = model.x;
            y = model.y;
        }

        public void Translate(int xoff, int yoff)
        {
            x += xoff;
            y += yoff;
        }

        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }

        public override bool Equals(object other)
        {
            if (other is not Point o)
            {
                return false;
            }

            return x == o.x && y == o.y;
        }

        public override int GetHashCode()
        {
            return x * 71 + y;
        }
    }

}
