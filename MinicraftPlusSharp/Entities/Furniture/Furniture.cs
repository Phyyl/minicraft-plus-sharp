using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class Furniture : Entity
    {
        protected int pushTime = 0, multiPushTime = 0; // time for each push; multi is for multiplayer, to make it so not so many updates are sent.
        private Direction pushDir = Direction.NONE; // the direction to push the furniture
        public Sprite sprite;
        public string name;

        /**
         * Constructor for the furniture entity.
         * Size will be set to 3.
         * @param name Name of the furniture.
         * @param sprite Furniture sprite.
         */
        public Furniture(string name, Sprite sprite)
            : this(name, sprite, 3, 3)
        {
        }

        /**
         * Constructor for the furniture entity.
         * Radius is only used for collision detection.
         * @param name Name of the furniture.
         * @param sprite Furniture sprite.
         * @param xr Horizontal radius.
         * @param yr Vertical radius.
         */
        public Furniture(string name, Sprite sprite, int xr, int yr)
            : base(xr, yr)
        {
            // all of these are 2x2 on the spritesheet; radius is for collisions only.
            this.name = name;
            this.sprite = sprite;
            col = sprite.color;
        }

        public virtual Furniture Clone()
        {
            try
            {
                return (Furniture)Activator.CreateInstance(GetType());
            }
            catch (Exception ex)
            {
                ex.PrintStackTrace();
            }

            return new Furniture(name, sprite);
        }

        public override void Tick()
        {
            // moves the furniture in the correct direction.
            Move(pushDir.GetX(), pushDir.GetY());

            pushDir = Direction.NONE;

            if (pushTime > 0)
            {
                pushTime--; // update pushTime by subtracting 1.
            }
            else
            {
                multiPushTime = 0;
            }
        }

        /** Draws the furniture on the screen. */
        public override void Render(Screen screen)
        {
            sprite.Render(screen, x - 8, y - 8);
        }

        /** Called when the player presses the MENU key in front of this. */
        public virtual bool Use(Player player)
        {
            return false;
        }

        public override bool Blocks(Entity e)
        {
            return true; // furniture blocks all entities, even non-solid ones like arrows.
        }

        protected override void TouchedBy(Entity entity)
        {
            if (entity is Player player)
            {
                TryPush(player);
            }
        }

        /**
         * Used in PowerGloveItem.java to let the user pick up furniture.
         * @param player The player picking up the furniture.
         */
        public override bool Interact(Player player, Item item, Direction attackDir)
        {
            if (item is PowerGloveItem)
            {
                Sound.monsterHurt.play();

                if (!Game.ISONLINE)
                {
                    Remove();

                    if (!Game.IsMode("creative") && player.activeItem != null && !(player.activeItem is PowerGloveItem))
                    {
                        player.GetInventory().Add(0, player.activeItem); // put whatever item the player is holding into their inventory
                    }

                    player.activeItem = new FurnitureItem(this); // make this the player's current item.

                    return true;
                }
                else if (Game.IsValidServer() && player is RemotePlayer remotePlayer)
                {
                    Remove();

                    Game.server.GetAssociatedThread(remotePlayer).UpdatePlayerActiveItem(new FurnitureItem(this));

                    return true;
                }
                else
                {
                    Console.WriteLine("WARNING: undefined behavior; online game was not server and ticked furniture: " + this + "; and/or player in online game found that isn't a RemotePlayer: " + player);
                }
            }

            return false;
        }

        /**
         * Tries to let the player push this furniture.
         * @param player The player doing the pushing.
         */
        public void TryPush(Player player)
        {
            if (pushTime == 0)
            {
                pushDir = player.dir; // set pushDir to the player's dir.
                pushTime = multiPushTime = 10; // set pushTime to 10.

                if (Game.IsConnectedClient())
                {
                    Game.client.pushFurniture(this);
                }
            }
        }

        public override bool CanWool()
        {
            return true;
        }

        protected override string GetUpdateString()
        {
            return base.GetUpdateString() + ";pushTime," + multiPushTime;
        }

        protected override bool UpdateField(string field, string val)
        {
            if (base.UpdateField(field, val))
            {
                return true;
            }

            switch (field)
            {
                case "pushTime":
                    pushTime = int.Parse(val);
                    return true;
            }

            return false;
        }
    }

}
