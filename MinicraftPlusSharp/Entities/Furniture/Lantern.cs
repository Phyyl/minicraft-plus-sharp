using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class Lantern : Furniture
    {
        public class Type : JavaEnum<Type>
        {
            public static readonly Type NORM = new("Lantern", 9, 0);
		    public static readonly Type IRON = new("Iron Lantern", 12, 2);
		    public static readonly Type GOLD = new("Gold Lantern", 15, 4);

            public int light, offset;
            public string title;

            Type(string title, int light, int offset)
            {
                this.title = title;
                this.offset = offset;
                this.light = light;
            }
        }

        public Lantern.Type type;

        /**
         * Creates a lantern of a given type.
         * @param type Type of lantern.
         */
        public Lantern(Lantern.Type type)
            : base(type.title, new Sprite(18 + type.offset, 26, 2, 2, 2), 3, 2)
        {
            this.type = type;
        }

        public override Furniture Clone()
        {
            return new Lantern(type);
        }

        /** 
         * Gets the size of the radius for light underground (Bigger number, larger light) 
         */
        public override int GetLightRadius()
        {
            return type.light;
        }
    }
}
