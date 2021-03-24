using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class LinkEntry : SelectEntry
    {
        private static readonly string openMsg = "Opening with browser...";

        private readonly int color;

        // note that if the failMsg should be localized, such must be done before passing them as parameters, for this class will not do it since, by default, the failMsg contains a url.

        public LinkEntry(int color, string urlText)
            : this(color, urlText, urlText, false)
        {
        }

        public LinkEntry(int color, string text, string url)
            : this(color, text, url, true)
        {
        }
        public LinkEntry(int color, string text, string url, string failMsg)
            : this(color, text, url, failMsg, true)
        {
        }

        public LinkEntry(int color, string text, string url, bool localize)
            : this(color, text, url, Localization.GetLocalized("Go to") + ": " + url, localize)
        {
        }

        public LinkEntry(int color, string text, string url, string failMsg, bool localize)
            : base(text, () =>
            {
                // try to open the download link directly from the browser.
                try
                {
                    Game.SetMenu(new TempDisplay(3000, false, true, new Menu.Builder(true, 0, RelPos.CENTER, new StringEntry(Localization.GetLocalized(openMsg))).CreateMenu()));
                    Process.Start(url);
                }
                catch (IOException e)
                {
                    Console.Error.WriteLine("Could not parse LinkEntry url \"" + url + "\" into valid URI:");
                    e.PrintStackTrace();
                }

                //if (!canBrowse)
                //{
                //    Game.SetMenu(new BookDisplay(failMsg, false));
                //}

            }, localize)
        {
            this.color = color;
        }

        public override int GetColor(bool isSelected)
        {
            return color;
        }
    }
}
