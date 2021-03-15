using MinicraftPlusSharp.Core;
using System.Collections.Generic;

namespace MinicraftPlusSharp.Items
{
    public class Recipe
    {
        private Dictionary<string, int> costs = new();  // A list of costs for the recipe
        private string product; // the result item of the recipe
        private int amount;
        private bool canCraft; // checks if the player can craft the recipe

        public Recipe(string createdItem, params string[] reqItems)
        {
            canCraft = false;
            string[] sep = createdItem.Split("_");
            product = sep[0].ToUpper(); // assigns the result item
            amount = int.Parse(sep[1]);

            for (int i = 0; i < reqItems.Length; i++)
            {
                string[] curSep = reqItems[i].Split("_");
                string curItem = curSep[0].ToUpper(); // the current cost that's being added to costs.
                int amt = int.Parse(curSep[1]);
                bool added = false;
                foreach (string cost in costs.Keys)
                { // loop through the costs that have already been added
                    if (cost.Equals(curItem))
                    {
                        costs[cost] = costs[cost] + amt;
                        added = true;
                        break;
                    }
                }

                if (added)
                {
                    continue;
                }

                costs.Add(curItem, amt);
            }
        }

        public Item GetProduct()
        {
            return Items.Get(product);
        }

        public Dictionary<string, int> GetCosts()
        {
            return costs;
        }

        public int GetAmount()
        {
            return amount;
        }

        public bool GetCanCraft()
        {
            return canCraft;
        }

        public bool CheckCanCraft(Player player)
        {
            canCraft = getCanCraft(player);
            return canCraft;
        }

        /** Checks if the player can craft the recipe */
        private bool GetCanCraft(Player player)
        {
            if (Game.IsMode("creative"))
            {
                return true;
            }

            foreach (string cost in costs.Keys)
            { //cycles through the costs list
                /// this method ONLY WORKS if costs does not contain two elements such that inventory.count will count an item it contains as matching more than once.
                if (player.getInventory().count(Items.Get(cost)) < costs[cost])
                {
                    return false;
                }
            }

            return true;
        }

        // (WAS) abstract method given to the sub-recipe classes.
        public bool Craft(Player player)
        {
            if (!GetCanCraft(player))
            {
                return false;
            }

            if (!Game.IsMode("creative"))
            {
                // remove the cost items from the inventory.
                foreach (string cost in costs.Keys)
                {
                    player.GetInventory().RemoveItems(Items.Get(cost), costs[cost]);
                }
            }

            // add the crafted items.
            for (int i = 0; i < amount; i++)
            {
                player.getInventory().Add(GetProduct());
            }

            return true;
        }
    }
}