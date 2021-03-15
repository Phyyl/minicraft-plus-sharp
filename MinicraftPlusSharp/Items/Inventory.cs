using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinicraftPlusSharp.Items
{
    public class Inventory
    {
        private readonly JavaRandom random = new();
        private readonly List<Item> items = new(); // the list of items that is in the inventory.

        /**
         * Returns all the items which are in this inventory.
         * @return List containing all the items in the inventory.
         */
        public List<Item> GetItems()
        {
            return new List<Item>(items);
        }

        public void ClearInv() { items.Clear(); }

        public int InvSize() { return items.Count; }

        /**
         * Get one item in this inventory.
         * @param idx The index of the item in the inventory's item array.
         * @return The specified item.
         */
        public Item Get(int idx) { return items[idx]; }

        /**
         * Remove an item in this inventory.
         * @param idx The index of the item in the inventory's item array.
         * @return The removed item.
         */
        public virtual Item Remove(int idx)
        {
            Item item = items[idx];
            items.RemoveAt(idx);
            return item;
        }

        public void AddAll(Inventory other)
        {
            foreach (Item i in other.GetItems())
            {
                Add(i.Clone());
            }
        }

        /** Adds an item to the inventory */
        public void Add(Item item)
        {
            if (item != null)
            {
                Add(items.Count, item);  // adds the item to the end of the inventory list
            }
        }

        /**
         * Adds several copies of the same item to the end of the inventory.
         * @param item Item to be added.
         * @param num Amount of items to add.
         */
        public void Add(Item item, int num)
        {
            for (int i = 0; i < num; i++)
            {
                Add(item.Clone());
            }
        }

        /**
         * Adds an item to a specific spot in the inventory.
         * @param slot Index to place item at.
         * @param item Item to be added.
         */
        public virtual void Add(int slot, Item item)
        {

            // Do not add to inventory if it is a PowerGlove
            if (item is PowerGloveItem)
            {
                Console.WriteLine("WARNING: tried to add power glove to inventory. stack trace:");
                JavaThread.DumpStack();
                return;
            }

            if (item is StackableItem toTake)
            { // if the item is a item...
                bool added = false;

                foreach (Item value in items)
                {
                    if (toTake.StacksWith(value))
                    {
                        // matching implies that the other item is stackable, too.
                        ((StackableItem)value).count += toTake.count;
                        added = true;
                        break;
                    }
                }

                if (!added)
                {
                    items.Insert(slot, toTake);
                }
            }
            else
            {
                items.Insert(slot, item); // add the item to the items list
            }
        }

        /** Removes items from your inventory; looks for stacks, and removes from each until reached count. returns amount removed. */
        private int RemoveFromStack(StackableItem given, int count)
        {
            int removed = 0; // to keep track of amount removed.
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is not StackableItem curItem)
                {
                    continue;
                }

                if (!curItem.StacksWith(given))
                {
                    continue; // can't do equals, becuase that includes the stack size.
                }

                // equals; and current item is stackable.
                int amountRemoving = Math.Min(count - removed, curItem.count); // this is the number of items that are being removed from the stack this run-through.
                curItem.count -= amountRemoving;

                if (curItem.count == 0)
                { // remove the item from the inventory if its stack is empty.
                    Remove(i);
                    i--;
                }

                removed += amountRemoving;

                if (removed == count)
                {
                    break;
                }

                if (removed > count)
                { // just in case...
                    Console.WriteLine("SCREW UP while removing items from stack: " + (removed - count) + " too many.");
                    break;
                }
                // if not all have been removed, look for another stack.
            }

            if (removed < count)
            {
                Console.WriteLine("Inventory: could not remove all items; " + (count - removed) + " left.");
            }

            return removed;
        }

        /** 
         * Removes the item from the inventory entirely, whether it's a stack, or a lone item.
         */
        public void RemoveItem(Item i)
        {
            //if (Game.debug) System.out.println("original item: " + i);
            if (i is StackableItem stackable)
            {
                RemoveItem(i.Clone(), stackable.count);
            }
            else
            {
                RemoveItem(i.Clone(), 1);
            }
        }

        /**
         * Removes items from this inventory. Note, if passed a stackable item, this will only remove a max of count from the stack.
         * @param given Item to remove.
         * @param count Max amount of the item to remove.
         */
        public void RemoveItem(Item given, int count)
        {
            if (given is StackableItem stackable)
            {
                count -= RemoveFromStack(stackable, count);
            }
            else
            {
                for (int i = 0; i < items.Count; i++)
                {
                    Item curItem = items[i];

                    if (curItem.Equals(given))
                    {
                        Remove(i);
                        count--;
                        if (count == 0) break;
                    }
                }
            }

            if (count > 0)
            {
                Console.WriteLine("WARNING: could not remove " + count + " " + given + (count > 1 ? "s" : "") + " from inventory");
            }
        }

        /** Returns the how many of an item you have in the inventory. */
        public int Count(Item given)
        {
            if (given == null)
            {
                return 0; // null requests get no items. :)
            }

            int found = 0; // initialize counting var
                           // assign current item
            foreach (Item curItem in items)
            { // loop though items in inv
              // if the item can be a stack...
                if (curItem is StackableItem stackable && stackable.StacksWith(given))
                {
                    found += ((StackableItem)curItem).count; // add however many items are in the stack.
                }
                else if (curItem.Equals(given))
                {
                    found++; // otherwise, just add 1 to the found count.
                }
            }

            return found;
        }

        /**
         * Generates a string representation of all the items in the inventory which can be sent
         * over the network.
         * @return string representation of all the items in the inventory.
         */
        public string GetItemData()
        {
            StringBuilder itemdata = new();
            foreach (Item i in items)
            {
                itemdata.Append(i.GetData()).Append(":");
            }

            if (itemdata.Length > 0)
            {
                itemdata = new StringBuilder(itemdata.ToString(0, itemdata.Length - 1)); //remove extra ",".
            }

            return itemdata.ToString();
        }

        /**
         * Replaces all the items in the inventory with the items in the string.
         * @param items string representation of an inventory.
         */
        public void UpdateInv(string items)
        {
            ClearInv();

            if (items.Length == 0)
            {
                return; // there are no items to add.
            }

            foreach (string item in items.Split(":")) // this still generates a 1-item array when "items" is blank... [""].
            {
                Add(Items.Get(item));
            }
        }

        /**
         * Tries to add an item to the inventory.
         * @param chance Chance for the item to be added.
         * @param item Item to be added.
         * @param num How many of the item.
         * @param allOrNothing if true, either all items will be added or none, if false its possible to add
         * between 0-num items.
         */
        public void TryAdd(int chance, Item item, int num, bool allOrNothing)
        {
            if (!allOrNothing || random.nextInt(chance) == 0)
            {
                for (int i = 0; i < num; i++)
                {
                    if (allOrNothing || random.nextInt(chance) == 0)
                    {
                        Add(item.Clone());
                    }
                }
            }
        }

        public void TryAdd(int chance, Item item, int num)
        {
            if (item == null)
            {
                return;
            }

            if (item is StackableItem stackable)
            {
                stackable.count *= num;
                TryAdd(chance, item, 1, true);
            }
            else
            {
                TryAdd(chance, item, num, false);
            }
        }

        public void TryAdd(int chance, Item item)
        {
            TryAdd(chance, item, 1);
        }
        
        public void TryAdd(int chance, ToolType type, int lvl)
        {
            TryAdd(chance, new ToolItem(type, lvl));
        }

        /**
         * Tries to add an Furniture to the inventory.
         * @param chance Chance for the item to be added.
         * @param type Type of furniture to add.
         */
        public void TryAdd(int chance, Furniture type)
        {
            TryAdd(chance, new FurnitureItem(type));
        }
    }
}