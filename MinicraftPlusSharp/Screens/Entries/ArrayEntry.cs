using MinicraftPlusSharp.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class ArrayEntry<T> : ListEntry
    {
        private string label;
        private T[] options;
        private bool[] optionVis;

        private int selection;
        private bool wrap;
        private bool localize;

        private int maxWidth;

        private Action<object> changeAction;

        public ArrayEntry(string label, params T[] options)
            : this(label, true, true, options)
        {
        }

        public ArrayEntry(string label, bool wrap, params T[] options)
            : this(label, wrap, true, options)
        {
        }

        public ArrayEntry(string label, bool wrap, bool localize, params T[] options)
        {
            this.label = label;
            this.options = options;
            this.wrap = wrap;
            this.localize = localize;

            maxWidth = 0;

            foreach (T option in options)
            {
                maxWidth = Math.Max(maxWidth, Font.TextWidth(localize ? Localization.GetLocalized(option.ToString()) : option.ToString()));
            }

            optionVis = new bool[options.Length];

            Array.Fill(optionVis, true);
        }

        public void SetSelection(int idx)
        {
            bool diff = idx != selection;

            if (idx >= 0 && idx < options.Length)
            {
                selection = idx;

                if (diff)
                {
                    changeAction?.Invoke(GetValue());
                }
            }
        }

        public virtual void SetValue(object value)
        {
            SetSelection(GetIndex(value)); // if it is -1, setSelection simply won't set the value.
        }

        protected string GetLabel()
        {
            return label;
        }

        public int GetSelection()
        {
            return selection;
        }

        public T GetValue()
        {
            return options[selection];
        }

        public bool ValueIs(Object value)
        {
            if (value is string s && options is string[] arr)
            {
                return s.Equals(GetValue() as string, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return GetValue().Equals(value);
            }
        }


        private int GetIndex(Object value)
        {
            if (value is string s && options is string[] arr)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    if (s.Equals(options[i] as string, StringComparison.InvariantCultureIgnoreCase) || options[i].Equals(value))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }


        public void SetValueVisibility(Object value, bool visible)
        {
            int idx = GetIndex(value);

            if (idx >= 0)
            {
                optionVis[idx] = visible;

                if (idx == selection && !visible)
                {
                    MoveSelection(1);
                }
            }
        }

        public bool GetValueVisibility(Object value)
        {
            int idx = GetIndex(value);

            return idx < 0 ? false : optionVis[idx];
        }


        public override void Tick(InputHandler input)
        {
            int prevSel = this.selection;
            int selection = this.selection;

            if (input.GetKey("cursor-left").clicked) selection--;
            if (input.GetKey("cursor-right").clicked) selection++;

            if (prevSel != selection)
            {
                Sound.select.Play();
                MoveSelection(selection - prevSel);
            }
        }

        private void MoveSelection(int dir)
        {
            // stuff for changing the selection, including skipping locked entries
            int prevSel = this.selection;
            int selection = this.selection;

            do
            {
                selection += dir;

                if (wrap)
                {
                    selection = selection % options.Length;

                    if (selection < 0)
                    {
                        selection = options.Length - 1;
                    }
                }
                else
                {
                    selection = Math.Min(selection, options.Length - 1);
                    selection = Math.Max(0, selection);
                }
            } while (!optionVis[selection] && selection != prevSel);

            SetSelection(selection);
        }

        public override int GetWidth()
        {
            return Font.TextWidth(Localization.GetLocalized(label) + ": ") + maxWidth;
        }

        public override string ToString()
        {
            string str = Localization.GetLocalized(label) + ": ";
            string option = options[selection].ToString();

            str += localize ? Localization.GetLocalized(option) : option;

            return str;
        }

        public void SetChangeAction(Action<object>  l)
        {
            this.changeAction = l;

            l?.Invoke(GetValue());
        }
    }
}
