using MinicraftPlusSharp.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class ItemListing : ItemEntry
    {
        private string info;

        public ItemListing(Item i, string text)
            : base(i)
        {
            SetSelectable(false);

            this.info = text;
        }

        public void SetText(string text)
        {
            info = text;
        }

        public override string ToString()
        {
            return " " + info;
        }
    }
}
