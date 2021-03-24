using MinicraftPlusSharp.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class SelectEntry : ListEntry
    {
        private Action onSelect;
        private string text;
        private bool localize;

        /**
         * Creates a new entry which acts as a button. 
         * Can do an action when it is selected.
         * @param text Text displayed on this entry
         * @param onSelect Action which happens when the entry is selected
         */
        public SelectEntry(string text, Action onSelect)
            : this(text, onSelect, true)
        {
        }

        public SelectEntry(string text, Action onSelect, bool localize)
        {
            this.onSelect = onSelect;
            this.text = text;
            this.localize = localize;
        }

        /**
         * Changes the text of the entry.
         * @param text new text
         */
        public void SetText(string text)
        {
            this.text = text;
        }

        public override void Tick(InputHandler input)
        {
            if (input.GetKey("select").clicked)
            {
                Sound.confirm.Play();
                onSelect?.Invoke();
            }
        }

        public override int GetWidth()
        {
            return Font.TextWidth(ToString());
        }

        public override string ToString()
        {
            return localize ? Localization.GetLocalized(text) : text;
        }
    }
}
