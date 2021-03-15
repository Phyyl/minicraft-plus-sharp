using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.Levels.Tiles;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class FishingRodItem : Item
    {
        protected static Item[] GetAllInstances()
        {
            List<Item> items = new();

            for (int i = 0; i < 4; i++)
            {
                items.Add(new FishingRodItem(i));
            }

            return items.ToArray();
        }

        private int uses = 0; // the more uses, the higher the chance of breaking
        public int level; // the higher the level the lower the chance of breaking

        private JavaRandom random = new();

        /* these numbers are a bit confusing, so here's an explanation
        * if you want to know the percent chance of a category (let's say tool, which is third)
        * you have to subtract 1 + the "tool" number from the number before it (for the first number subtract from 100)*/
        private static readonly int[,] LEVEL_CHANCES = {
            {44, 14, 9, 4}, // they're in the order "fish", "junk", "tools", "rare"
            {24, 14, 9, 4}, // iron has very high chance of fish
            {59, 49, 9, 4}, // gold has very high chance of tools
            {79, 69, 59, 4} // gem has very high chance of rare items
        };

        private static readonly string[] LEVEL_NAMES = {
            "Wood",
            "Iron",
            "Gold",
            "Gem"
        };

        public FishingRodItem(int level)
            : base(LEVEL_NAMES[level] + " Fishing Rod", new Sprite(level, 11, 0))
        {
            this.level = level;
        }

        public static int GetChance(int idx, int level)
        {
            return LEVEL_CHANCES[idx, level];
        }

        
        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            if (tile == Tiles.Get("water") && !player.isSwimming())
            { // make sure not to use it if swimming
                uses++;
                player.isFishing = true;
                player.fishingLevel = this.level;
                return true;
            }

            return false;
        }

        public override bool CanAttack() { return false; }

        public override bool IsDepleted()
        {
            if (random.NextInt(100) > 120 - uses + level * 6)
            { // breaking is random, the lower the level, and the more times you use it, the higher the chance
                Game.notifications.Add("Your Fishing rod broke.");
                return true;
            }
            return false;
        }

        public override Item Clone()
        {
            FishingRodItem item = new(level);
            item.uses = uses;
            return item;
        }
    }
}