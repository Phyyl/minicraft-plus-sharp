using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Java;

namespace MinicraftPlusSharp.Screens
{
    public class RelPos : JavaEnum<RelPos>
    {
        public static readonly RelPos TOP_LEFT = new();
        public static readonly RelPos TOP = new();
        public static readonly RelPos TOP_RIGHT = new();
        public static readonly RelPos LEFT = new();
        public static readonly RelPos CENTER = new();
        public static readonly RelPos RIGHT = new();
        public static readonly RelPos BOTTOM_LEFT = new();
        public static readonly RelPos BOTTOM = new();
        public static readonly RelPos BOTTOM_RIGHT = new();

        public int xIndex, yIndex;

        static RelPos()
        {
            foreach (RelPos rp in All)
            {
                int ord = rp.Ordinal;
                rp.xIndex = ord % 3;
                rp.yIndex = ord / 3;
            }
        }

        private RelPos([CallerMemberName] string name = null) : base(name)
        {
        }

        public static RelPos GetPos(int xIndex, int yIndex)
        {
            return All[Math.Clamp(xIndex, 0, 2) + Math.Clamp(yIndex, 0, 2) * 3];
        }

        public RelPos GetOpposite()
        {
            int nx = -(xIndex - 1) + 1;
            int ny = -(yIndex - 1) + 1;

            return GetPos(nx, ny);
        }

        /** positions the given rect around the given anchor. The double size is what aligns it to a point rather than a rect. */
        public Point PositionRect(Dimension rectSize, Point anchor)
        {
            Rectangle bounds = new Rectangle(anchor.x, anchor.y, rectSize.width * 2, rectSize.height * 2, Rectangle.CENTER_DIMS);
            return PositionRect(rectSize, bounds);
        }
        // the point is returned as a rectangle with the given dimension and the found location, within the provided dummy rectangle.
        public Rectangle PositionRect(Dimension rectSize, Point anchor, Rectangle dummy)
        {
            Point pos = PositionRect(rectSize, anchor);
            dummy.SetSize(rectSize, RelPos.TOP_LEFT);
            dummy.SetPosition(pos, RelPos.TOP_LEFT);
            return dummy;
        }

        /** positions the given rect to a relative position in the container. */
        public Point PositionRect(Dimension rectSize, Rectangle container)
        {
            Point tlcorner = container.GetCenter();

            // this moves the inner box correctly
            tlcorner.x += ((xIndex - 1) * container.GetWidth() / 2) - (xIndex * rectSize.width / 2);
            tlcorner.y += ((yIndex - 1) * container.GetHeight() / 2) - (yIndex * rectSize.height / 2);

            return tlcorner;
        }

        // the point is returned as a rectangle with the given dimension and the found location, within the provided dummy rectangle.
        public Rectangle PositionRect(Dimension rectSize, Rectangle container, Rectangle dummy)
        {
            Point pos = PositionRect(rectSize, container);
            dummy.SetSize(rectSize, RelPos.TOP_LEFT);
            dummy.SetPosition(pos, RelPos.TOP_LEFT);
            return dummy;
        }
    }
}
