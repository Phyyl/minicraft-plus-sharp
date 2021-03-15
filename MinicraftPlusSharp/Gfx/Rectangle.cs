using MinicraftPlusSharp.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Gfx
{
    public class Rectangle
    {
        public static readonly int CORNER_DIMS = 0;
        public static readonly int CORNERS = 1;
        public static readonly int CENTER_DIMS = 2;

        private int x, y, w, h;

        public Rectangle()
        {
        } // 0 all.

        public Rectangle(int x, int y, int x1, int y1, int type)
        {
            if (type < 0 || type > 2)
            {
                type = 0;
            }

            if (type != CENTER_DIMS)
            { // x and y are the coords of the top left corner.
                this.x = x;
                this.y = y;
            }
            else
            { // x and y are the coords of the center.
                this.x = x - x1 / 2;
                this.y = y - y1 / 2;
            }

            if (type != CORNERS)
            { // x1 and y1 are the width and height.
                this.w = x1;
                this.h = y1;
            }
            else
            { // x1 and y1 are the coords of the bottom right corner.
                this.w = x1 - x;
                this.h = y1 - y;
            }
        }

        public Rectangle(Point p, Dimension d)
            : this(false, p, d)
        {
        }
        public Rectangle(bool isCenter, Point p, Dimension d)
            : this(p.x, p.y, d.width, d.height, isCenter ? CENTER_DIMS : CORNER_DIMS)
        {
        }

        public Rectangle(Rectangle model)
        {
            x = model.x;
            y = model.y;
            w = model.w;
            h = model.h;
        }

        public int GetLeft()
        {
            return x;
        }
        public int GetRight()
        {
            return x + w;
        }
        public int GetTop()
        {
            return y;
        }
        public int GetBottom()
        {
            return y + h;
        }

        public int GetWidth()
        {
            return w;
        }
        public int GetHeight()
        {
            return h;
        }

        public Point GetCenter()
        {
            return new Point(x + w / 2, y + h / 2);
        }
        public Dimension GetSize()
        {
            return new Dimension(w, h);
        }

        public Point GetPosition(RelPos relPos)
        {
            Point p = new Point(x, y);
            p.x += relPos.xIndex * w / 2;
            p.y += relPos.yIndex * h / 2;
            return p;
        }

        public bool Intersects(Rectangle other)
        {
            return !(GetLeft() > other.GetRight() // left side is past the other right side
              || other.GetLeft() > GetRight() // other left side is past the right side
              || GetBottom() < other.GetTop() // other top is below the bottom
              || other.GetBottom() < GetTop() // top is below the other bottom
            );
        }

        public void SetPosition(Point p, RelPos relPos)
        {
            SetPosition(p.x, p.y, relPos);
        }
        public void SetPosition(int x, int y, RelPos relPos)
        {
            this.x = x - relPos.xIndex * w / 2;
            this.y = y - relPos.yIndex * h / 2;
        }

        public void Translate(int xoff, int yoff)
        {
            x += xoff;
            y += yoff;
        }

        public void SetSize(Dimension d, RelPos anchor)
        {
            SetSize(d.width, d.height, anchor);
        }
        public void SetSize(int width, int height, RelPos anchor)
        {
            Point p = GetPosition(anchor);
            this.w = width;
            this.h = height;
            SetPosition(p, anchor);
        }

        public override string ToString()
        {
            return base.ToString() + "[center=" + GetCenter() + "; size=" + GetSize() + "]";
        }
    }
}
