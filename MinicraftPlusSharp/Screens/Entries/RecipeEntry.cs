using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class RecipeEntry : ItemEntry
    {
        public static RecipeEntry[] UseRecipes(List<Recipe> recipes)
        {
            RecipeEntry[] entries = new RecipeEntry[recipes.Count];

            for (int i = 0; i < recipes.Count; i++)
            {
                entries[i] = new RecipeEntry(recipes[i]);
            }

            return entries;
        }

        private Recipe recipe;

        public RecipeEntry(Recipe r)
            : base(r.GetProduct())
        {
            this.recipe = r;
        }

        public override void Tick(InputHandler input) { }

        public override void Render(Screen screen, int x, int y, bool isSelected)
        {
            if (IsVisible())
            {
                Font.Draw(ToString(), screen, x, y, recipe.GetCanCraft() ? COL_SLCT : COL_UNSLCT);
                GetItem().sprite.Render(screen, x, y);
            }
        }

        public override string ToString()
        {
            return base.ToString() + (recipe.GetAmount() > 1 ? " x" + recipe.GetAmount() : "");
        }
    }
}
