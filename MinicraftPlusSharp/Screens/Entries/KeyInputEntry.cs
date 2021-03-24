using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class KeyInputEntry : SelectEntry
    {
        private string action, mapping, buffer;

        public KeyInputEntry(string key)
            : base("", null)
        {

            this.action = key.Substring(0, key.IndexOf(";"));
            SetMapping(key.Substring(key.IndexOf(";") + 1));
        }

        private void SetMapping(string mapping)
        {
            this.mapping = mapping;

            StringBuilder buffer = new StringBuilder();

            for (int spaces = 0; spaces < Screen.w / Font.TextWidth(" ") - action.Length - mapping.Length; spaces++)
            {
                buffer.Append(" ");
            }

            this.buffer = buffer.ToString();
        }

        public override void Tick(InputHandler input)
        {
            if (input.GetKey("c").clicked || input.GetKey("enter").clicked)
            {
                input.ChangeKeyBinding(action);
            }
            else if (input.GetKey("a").clicked)
            {
                // add a binding, don't remove previous.
                input.AddKeyBinding(action);
            }
        }

        public override int GetWidth()
        {
            return Screen.w;
        }

        public override string ToString()
        {
            return Localization.GetLocalized(action) + buffer + mapping;
        }
    }

}
