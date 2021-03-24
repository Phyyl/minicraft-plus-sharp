using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public abstract class ListEntry
    {
        public static readonly int COL_UNSLCT = Color.GRAY;
        public static readonly int COL_SLCT = Color.WHITE;

        private bool selectable = true, visible = true;

        /**
		 * Ticks the entry. Used to handle input from the InputHandler
		 * @param input InputHandler used to get player input.
		 */
        public abstract void Tick(InputHandler input);

        /**
		 * Renders the entry to the given screen.
		 * Coordinate origin is in the top left corner of the entry space.
		 * @param screen Screen to render the entry to
		 * @param x X coordinate
		 * @param y Y coordinate
		 * @param isSelected true if the entry is selected, false otherwise
		 */
        public virtual void Render(Screen screen, int x, int y, bool isSelected)
        {
            if (visible)
            {
                Font.Draw(ToString(), screen, x, y, GetColor(isSelected));
            }
        }

        /**
		 * Returns the current color depending on if the entry is selected.
		 * @param isSelected true if the entry is selected, false otherwise
		 * @return the current entry color
		 */
        public virtual int GetColor(bool isSelected)
        {
            return isSelected ? COL_SLCT : COL_UNSLCT;
        }

        /**
		 * Calculates the width of the entry.
		 * @return the entry's width
		 */
        public virtual int GetWidth()
        {
            return Font.TextWidth(ToString());
        }

        /**
		 * Calculates the height of the entry.
		 * @return the entry's height
		 */
        public static int GetHeight()
        {
            return Font.TextHeight();
        }

        /**
		 * Determines if this entry can be selected.
		 * @return true if it is visible and can be selected, false otherwise.
		 */
        public bool IsSelectable()
        {
            return selectable && visible;
        }

        /**
		 * Returns whether the entry is visible or not.
		 * @return true if the entry is visible, false otherwise
		 */
        public bool IsVisible()
        {
            return visible;
        }

        /**
		 * Changes if the entry can be selected or not.
		 * @param selectable true if the entry can be selected, false if not
		 */
        public void SetSelectable(bool selectable)
        {
            this.selectable = selectable;
        }

        /**
		 * Changes if the entry is visible or not.
		 * @param visible true if the entry should be visible, false if not
		 */
        public void SetVisible(bool visible)
        {
            this.visible = visible;
        }

        public override abstract string ToString();
    }
}
