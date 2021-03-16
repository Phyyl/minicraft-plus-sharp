using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class DeathChest : Chest
    {
        private static Sprite normalSprite = new Sprite(10, 26, 2, 2, 2);
        private static Sprite redSprite = new Sprite(10, 24, 2, 2, 2);

        public int time; // time passed (used for death chest despawn)
        private int redtick = 0; // this is used to determine the shade of red when the chest is about to expire.
        private bool reverse; // what direction the red shade (redtick) is changing.

        /**
         * Creates a custom chest with the name Death Chest
         */
        public DeathChest()
         : base("Death Chest")
        {
            this.sprite = normalSprite;

            /// set the expiration time based on the world difficulty.
            if (Settings.Get("diff").Equals("Easy"))
            {
                time = 300 * Updater.normSpeed;
            }
            else if (Settings.Get("diff").Equals("Normal"))
            {
                time = 120 * Updater.normSpeed;
            }
            else if (Settings.Get("diff").Equals("Hard"))
            {
                time = 30 * Updater.normSpeed;
            }
        }

        public DeathChest(Player player)
            : this()
        {
            this.x = player.x;
            this.y = player.y;

            GetInventory().AddAll(player.GetInventory());
        }

        // for death chest time count, I imagine.
        public override void Tick()
        {
            base.Tick();
            //name = "Death Chest:"; // add the current

            if (GetInventory().InvSize() == 0)
            {
                Remove();
            }

            if (time < 30 * Updater.normSpeed)
            { // if there is less than 30 seconds left...
                redtick += reverse ? -1 : 1; // inc/dec-rement redtick, changing the red shading.

                /// these two statements keep the red color oscillating.
                if (redtick > 13)
                {
                    reverse = true;
                    this.sprite = normalSprite;
                }

                if (redtick < 0)
                {
                    reverse = false;
                    this.sprite = redSprite;
                }
            }

            if (time > 0)
            {
                time--; // decrement the time if it is not already zero.
            }

            if (time == 0)
            {
                Die(); // remove the death chest when the time expires, spilling all the contents.
            }
        }

        public override void Render(Screen screen)
        {
            base.Render(screen);

            string timeString = (time / Updater.normSpeed) + "S";

            Font.Draw(timeString, screen, x - Font.TextWidth(timeString) / 2, y - Font.TextHeight() - GetBounds().GetHeight() / 2, Color.WHITE);
        }

        public override bool Use(Player player)
        {
            return false;
        } // can't open it, just walk into it.

        public void Take(Player player)
        {
        } // can't grab a death chest.

        protected override void TouchedBy(Entity other)
        {
            if (other is Player player)
            {
                if (!Game.ISONLINE)
                {
                    player.GetInventory().AddAll(GetInventory());

                    Remove();

                    Game.notifications.Add("Death chest retrieved!");
                }
                else if (Game.IsValidClient())
                {
                    Game.client.TouchDeathChest(this);

                    Remove();
                }
            }
        }

        protected override string GetUpdateString()
        {
            string updates = base.GetUpdateString() + ";";

            updates += "time," + time;

            return updates;
        }

        protected override bool UpdateField(string field, string val)
        {
            if (base.UpdateField(field, val))
            {
                return true;
            }

            switch (field)
            {
                case "time":
                    time = int.Parse(val);
                    return true;
            }

            return false;
        }
    }

}
