using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class FurnitureItem : Item
    {
        protected static Item[] GetAllInstances()
        {
            List<Item> items = new();

            /// there should be a spawner for each level of mob, or at least make the level able to be changed.
            items.Add(new FurnitureItem(new Spawner(new Cow())));
            items.Add(new FurnitureItem(new Spawner(new Pig())));
            items.Add(new FurnitureItem(new Spawner(new Sheep())));
            items.Add(new FurnitureItem(new Spawner(new Slime(1))));
            items.Add(new FurnitureItem(new Spawner(new Zombie(1))));
            items.Add(new FurnitureItem(new Spawner(new Creeper(1))));
            items.Add(new FurnitureItem(new Spawner(new Skeleton(1))));
            items.Add(new FurnitureItem(new Spawner(new Snake(1))));
            items.Add(new FurnitureItem(new Spawner(new Knight(1))));
            items.Add(new FurnitureItem(new Spawner(new AirWizard(false))));

            items.Add(new FurnitureItem(new Chest()));
            items.Add(new FurnitureItem(new DungeonChest(false, true)));
            // add the various types of crafting furniture
            foreach (Crafter.Type type in Enum.GetValues<Crafter.Type>())
            {
                items.Add(new FurnitureItem(new Crafter(type)));
            }
            // add the various lanterns
            foreach (Lantern.Type type in Enum.GetValues<Lantern.Type>())
            {
                items.Add(new FurnitureItem(new Lantern(type)));
            }

            items.Add(new FurnitureItem(new Tnt()));
            items.Add(new FurnitureItem(new Bed()));

            return items.ToArray();
        }

        public Furniture furniture; // the furniture of this item
        public bool placed; // value if the furniture has been placed or not.

        private static int GetSpritePos(int fpos)
        {
            int x = fpos % 32;
            int y = fpos / 32;
            return (x - 8) / 2 + y * 32;
        }

        public FurnitureItem(Furniture furniture)
            : base(furniture.name, new Sprite(GetSpritePos(furniture.sprite.getPos()), 0))
        {
            this.furniture = furniture; // Assigns the furniture to the item
            placed = false;
        }

        /** Determines if you can attack enemies with furniture (you can't) */
        public override bool CanAttack()
        {
            return false;
        }

        /** What happens when you press the "Attack" key with the furniture in your hands */
        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            if (tile.MayPass(level, xt, yt, furniture))
            { // If the furniture can go on the tile
                Sound.place.Play();

                // Placed furniture's X and Y positions
                furniture.x = xt * 16 + 8;
                furniture.y = yt * 16 + 8;
                level.Add(furniture); // adds the furniture to the world
                if (Game.IsMode("creative"))
                    furniture = furniture.clone();
                else
                    placed = true; // the value becomes true, which removes it from the player's active item

                return true;
            }
            return false;
        }

        public override bool IsDepleted()
        {
            return placed;
        }

        public override FurnitureItem Clone()
        {
            return new FurnitureItem(furniture.clone());
        }
    }
}