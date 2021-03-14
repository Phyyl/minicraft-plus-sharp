using MinicraftPlusSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Items
{
    public class StackableItem : Item
    {
        internal static Item[] GetAllInstances()
        {
            List<Item> items = new List<Item>();

            items.Add(new StackableItem("Wood", new Sprite(1, 0, 0)));
            items.Add(new StackableItem("Stone", new Sprite(2, 0, 0)));
            items.Add(new StackableItem("Leather", new Sprite(8, 0, 0)));
            items.Add(new StackableItem("Wheat", new Sprite(6, 0, 0)));
            items.Add(new StackableItem("Key", new Sprite(0, 4, 0)));
            items.Add(new StackableItem("arrow", new Sprite(0, 2, 0)));
            items.Add(new StackableItem("string", new Sprite(1, 4, 0)));
            items.Add(new StackableItem("Coal", new Sprite(2, 4, 0)));
            items.Add(new StackableItem("Iron Ore", new Sprite(3, 4, 0)));
            items.Add(new StackableItem("Lapis", new Sprite(4, 4, 0)));
            items.Add(new StackableItem("Gold Ore", new Sprite(5, 4, 0)));
            items.Add(new StackableItem("Iron", new Sprite(6, 4, 0)));
            items.Add(new StackableItem("Gold", new Sprite(7, 4, 0)));
            items.Add(new StackableItem("Rose", new Sprite(5, 0, 0)));
            items.Add(new StackableItem("GunPowder", new Sprite(8, 4, 0)));
            items.Add(new StackableItem("Slime", new Sprite(9, 4, 0)));
            items.Add(new StackableItem("glass", new Sprite(10, 4, 0)));
            items.Add(new StackableItem("cloth", new Sprite(11, 4, 0)));
            items.Add(new StackableItem("gem", new Sprite(12, 4, 0)));
            items.Add(new StackableItem("Scale", new Sprite(13, 4, 0)));
            items.Add(new StackableItem("Shard", new Sprite(14, 4, 0)));

            return items.ToArray();
        }

        protected int count;

        protected StackableItem(string name, Sprite sprite)
            : base(name, sprite)
        {
            count = 1;
        }

        protected StackableItem(string name, Sprite sprite, int count)
            : this(name, sprite)
        {
            this.count = count;
        }

        public bool StacksWith(Item other)
        {
            return other is StackableItem && other.GetName().Equals(GetName());
        }

        public bool InteractOn(bool subClassSuccess)
        {
            if (subClassSuccess && !Game.IsMode("creative"))
            {
                count--;
            }

            return subClassSuccess;
        }

        public override bool IsDepleted()
        {
            return count <= 0;
        }

        public override Item Clone()
        {
            return new StackableItem(GetName(), sprite, count);
        }

        public override string ToString()
        {
            return $"{base.ToString()}-Stack_Size:{count}";
        }

        public override string GetData()
        {
            return $"{GetName()}_{count}";
        }

        public override string GetDisplayName()
        {
            string amt = (Math.Min(count, 999)) + " ";
            return $" {amt}{Localization.GetLocalized(GetName())}";
        }
    }
}
