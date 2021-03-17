using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinicraftPlusSharp.Core.IO
{
    public class InputHandler
    {
        public string keyToChange = null; // this is used when listening to change key bindings.
        private string keyChanged = null; // this is used when listening to change key bindings.
        private bool overwrite = false;
        private object lockObject = new();

        public string GetChangedKey()
        {
            string key = keyChanged + ";" + keymap[keyChanged];

            keyChanged = null;

            return key;
        }

        private static Dictionary<Keys, string> keyNames = new();

        static InputHandler()
        {
            foreach (Keys key in Enum.GetValues<Keys>())
            {
                string name = key.ToString();

                try
                {
                    keyNames.Add(key, name);
                }
                catch { }
            }

            // for compatibility becuase I'm lazy. :P
            keyNames.Add(Keys.Back, "BACKSPACE");
            keyNames.Add(Keys.ControlKey, "CTRL");
        }

        private Dictionary<string, string> keymap; // The symbolic map of actions to physical key names.
        private Dictionary<string, Key> keyboard; // The actual map of key names to Key objects.
        private string lastKeyTyped = ""; // Used for things like typing world names.
        private string keyTypedBuffer = ""; // used to store the last key typed before putting it into the main var during tick().

        public InputHandler()
        {
            keymap = new(); //stores custom key name with physical key name in keyboard.
            keyboard = new(); //stores physical keyboard keys; auto-generated :D

            InitKeyMap(); // this is seperate so I can make a "restore defaults" option.

            // I'm not entirely sure if this is necessary... but it doesn't hurt.
            keyboard.Add("SHIFT", new Key(true));
            keyboard.Add("CTRL", new Key(true));
            keyboard.Add("ALT", new Key(true));
        }

        public InputHandler(Control inputSource)
            : this()
        {
            inputSource.KeyDown += InputSource_KeyDown;
            inputSource.KeyUp += InputSource_KeyUp;
            inputSource.KeyPress += InputSource_KeyPress;
        }

        private void InitKeyMap()
        {
            keymap.Add("MOVE-UP", "UP|W");
            keymap.Add("MOVE-DOWN", "DOWN|S");
            keymap.Add("MOVE-LEFT", "LEFT|A");
            keymap.Add("MOVE-RIGHT", "RIGHT|D");

            keymap.Add("CURSOR-UP", "UP");
            keymap.Add("CURSOR-DOWN", "DOWN");
            keymap.Add("CURSOR-LEFT", "LEFT");
            keymap.Add("CURSOR-RIGHT", "RIGHT");

            keymap.Add("SELECT", "ENTER");
            keymap.Add("EXIT", "ESCAPE");

            keymap.Add("QUICKSAVE", "R"); // saves the game while still playing

            keymap.Add("ATTACK", "C|SPACE|ENTER"); //attack action references "C" key
            keymap.Add("MENU", "X|E"); //and so on... menu does various things.
            keymap.Add("CRAFT", "Z|SHIFT-E"); // open/close personal crafting window.
            keymap.Add("PICKUP", "V|P"); // pickup torches / furniture; this replaces the power glove.
            keymap.Add("DROP-ONE", "Q"); // drops the item in your hand, or selected in your inventory, by ones; it won't drop an entire stack
            keymap.Add("DROP-STACK", "SHIFT-Q"); // drops the item in your hand, or selected in your inventory, entirely; even if it's a stack.

            keymap.Add("PAUSE", "ESCAPE"); // pause the Game.

            keymap.Add("SURVIVAL=debug", "SHIFT-S|SHIFT-1");
            keymap.Add("CREATIVE=debug", "SHIFT-C|SHIFT-2");

            keymap.Add("POTIONEFFECTS", "P"); // toggle potion effect display
                                              //keymap.put("FPSDISP", "F3"); // toggle fps display
            keymap.Add("INFO", "SHIFT-I"); // toggle player stats display
        }

        public void ResetKeyBindings()
        {
            keymap.Clear();
            InitKeyMap();
        }

        /** Processes each key one by one, in keyboard. */
        public void Tick()
        {
            lastKeyTyped = keyTypedBuffer;
            keyTypedBuffer = "";

            lock (lockObject)
            {
                foreach (Key key in keyboard.Values)
                {
                    key.Tick(); //call tick() for each key.
                }
            }
        }

        //The Key class.
        public class Key
        {
            //presses = how many times the Key has been pressed.
            //absorbs = how many key presses have been processed.
            private int presses, absorbs;
            //down = if the key is currently physically being held down.
            //clicked = if the key is still being processed at the current tick.
            public bool down, clicked;
            //sticky = true if presses reaches 3, and the key continues to be held down.
            private bool sticky;

            bool stayDown;

            public Key()
                : this(false)
            {
            }

            public Key(bool stayDown)
            {
                this.stayDown = stayDown;
            }

            /** toggles the key down or not down. */
            public void Toggle(bool pressed)
            {
                down = pressed; // set down to the passed in value; the if statement is probably unnecessary...
                if (pressed && !sticky) presses++; //add to the number of total presses.
            }

            /** Processes the key presses. */
            public void Tick()
            {
                if (absorbs < presses)
                { // If there are more key presses to process...
                    absorbs++; //process them!

                    if (presses - absorbs > 3)
                    {
                        absorbs = presses - 3;
                    }

                    clicked = true; // make clicked true, since key presses are still being processed.
                }
                else
                { // All key presses so far for this key have been processed.
                    sticky = !sticky ? presses > 3 : down;
                    clicked = sticky; // set clicked to false, since we're done processing; UNLESS the key has been held down for a bit, and hasn't yet been released.

                    //reset the presses and absorbs, to ensure they don't get too high, or something:
                    presses = 0;
                    absorbs = 0;
                }
            }

            public void Release()
            {
                down = false;
                clicked = false;
                presses = 0;
                absorbs = 0;
                sticky = false;
            }

            //custom toString() method, I used it for debugging.
            public override string ToString()
            {
                return "down:" + down + "; clicked:" + clicked + "; presses=" + presses + "; absorbs=" + absorbs;
            }
        }

        /** This is used to stop all of the actions when the game is out of focus. */
        public void ReleaseAll()
        {
            foreach (Key key in keyboard.Values)
            {
                key.Release();
            }
        }

        /// this is meant for changing the default keys. Call it from the options menu, or something.
        public void SetKey(string keymapKey, string keyboardKey)
        {
            if (keymapKey != null && keymap.ContainsKey(keymapKey) && (!keymapKey.Contains("=debug") || Game.debug)) //the keyboardKey can be null, I suppose, if you want to disable a key...
            {
                keymap.Add(keymapKey, keyboardKey);
            }
        }

        /** Simply returns the mapped value of key in keymap. */
        public string GetMapping(string actionKey)
        {
            actionKey = actionKey.ToUpper();

            return keymap.ContainsKey(actionKey) ? keymap[actionKey].Replace("|", "/") : "NO_KEY";
        }

        /// THIS is pretty much the only way you want to be interfacing with this class; it has all the auto-create and protection functions and such built-in.
        public Key GetKey(string keytext)
        {
            return GetKey(keytext, true);
        }

        private Key GetKey(string keytext, bool getFromMap)
        {
            // if the passed-in key is blank, or null, then return null.
            if (keytext == null || keytext.Length == 0)
            {
                return new Key();
            }

            Key key; // make a new key to return at the end
            keytext = keytext.ToUpper(); // prevent errors due to improper "casing"

            lock (lockObject)
            {
                // this should never be run, actually, b/c the "=debug" isn't used in other places in the code.
                if (keymap.ContainsKey(keytext + "=debug"))
                {
                    if (!Game.debug)
                    {
                        return new Key();
                    }
                    else
                    {
                        keytext += "=debug";
                    }
                }

                if (getFromMap)
                { // if false, we assume that keytext is a physical key.
                  // if the passed-in key equals one in keymap, then replace it with it's match, a key in keyboard.
                    if (keymap.ContainsKey(keytext))
                    {
                        keytext = keymap[keytext]; // converts action name to physical key name
                    }
                }
            }

            string fullKeytext = keytext;

            if (keytext.Contains("|"))
            {
                /// multiple key possibilities exist for this action; so, combine the results of each one!
                key = new Key();

                foreach (string keyposs in keytext.Split("\\|"))
                { // String.split() uses regex, and "|" is a special character, so it must be escaped; but the backslash must be passed in, so it needs escaping.
                    Key aKey = GetKey(keyposs, false); //this time, do NOT attempt to fetch from keymap.

                    // it really does combine using "or":
                    key.down = key.down || aKey.down;
                    key.clicked = key.clicked || aKey.clicked;
                }

                return key;
            }

            lock (lockObject)
            {
                if (keytext.Contains("-")) // truncate compound keys to only the base key, no modifiers
                {
                    keytext = keytext[(keytext.LastIndexOf("-") + 1)..];
                }

                if (keyboard.ContainsKey(keytext))
                {
                    key = keyboard[keytext]; // gets the key object from keyboard, if if exists.
                }
                else
                {
                    // If the specified key does not yet exist in keyboard, then create a new Key, and put it there.
                    key = new Key(); //make new key
                    keyboard.Add(keytext, key); //add it to keyboard

                    //if(Game.debug) System.out.println("Added new key: \'" + keytext + "\'"); //log to console that a new key was added to the keyboard
                }
            } // "key" has been set to the appropriate key Object.

            keytext = fullKeytext;

            if (keytext.Equals("SHIFT") || keytext.Equals("CTRL") || keytext.Equals("ALT"))
            {
                return key; // nothing more must be done with modifier keys.
            }

            bool foundS = false, foundC = false, foundA = false;
            if (keytext.Contains("-"))
            {
                foreach (string keyname in keytext.Split("-"))
                {
                    if (keyname.Equals("SHIFT")) foundS = true;
                    if (keyname.Equals("CTRL")) foundC = true;
                    if (keyname.Equals("ALT")) foundA = true;
                }
            }
            bool modMatch =
              GetKey("shift").down == foundS &&
              GetKey("ctrl").down == foundC &&
              GetKey("alt").down == foundA;

            if (keytext.Contains("-"))
            { // we want to return a compound key, but still care about the trigger key.
                Key mainKey = key; // move the fetched key to a different variable

                key = new Key(); // set up return key to have proper values
                key.down = modMatch && mainKey.down;
                key.clicked = modMatch && mainKey.clicked;
            }
            else if (!modMatch)
            {
                key = new Key();
            }

            //if(key.clicked && Game.debug) System.out.println("Processed key: " + keytext + " is clicked; tickNum=" + ticks);

            return key; // return the Key object.
        }

        /// this method provides a way to press physical keys without actually generating a key event.
        /*public void pressKey(String keyname, bool pressed) {
            Key key = getPhysKey(keyname);
            key.toggle(pressed);
            //System.out.println("Key " + keyname + " is clicked: " + getPhysKey(keyname).clicked);
        }*/

        public List<string> GetAllPressedKeys()
        {
            List<string> keys = new();
            foreach (string keyname in keyboard.Keys)
            {
                if (keyboard[keyname].down)
                {
                    keys.Add(keyname);
                }
            }

            return keys;
        }

        /// this gets a key from key text, w/o adding to the key list.
        private Key GetPhysKey(string keytext)
        {
            keytext = keytext.ToUpper();

            if (keyboard.ContainsKey(keytext))
            {
                return keyboard[keytext];
            }
            else
            {
                //System.out.println("UNKNOWN KEYBOARD KEY: " + keytext); // it's okay really; was just checking
                return new Key(); //won't matter where I'm calling it.
            }
        }

        //called by KeyListener Event methods, below. Only accesses keyboard Keys.
        private void Toggle(Keys keycode, bool pressed)
        {
            string keytext = "NO_KEY";

            if (keyNames.ContainsKey(keycode))
            {
                keytext = keyNames[keycode];
            }
            else
            {
                Console.WriteLine("INPUT: Could not find keyname for keycode \"" + keycode + "\"");
                return;
            }

            keytext = keytext.ToUpper();

            //System.out.println("Interpreted key press: " + keytext);

            //System.out.println("Toggling " + keytext + " key (keycode " + keycode + ") to "+pressed+".");
            if (pressed && keyToChange != null && !IsMod(keytext))
            {
                keymap.Add(keyToChange, (overwrite ? "" : keymap[keyToChange] + "|") + GetCurModifiers() + keytext);
                keyChanged = keyToChange;
                keyToChange = null;

                return;
            }

            GetPhysKey(keytext).Toggle(pressed);
        }

        private static bool IsMod(string keyname)
        {
            keyname = keyname.ToUpper();
            return keyname.Equals("SHIFT") || keyname.Equals("CTRL") || keyname.Equals("ALT");
        }

        private string GetCurModifiers()
        {
            return (GetKey("ctrl").down ? "CTRL-" : "") +
                    (GetKey("alt").down ? "ALT-" : "") +
                    (GetKey("shift").down ? "SHIFT-" : "");
        }

        /** Used by Save.java, to save user key preferences. */
        public string[] GetKeyPrefs()
        {
            List<string> keystore = new(); //make a list for keys

            foreach (string keyname in keymap.Keys) //go though each mapping
            {
                if (!keyname.Contains("=debug") || Game.debug)
                {
                    keystore.Add(keyname + ";" + keymap[keyname]); //add the mapping values as one string, seperated by a semicolon.
                }
            }

            return keystore.ToArray(); //return the array of encoded key preferences.
        }


        public void ChangeKeyBinding(string actionKey)
        {
            keyToChange = actionKey.ToUpper();
            overwrite = true;
        }

        public void AddKeyBinding(string actionKey)
        {
            keyToChange = actionKey.ToUpper();
            overwrite = false;
        }

        private void InputSource_KeyDown(object sender, KeyEventArgs e)
        {
            Toggle(e.KeyCode, true);
        }

        private void InputSource_KeyUp(object sender, KeyEventArgs e)
        {
            Toggle(e.KeyCode, false);
        }

        private void InputSource_KeyPress(object sender, KeyPressEventArgs e)
        {
            keyTypedBuffer = e.KeyChar.ToString();
        }

        public string AddKeyTyped(string typing, string pattern)
        {
            if (lastKeyTyped.Length > 0)
            {
                string letter = lastKeyTyped;
                lastKeyTyped = "";

                if (letter.All(c => !char.IsControl(c)) && (pattern == null || Regex.IsMatch(letter, pattern)))
                {
                    typing += letter;
                }
            }

            if (GetKey("backspace").clicked && typing.Length > 0)
            {
                // backspace counts as a letter itself, but we don't have to worry about it if it's part of the regex.
                typing = typing[0..^1];
            }

            return typing;
        }
    }
}
