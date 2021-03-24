using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Screens.Entries
{
    public class InputEntry : ListEntry
    {
        private string prompt;
        private string regex;
        private int maxLength;

        private string userInput;

        private Action<object> listener;

        public InputEntry(string prompt)
            : this(prompt, null, 0)
        {
        }

        public InputEntry(string prompt, string regex, int maxLen)
            : this(prompt, regex, maxLen, "")
        {
        }

        public InputEntry(string prompt, string regex, int maxLen, string initValue)
        {
            this.prompt = prompt;
            this.regex = regex;
            this.maxLength = maxLen;

            userInput = initValue;
        }

        public override void Tick(InputHandler input)
        {
            string prev = userInput;

            userInput = input.AddKeyTyped(userInput, regex);

            if (!prev.Equals(userInput))
            {
                listener?.Invoke(userInput);
            }

            if (maxLength > 0 && userInput.Length > maxLength)
            {
                userInput = userInput.Substring(0, maxLength); // truncates extra
            }
        }

        public string GetUserInput()
        {
            return userInput;
        }

        public override string ToString()
        {
            return Localization.GetLocalized(prompt) + (prompt.Length == 0 ? "" : ": ") + userInput;
        }

        public override void Render(Screen screen, int x, int y, bool isSelected)
        {
            Font.Draw(ToString(), screen, x, y, IsValid() ? isSelected ? Color.GREEN : COL_UNSLCT : Color.RED);
        }

        public bool IsValid()
        {
            return Regex.IsMatch(userInput, regex);
        }

        public void SetChangeListener(Action<object> l)
        {
            listener = l;
        }
    }
}
