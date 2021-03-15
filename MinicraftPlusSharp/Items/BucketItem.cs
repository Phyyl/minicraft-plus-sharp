using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Items
{
    public class BucketItem : StackableItem
    {
        public class Fill
        {
            private static readonly List<Fill> all = new();
            public static Fill[] All => all.ToArray();

            public static readonly Fill Empty = new(Tiles.Get("hole"), 2);
            public static readonly Fill Water = new(Tiles.Get("water"), 0);
            public static readonly Fill Lava = new(Tiles.Get("lava"), 1);

            public readonly Tile contained;
            public readonly int offset;
            public readonly string name;

            private Fill(Tile contained, int offset, [CallerMemberName] string name = default)
            {
                this.contained = contained;
                this.offset = offset;
                this.name = name;

                all.Add(this);
            }
        }

        internal new static Item[] GetAllInstances()
        {
            List<Item> items = new();

            foreach (Fill fill in Fill.All)
            {
                items.Add(new BucketItem(fill));
            }

            return items.ToArray();
        }

        private static Fill GetFilling(Tile tile)
        {
            foreach (Fill fill in Fill.All)
            {
                if (fill.contained.id == tile.id)
                {
                    return fill;
                }
            }

            return null;
        }

        private readonly Fill filling;

        private BucketItem(Fill fill)
            : this(fill, 1)
        {
        }

        private BucketItem(Fill fill, int count)
            : base(fill.name + " Bucket", new Sprite(fill.offset, 6, 0), count)
        {
            this.filling = fill;
        }

        public bool interactOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            Fill fill = GetFilling(tile);

            if (fill == null)
            {
                return false;
            }

            if (filling != Fill.Empty)
            {
                if (fill == Fill.Empty)
                {
                    level.SetTile(xt, yt, filling.contained);
                    if (!Game.IsMode("creative"))
                    {
                        player.activeItem = EditBucket(player, Fill.Empty);
                    }

                    return true;
                }
                else if (fill == Fill.Lava && filling == Fill.Water)
                {
                    level.SetTile(xt, yt, Tiles.Get("Obsidian"));
                    if (!Game.IsMode("creative"))
                    {
                        player.activeItem = EditBucket(player, Fill.Empty);
                    }

                    return true;
                }
            }
            else
            { // this is an empty bucket
                level.SetTile(xt, yt, Tiles.Get("hole"));
                if (!Game.IsMode("creative"))
                {
                    player.activeItem = EditBucket(player, fill);
                }

                return true;
            }

            return false;
        }

        /** This method exists due to the fact that buckets are stackable, but only one should be changed at one time. */
        private BucketItem EditBucket(Player player, Fill newFill)
        {
            if (count == 0)
            {
                return null; // this honestly should never happen...
            }

            if (count == 1)
            {
                return new BucketItem(newFill);
            }

            // this item object is a stack of buckets.
            count--;
            player.GetInventory().Add(new BucketItem(newFill));
            return this;
        }

        public override bool Equals(Item other)
        {
            return base.Equals(other) && filling == ((BucketItem)other).filling;
        }

        public override int GetHashCode() { return base.GetHashCode() + filling.offset * 31; }

        public override BucketItem Clone()
        {
            return new BucketItem(filling, count);
        }
    }
}
