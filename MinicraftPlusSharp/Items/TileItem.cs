using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinicraftPlusSharp.Items
{
    public class TileItem : StackableItem
    {
        internal new static Item[] GetAllInstances()
        {
            List<Item> items = new();

            /// TileItem sprites all have 1x1 sprites.
            items.Add(new TileItem("Flower", new Sprite(4, 0, 0), "flower", "grass"));
            items.Add(new TileItem("Acorn", new Sprite(7, 3, 0), "tree Sapling", "grass"));
            items.Add(new TileItem("Dirt", new Sprite(0, 0, 0), "dirt", "hole", "water", "lava"));
            items.Add(new TileItem("Natural Rock", new Sprite(2, 0, 0), "rock", "hole", "dirt", "sand", "grass", "path", "water", "lava"));

            items.Add(new TileItem("Plank", new Sprite(0, 5, 0), "Wood Planks", "hole", "water", "cloud"));
            items.Add(new TileItem("Plank Wall", new Sprite(1, 5, 0), "Wood Wall", "Wood Planks"));
            items.Add(new TileItem("Wood Door", new Sprite(2, 5, 0), "Wood Door", "Wood Planks"));
            items.Add(new TileItem("Stone Brick", new Sprite(3, 5, 0), "Stone Bricks", "hole", "water", "cloud", "lava"));
            items.Add(new TileItem("Stone Wall", new Sprite(4, 5, 0), "Stone Wall", "Stone Bricks"));
            items.Add(new TileItem("Stone Door", new Sprite(5, 5, 0), "Stone Door", "Stone Bricks"));
            items.Add(new TileItem("Obsidian Brick", new Sprite(6, 5, 0), "Obsidian", "hole", "water", "cloud", "lava"));
            items.Add(new TileItem("Obsidian Wall", new Sprite(7, 5, 0), "Obsidian Wall", "Obsidian"));
            items.Add(new TileItem("Obsidian Door", new Sprite(8, 5, 0), "Obsidian Door", "Obsidian"));

            items.Add(new TileItem("Wool", new Sprite(5, 3, 0), "Wool", "hole", "water"));
            items.Add(new TileItem("Red Wool", new Sprite(4, 3, 0), "Red Wool", "hole", "water"));
            items.Add(new TileItem("Blue Wool", new Sprite(3, 3, 0), "Blue Wool", "hole", "water"));
            items.Add(new TileItem("Green Wool", new Sprite(2, 3, 0), "Green Wool", "hole", "water"));
            items.Add(new TileItem("Yellow Wool", new Sprite(1, 3, 0), "Yellow Wool", "hole", "water"));
            items.Add(new TileItem("Black Wool", new Sprite(0, 3, 0), "Black Wool", "hole", "water"));

            items.Add(new TileItem("Sand", new Sprite(6, 3, 0), "sand", "hole", "water", "lava"));
            items.Add(new TileItem("Cactus", new Sprite(8, 3, 0), "cactus Sapling", "sand"));
            items.Add(new TileItem("Bone", new Sprite(9, 3, 0), "tree", "tree Sapling"));
            items.Add(new TileItem("Cloud", new Sprite(10, 3, 0), "cloud", "Infinite Fall"));

            items.Add(new TileItem("Wheat Seeds", new Sprite(3, 0, 0), "wheat", "farmland"));
            items.Add(new TileItem("Potato", new Sprite(18, 0, 0), "potato", "farmland"));
            items.Add(new TileItem("Grass Seeds", new Sprite(3, 0, 0), "grass", "dirt"));

            return items.ToArray();
        }

        public readonly string model;
        public readonly List<string> validTiles;

        protected TileItem(string name, Sprite sprite, string model, params string[] validTiles)
            : this(name, sprite, 1, model, validTiles.ToList())
        {
        }

        protected TileItem(string name, Sprite sprite, int count, string model, params string[] validTiles)
            : this(name, sprite, count, model, validTiles.ToList())
        {
        }

        protected TileItem(string name, Sprite sprite, int count, string model, List<string> validTiles)
            : base(name, sprite, count)
        {
            this.model = model.ToUpper();
            this.validTiles = new();

            foreach (string tile in validTiles)
            {
                this.validTiles.Add(tile.ToUpper());
            }
        }

        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            foreach (string tilename in validTiles)
            {
                if (tile.Matches(level.getData(xt, yt), tilename))
                {
                    level.setTile(xt, yt, model); // TODO maybe data should be part of the saved tile..?

                    Sound.place.play();

                    return base.InteractOn(true);
                }
            }

            if (Game.debug)
            {
                Console.WriteLine(model + " cannot be placed on " + tile.name);
            }

            string note = "";
            if (model.Contains("WALL"))
            {
                note = "Can only be placed on " + Tiles.GetName(validTiles[0]) + "!";
            }
            else if (model.Contains("DOOR"))
            {
                note = "Can only be placed on " + Tiles.GetName(validTiles[0]) + "!";
            }
            else if (model.Contains("BRICK") || model.Contains("PLANK"))
            {
                note = "Dig a hole first!";
            }

            if (note.Length > 0)
            {
                if (!Game.IsValidServer())
                {
                    Game.notifications.Add(note);
                }
                else
                {
                    Game.server.GetAssociatedThread((RemotePlayer)player).sendNotification(note, 0);
                }
            }

            return base.InteractOn(false);
        }

        public override bool Equals(Item other)
        {
            return base.Equals(other) && model.Equals(((TileItem)other).model);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() + model.GetHashCode();
        }

        public override TileItem Clone()
        {
            return new TileItem(GetName(), sprite, count, model, validTiles);
        }
    }
}