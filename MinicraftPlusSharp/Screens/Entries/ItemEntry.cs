using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class ItemEntry : ListEntry
    {
        public static ItemEntry[] UseItems(List<Item> items)
        {
            ItemEntry[] entries = new ItemEntry[items.Count];

            for (int i = 0; i < items.Count; i++)
            {
                entries[i] = new ItemEntry(items[i]);
            }

            return entries;
        }

        private Item item;

        public ItemEntry(Item i)
        {
            this.item = i;
        }

        public Item GetItem()
        {
            return item;
        }

        public override void Tick(InputHandler input)
        {
        }

        public override void Render(Screen screen, int x, int y, bool isSelected)
        {
            base.Render(screen, x, y, true);
            item.sprite.Render(screen, x, y);
        }

        // if you add to the length of the string, and therefore the width of the entry, then it will actually move the entry RIGHT in the inventory, instead of the intended left, because it is auto-positioned to the left side.
        public override string ToString()
        {
            return item.GetDisplayName();
        }
    }

}
