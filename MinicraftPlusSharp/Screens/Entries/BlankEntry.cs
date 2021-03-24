using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class BlankEntry : ListEntry
    {
        public BlankEntry()
        {
            SetSelectable(false);
        }

        public override void Tick(InputHandler input)
        {
        }

        public override void Render(Screen screen, int x, int y, bool isSelected)
        {
        }

        public override int GetWidth()
        {
            return SpriteSheet.boxWidth;
        }

        public override string ToString()
        {
            return " ";
        }
    }
}
