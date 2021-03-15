using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.Levels;
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
        public class Fill : JavaEnum<Fill>
        {
            public static readonly Fill Water = new(Tiles.Get("water"));
            public static readonly Fill Lava = new(Tiles.Get("lava"));
            public static readonly Fill Empty = new(Tiles.Get("hole"));

            public readonly Tile contained;

            private Fill(Tile contained, [CallerMemberName] string name = default)
                : base(name)
            {
                this.contained = contained;
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
            : base(fill.Name + " Bucket", new Sprite(fill.Ordinal, 6, 0), count)
        {
            this.filling = fill;
        }

        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
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

        public override int GetHashCode() { return base.GetHashCode() + filling * 31; }

        public override BucketItem Clone()
        {
            return new BucketItem(filling, count);
        }
    }
}
