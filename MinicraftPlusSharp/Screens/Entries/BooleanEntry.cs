using MinicraftPlusSharp.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class BooleanEntry : ArrayEntry<bool>
    {
        public BooleanEntry(string label, bool initial)
            : base(label, true, new bool[] { true, false })
        {
            SetSelection(initial ? 0 : 1);
        }

        public override string ToString()
        {
            return GetLabel() + ": " + (GetValue() ?
                Localization.GetLocalized("On") :
                Localization.GetLocalized("Off")
            );
        }
    }
}
