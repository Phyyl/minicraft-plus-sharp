using MinicraftPlusSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Items
{
    public class BookItem : Item
    {
        internal static Item[] GetAllInstances()
        {
            List<Item> items = new List<Item>();
            items.Add(new BookItem("Book", new Sprite(0, 8, 0), null));
            items.Add(new BookItem("Antidious", new Sprite(1, 8, 0), BookData.antVenomBook, true));
            return items.ToArray();
        }

        protected string book; // TODO this is not saved yet; it could be, for editable books.
        private readonly bool hasTitlePage;
        private Sprite sprite;

        private BookItem(string title, Sprite sprite, string book)
            : this(title, sprite, book, false)
        {
        }

        private BookItem(string title, Sprite sprite, string book, bool hasTitlePage)
            : base(title, sprite)
        {
            this.book = book;
            this.hasTitlePage = hasTitlePage;
            this.sprite = sprite;
        }

        public override bool InteractOn(Tile tile, Level level, int xt, int yt, Player player, Direction attackDir)
        {
            Game.SetMenu(new BookDisplay(book, hasTitlePage));
            return true;
        }

        public override bool InteractsWithWorld()
        {
            return false;
        }

        public override BookItem Clone()
        {
            return new BookItem(GetName(), sprite, book, hasTitlePage);
        }
    }

}
