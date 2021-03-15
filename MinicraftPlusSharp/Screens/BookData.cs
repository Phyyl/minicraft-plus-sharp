using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens
{
    public class BookData
    {
        public static readonly string about = LoadBook("about");
        public static readonly string instructions = LoadBook("instructions");
        public static readonly string antVenomBook = LoadBook("antidous");
        public static readonly string storylineGuid = LoadBook("story_guide");

        private static readonly string LoadBook(string bookTitle)
        {
            string book = "";

            try
            {
                book = string.Join(Load)
            }
        }
    }
}
