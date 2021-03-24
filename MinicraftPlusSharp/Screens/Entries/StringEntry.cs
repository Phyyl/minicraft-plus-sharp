using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class StringEntry : ListEntry
    {
        private static readonly int DEFAULT_COLOR = Color.WHITE;

        private string text;
        private int color;

        public static StringEntry[] UseLines(params string[] lines)
        {
            return UseLines(DEFAULT_COLOR, lines);
        }

        public static StringEntry[] UseLines(int color, params string[] lines)
        {
            StringEntry[] entries = new StringEntry[lines.Length];

            for (int i = 0; i < lines.Length; i++)
            {
                entries[i] = new StringEntry(lines[i], color);
            }

            return entries;
        }

        public StringEntry(string text)
            : this(text, DEFAULT_COLOR)
        {
        }

        public StringEntry(string text, int color)
        {
            SetSelectable(false);

            this.text = text;
            this.color = color;
        }

        public override void Tick(InputHandler input)
        {
        }

        public override int GetColor(bool isSelected)
        {
            return color;
        }

        public override string ToString()
        {
            return text;
        }
    }
}
