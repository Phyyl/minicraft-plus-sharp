using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities
{
    public class Direction : JavaEnum<Direction>
    {
        public static readonly Direction NONE = new(0, 0);
        public static readonly Direction DOWN = new(0, 1);
        public static readonly Direction UP = new(0, -1);
        public static readonly Direction LEFT = new(-1, 0);
        public static readonly Direction RIGHT = new(1, 0);

        public readonly int x, y;

        private Direction(int x, int y, [CallerMemberName] string name = default)
            : base(default)
        {
            this.x = x;
            this.y = y;
        }

        public int GetX() { return x; }
        public int GetY() { return y; }

        public static Direction GetDirection(int xd, int yd)
        {
            if (xd == 0 && yd == 0)
            {
                return Direction.NONE; // the attack was from the same entity, probably; or at least the exact same space.
            }

            if (Math.Abs(xd) > Math.Abs(yd))
            {
                // the x distance is more prominent than the y distance
                if (xd < 0)
                {
                    return Direction.LEFT;
                }
                else
                {
                    return Direction.RIGHT;
                }
            }
            else
            {
                if (yd < 0)
                {
                    return Direction.UP;
                }
                else
                {
                    return Direction.DOWN;
                }
            }
        }

        public static Direction GetDirection(int dir)
        {
            return All[dir + 1];
        }

        public int GetDir()
        {
            return Ordinal - 1;
        }
    }
}
