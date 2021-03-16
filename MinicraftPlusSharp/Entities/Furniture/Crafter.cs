using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class Crafter : Furniture
    {
        public class Type : JavaEnum<Type>
        {
            public static readonly Type Workbench = new(new Sprite(16, 26, 2, 2, 2), 3, 2, Recipes.workbenchRecipes);
            public static readonly Type Oven = new(new Sprite(12, 26, 2, 2, 2), 3, 2, Recipes.ovenRecipes);
            public static readonly Type Furnace = new(new Sprite(14, 26, 2, 2, 2), 3, 2, Recipes.furnaceRecipes);
            public static readonly Type Anvil = new(new Sprite(8, 26, 2, 2, 2), 3, 2, Recipes.anvilRecipes);
            public static readonly Type Enchanter = new(new Sprite(24, 26, 2, 2, 2), 7, 2, Recipes.enchantRecipes);
            public static readonly Type Loom = new(new Sprite(26, 26, 2, 2, 2), 7, 2, Recipes.loomRecipes);

            public List<Recipe> recipes;

            public Sprite sprite;
            public int xr, yr;

            private Type(Sprite sprite, int xr, int yr, List<Recipe> list, [CallerMemberName] string name = default)
                : base(name)
            {
                this.sprite = sprite;
                this.xr = xr;
                this.yr = yr;

                recipes = list;

                Crafter.names.Add(this.Name);
            }
        }
        public static List<string> names = new();

        public Crafter.Type type;

        /**
         * Creates a crafter of a given type.
         * @param type What type of crafter this is.
         */
        public Crafter(Crafter.Type type)
            : base(type.Name, type.sprite, type.xr, type.yr)
        {
            this.type = type;
        }

        public override bool Use(Player player)
        {
            Game.SetMenu(new CraftingDisplay(type.recipes, type.Name, player));

            return true;
        }

        public override Furniture Clone()
        {
            return new Crafter(type);
        }

        public override string ToString()
        {
            return type.Name + GetDataPrints();
        }
    }
}
