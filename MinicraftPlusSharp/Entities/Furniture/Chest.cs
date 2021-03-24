using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.SaveLoad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class Chest : Furniture, ItemHolder
    {
        private Inventory inventory; // Inventory of the chest

        public Chest()
            : this("Chest")
        {
        }

        /**
         * Creates a chest with a custom name.
         * @param name Name of chest.
         */
        public Chest(string name)
            : base(name, new Sprite(10, 26, 2, 2, 2), 3, 3) // Name of the chest
        {
            inventory = new Inventory(); // initialize the inventory.
        }

        /** This is what occurs when the player uses the "Menu" command near this */
        public override bool Use(Player player)
        {
            Game.SetMenu(new ContainerDisplay(player, this));

            return true;
        }

        public void PopulateInvRandom(string lootTable, int depth)
        {
            try
            {
                string[] lines = Load.LoadFile("/resources/chestloot/" + lootTable + ".txt");

                foreach (string line in lines)
                {
                    //Console.WriteLine(line);
                    string[] data = line.Split(",");

                    if (!line.StartsWith(":"))
                    {
                        inventory.TryAdd(int.Parse(data[0]), Items.Items.Get(data[1]), data.Length < 3 ? 1 : int.Parse(data[2]));
                    }
                    else if (inventory.InvSize() == 0)
                    {
                        // adds the "fallback" items to ensure there's some stuff
                        string[] fallbacks = line[1..].Split(":");

                        foreach (string item in fallbacks)
                        {
                            inventory.Add(Items.Items.Get(item.Split(",")[0]), int.Parse(item.Split(",")[1]));
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Couldn't read loot table \"" + lootTable + ".txt" + "\"");
                e.PrintStackTrace();
            }
        }

        public override bool Interact(Player player, Item item, Direction attackDir)
        {
            if (inventory.InvSize() == 0)
            {
                return base.Interact(player, item, attackDir);
            }

            return false;
        }

        protected override string GetUpdateString()
        {
            string updates = base.GetUpdateString() + ";";
            updates += "inventory," + inventory.GetItemData();
            return updates;
        }

        protected override bool UpdateField(string fieldName, string val)
        {
            if (base.UpdateField(fieldName, val))
            {
                return true;
            }

            switch (fieldName)
            {
                case "inventory":
                    inventory.UpdateInv(val);
                    
                    if (Game.GetMenu() is ContainerDisplay containerMenu)
                    {
                        containerMenu.onInvUpdate(this);
                    }

                    return true;
            }

            return false;
        }

        public Inventory GetInventory()
        {
            return inventory;
        }

        public override void Die()
        {
            if (level != null)
            {
                List<Item> items = inventory.GetItems();

                level.DropItem(x, y, items.ToArray());
            }

            base.Die();
        }
    }
}
