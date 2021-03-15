using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class FoodItem : StackableItem
    {
        internal new static Item[] GetAllInstances()
        {
            List<Item> items = new();

            items.Add(new FoodItem("Baked Potato", new Sprite(19, 0, 0), 1));
            items.Add(new FoodItem("Apple", new Sprite(16, 0, 0), 1));
            items.Add(new FoodItem("Raw Pork", new Sprite(10, 0, 0), 1));
            items.Add(new FoodItem("Raw Fish", new Sprite(14, 0, 0), 1));
            items.Add(new FoodItem("Raw Beef", new Sprite(12, 0, 0), 1));
            items.Add(new FoodItem("Bread", new Sprite(7, 0, 0), 2));
            items.Add(new FoodItem("Cooked Fish", new Sprite(15, 0, 0), 3));
            items.Add(new FoodItem("Cooked Pork", new Sprite(11, 0, 0), 3));
            items.Add(new FoodItem("Steak", new Sprite(13, 0, 0), 3));
            items.Add(new FoodItem("Gold Apple", new Sprite(17, 0, 0), 10));

            return items.ToArray();
        }

        private int feed; // the amount of hunger the food "satisfies" you by.
        private int staminaCost; // the amount of stamina it costs to consume the food.

        private FoodItem(string name, Sprite sprite, int feed)
            : this(name, sprite, 1, feed)
        {
        }

        private FoodItem(string name, Sprite sprite, int count, int feed)
            : base(name, sprite, count)
        {
            this.feed = feed;
            staminaCost = 5;
        }

        /** What happens when the player uses the item on a tile */
        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            bool success = false;

            if (count > 0 && player.hunger < Player.maxHunger && player.payStamina(staminaCost))
            { // if the player has hunger to fill, and stamina to pay...
                player.hunger = Math.Min(player.hunger + feed, Player.maxHunger); // restore the hunger
                success = true;
            }

            return base.InteractOn(success);
        }

        public override bool InteractsWithWorld() { return false; }

        public override FoodItem Clone()
        {
            return new FoodItem(GetName(), sprite, count, feed);
        }
    }
}