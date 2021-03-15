using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels.Tiles;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class TorchItem : TileItem
    {
        public new static Item[] GetAllInstances()
        {
            List<Item> items = new();

            items.Add(new TorchItem());

            return items.ToArray();
        }

        private TorchItem()
            : this(1)
        {
        }

        private TorchItem(int count)
            : base("Torch", new Sprite(11, 3, 0), count, "", "dirt", "Wood Planks", "Stone Bricks", "Obsidian", "Wool", "Red Wool", "Blue Wool", "Green Wool", "Yellow Wool", "Black Wool", "grass", "sand")
        {
        }

        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            if (validTiles.Contains(tile.name))
            {
                level.setTile(xt, yt, TorchTile.GetTorchTile(tile));

                return base.InteractOn(true);
            }

            return base.InteractOn(false);
        }

        public override bool Equals(Item other)
        {
            return other is TorchItem;
        }

        public override int GetHashCode()
        {
            return 8931;
        }

        public override TorchItem Clone()
        {
            return new TorchItem(count);
        }
    }
}