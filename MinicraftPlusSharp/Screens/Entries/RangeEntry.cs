using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class RangeEntry : ArrayEntry<int>
    {
        private static int[] GetIntegerArray(int min, int max)
        {
            int[] ints = new int[max - min + 1];

            for (int i = 0; i < ints.Length; i++)
            {
                ints[i] = min + i;
            }

            return ints;
        }

        private int min, max;

        public RangeEntry(string label, int min, int max, int initial)
            : base(label, false, GetIntegerArray(min, max))
        {
            this.min = min;
            this.max = max;

            SetValue(initial);
        }

        public override void SetValue(object o)
        {
            if (o is not int i)
            {
                return;
            }

            SetSelection(i - min);
        }
    }
}
