
using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels.Tiles;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class ClothingItem : StackableItem
    {
        internal new static List<Item> GetAllInstances()
        {
            List<Item> items = new();

            items.Add(new ClothingItem("Red Clothes", new Sprite(0, 10, 0), Color.Get(1, 204, 0, 0)));
            items.Add(new ClothingItem("Blue Clothes", new Sprite(1, 10, 0), Color.Get(1, 0, 0, 204)));
            items.Add(new ClothingItem("Green Clothes", new Sprite(2, 10, 0), Color.Get(1, 0, 204, 0)));
            items.Add(new ClothingItem("Yellow Clothes", new Sprite(3, 10, 0), Color.Get(1, 204, 204, 0)));
            items.Add(new ClothingItem("Black Clothes", new Sprite(4, 10, 0), Color.Get(1, 51)));
            items.Add(new ClothingItem("Orange Clothes", new Sprite(5, 10, 0), Color.Get(1, 255, 102, 0)));
            items.Add(new ClothingItem("Purple Clothes", new Sprite(6, 10, 0), Color.Get(1, 102, 0, 153)));
            items.Add(new ClothingItem("Cyan Clothes", new Sprite(7, 10, 0), Color.Get(1, 0, 102, 153)));
            items.Add(new ClothingItem("Reg Clothes", new Sprite(8, 10, 0), Color.Get(1, 51, 51, 0)));

            return items;
        }

        private int playerCol;

        private ClothingItem(string name, Sprite sprite, int pcol)
            : this(name, 1, sprite, pcol)
        {
        }

        private ClothingItem(string name, int count, Sprite sprite, int pcol)
            : base(name, sprite, count)
        {
            playerCol = pcol;
            this.sprite = sprite;
        }

        // put on clothes
        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            if (player.shirtColor == playerCol)
            {
                return false;
            }
            else
            {
                player.shirtColor = playerCol;
                if (Game.IsValidClient())
                    Game.client.SendShirtColor();
                return base.InteractOn(true);
            }
        }

        public override bool InteractsWithWorld() { return false; }

        public override ClothingItem Clone()
        {
            return new ClothingItem(GetName(), count, sprite, playerCol);
        }
    }

}