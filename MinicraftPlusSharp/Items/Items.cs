using MinicraftPlusSharp.Items.New;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Items
{
    public static class Items
    {
        private static List<Item> items = new();

        private static void Add(Item i)
        {
            items.Add(i);
        }

        private static void AddAll(params Item[] items)
        {
            Items.items.AddRange(items);
        }

        static Items()
        {
            Add(new PowerGloveItem());
            AddAll(FurnitureItem.GetAllInstances());
            AddAll(TorchItem.GetAllInstances());
            AddAll(BucketItem.GetAllInstances());
            AddAll(BookItem.GetAllInstances());
            AddAll(TileItem.GetAllInstances());
            AddAll(ToolItem.GetAllInstances());
            AddAll(FoodItem.GetAllInstances());
            AddAll(StackableItem.GetAllInstances());
            AddAll(ClothingItem.GetAllInstances());
            AddAll(ArmorItem.GetAllInstances());
            AddAll(PotionItem.GetAllInstances());
            AddAll(FishingRodItem.GetAllInstances());
        }

        public static Item Get(string name)
        {
            Item i = Get(name, false);

            if (i is null)
            {
                return new UnknownItem("NULL"); // technically shouldn't ever happen
            }

            return i;
        }

        public static Item Get(string name, bool allowNull)
        {
            name = name.ToUpper();
            //System.out.println("fetching name: \"" + name + "\"");
            int data = 1;
            bool hadUnderscore = false;
            if (name.Contains("_"))
            {
                hadUnderscore = true;
                try
                {
                    data = int.Parse(name.Substring(name.IndexOf("_") + 1));
                }
                catch (Exception ex)
                {
                    ex.PrintStackTrace();
                }

                name = name.Substring(0, name.IndexOf("_"));
            }
            else if (name.Contains(";"))
            {
                hadUnderscore = true;
                try
                {
                    data = int.Parse(name.Substring(name.IndexOf(";") + 1));
                }
                catch (Exception ex)
                {
                    ex.PrintStackTrace();
                }
                name = name.Substring(0, name.IndexOf(";"));
            }

            if (name.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
            {
                if (allowNull)
                {
                    return null;
                }
                else
                {
                    Console.WriteLine("WARNING: Items.get passed argument \"null\" when null is not allowed; returning UnknownItem. StackTrace:");
                    JavaThread.DumpStack();
                    return new UnknownItem("NULL");
                }
            }

            if (name.Equals("UNKNOWN"))
            {
                return new UnknownItem("BLANK");
            }

            Item i = null;
            foreach (var cur in items)
            {
                if (cur.GetName().Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    i = cur;
                    break;
                }
            }

            if (i != null)
            {
                i = i.Clone();
                
                if (i is StackableItem stackableItem)
                { 
                    stackableItem.count = data;
                }

                if (i is ToolItem toolItem && hadUnderscore)
                {
                    toolItem.dur = data;
                }
                
                return i;
            }
            else
            {
                Console.WriteLine(Network.onlinePrefix() + "ITEMS GET: Invalid name requested: \"" + name + "\"");
                JavaThread.DumpStack();
                return new UnknownItem(name);
            }
        }

        public static Item ArrowItem = Get("arrow");

        public static void FillCreativeInv(Inventory inv)
        {
            FillCreativeInv(inv, true);
        }

        public static void FillCreativeInv(Inventory inv, bool addAll)
        {
            foreach (var item in items)
            {
                if (item is PowerGloveItem)
                {
                    continue;
                }

                if (addAll || inv.Count(item) == 0)
                {
                    inv.Add(item.Clone());
                }
            }
        }
    }
}
