using MinicraftPlusSharp.Java;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MinicraftPlusSharp.Items
{
    public class ToolType : JavaEnum<ToolType>
    {
        // if there's a second number, it specifies durability.
        public static readonly ToolType Shovel = new(0, 24); 
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

        private ToolType(int xPos, int dur, [CallerMemberName] string name = default)
            : base(name)
        {
            this.xPos = xPos;
            yPos = 13;
            durability = dur;
            noLevel = false;
        }

        private ToolType(int xPos, int dur, bool noLevel, [CallerMemberName] string name = default)
            : base(name)
        {
            yPos = 12;
            this.xPos = xPos;
            durability = dur;
            this.noLevel = noLevel;
        }
    }
}