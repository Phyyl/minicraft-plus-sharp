using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class DungeonChest : Chest
    {
        private static readonly Sprite openSprite = new Sprite(14, 24, 2, 2, 2);
        private static readonly Sprite lockSprite = new Sprite(12, 24, 2, 2, 2);

        private bool isLocked;

        /**
         * Creates a custom chest with the name Dungeon Chest.
         * @param populateInv
         */
        public DungeonChest(bool populateInv)
            : this(populateInv, false)
        {
        }

        public DungeonChest(bool populateInv, bool unlocked)
            : base("Dungeon Chest")
        {
            if (populateInv)
            {
                PopulateInv();
            }

            SetLocked(!unlocked);
        }

        public override Furniture Clone()
        {
            return new DungeonChest(false, !this.isLocked);
        }

        public override bool Use(Player player)
        {
            if (isLocked)
            {
                bool activeKey = player.activeItem != null && player.activeItem.Equals(Items.Items.Get("Key"));
                bool invKey = player.GetInventory().Count(Items.Items.Get("key")) > 0;

                if (activeKey || invKey)
                { // if the player has a key...
                    if (!Game.IsMode("creative"))
                    { // remove the key unless on creative mode.
                        if (activeKey)
                        { // remove activeItem
                            StackableItem key = (StackableItem)player.activeItem;
                            key.count--;
                        }
                        else
                        { // remove from inv
                            player.GetInventory().RemoveItem(Items.Items.Get("key"));
                        }
                    }

                    isLocked = false;
                    this.sprite = openSprite; // set to the unlocked color

                    level.Add(new SmashParticle(x * 16, y * 16));
                    level.Add(new TextParticle("-1 key", x, y, Color.RED));
                    level.chestCount--;

                    if (level.chestCount == 0)
                    { // if this was the last chest...
                        level.DropItem(x, y, 5, Items.Items.Get("Gold Apple"));

                        Updater.notifyAll("You hear a noise from the surface!", -100); // notify the player of the developments
                                                                                       // add a level 2 airwizard to the middle surface level.
                        AirWizard wizard = new AirWizard(true);
                        wizard.x = World.levels[World.LvlIdx(0)].w / 2;
                        wizard.y = World.levels[World.LvlIdx(0)].h / 2;
                        World.levels[World.LvlIdx(0)].add(wizard);
                    }

                    return base.Use(player); // the player unlocked the chest.
                }

                return false; // the chest is locked, and the player has no key.
            }
            else
            {
                return base.Use(player); // the chest was already unlocked.
            }
        }

        /**
         * Populate the inventory of the DungeonChest using the loot table system
         */
        private void PopulateInv()
        {
            Inventory inv = GetInventory(); // Yes, I'm that lazy. ;P
            inv.ClearInv(); // clear the inventory.

            PopulateInvRandom("dungeonchest", 0);
        }

        public bool IsLocked()
        {
            return isLocked;
        }

        public void SetLocked(bool locked)
        {
            this.isLocked = locked;

            // auto update sprite
            sprite = locked ? DungeonChest.lockSprite : DungeonChest.openSprite;
        }

        /** what happens if the player tries to push a Dungeon Chest. */
        protected override void TouchedBy(Entity entity)
        {
            if (!isLocked) // can only be pushed if unlocked.
            {
                base.TouchedBy(entity);
            }
        }

        public override bool Interact(Player player, Item item, Direction attackDir)
        {
            if (!isLocked)
            {
                return base.Interact(player, item, attackDir);
            }

            return false;
        }

        protected override string GetUpdateString()
        {
            string updates = base.GetUpdateString() + ";";
            updates += "isLocked," + isLocked;

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
                case "isLocked":
                    isLocked = bool.Parse(val);
                    return true;
            }

            return false;
        }
    }
}
