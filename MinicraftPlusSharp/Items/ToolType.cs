using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MinicraftPlusSharp.Items
{
    public class ToolType
    {
        private static readonly List<ToolType> all = new();

        public static ToolType[] All => all.ToArray();

        public static readonly ToolType Shovel = new(0, 24); // if there's a second number, it specifies durability.
        public static readonly ToolType Hoe = new(1, 20);
        public static readonly ToolType Sword = new(2, 42);
        public static readonly ToolType Pickaxe = new(3, 28);
        public static readonly ToolType Axe = new(4, 24);
        public static readonly ToolType Bow = new(5, 20);
        public static readonly ToolType Claymore = new(6, 34);
        public static readonly ToolType Shear = new(0, 42, true);

        public readonly int xPos; // X Position of origin
        public readonly int yPos; // Y position of origin
        public readonly int durability;
        public readonly bool noLevel;
        public readonly string name;

        private ToolType(int xPos, int dur, [CallerMemberName] string name = default)
        {
            this.xPos = xPos;
            yPos = 13;
            durability = dur;
            noLevel = false;
            this.name = name;

            all.Add(this);
        }

        private ToolType(int xPos, int dur, bool noLevel, [CallerMemberName] string name = default)
        {
            yPos = 12;
            this.xPos = xPos;
            durability = dur;
            this.noLevel = noLevel;
            this.name = name;

            all.Add(this);
        }
    }
}