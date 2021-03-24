using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinicraftPlusSharp.Core.IO
{
    public class Settings
    {
        private static Dictionary<string, ArrayEntry> options = new();

        static Settings()
        {
            options.Add("fps", new RangeEntry("Max FPS", 10, 300, getRefreshRate())); // Has to check if the game is running in a headless mode. If it doesn't set the fps to 60
            options.Add("diff", new ArrayEntry<string>("Difficulty", "Easy", "Normal", "Hard"));
            options["diff"].setSelection(1);
            options.Add("mode", new ArrayEntry<int>("Game Mode", "Survival", "Creative", "Hardcore", "Score"));

            options.Add("scoretime", new ArrayEntry("Time (Score Mode)", 10, 20, 40, 60, 120));
            options["scoretime"].setValueVisibility(10, false);
            options["scoretime"].setValueVisibility(120, false);

            options.Add("sound", new boolEntry("Sound", true));
            options.Add("autosave", new boolEntry("Autosave", true));

            options.Add("size", new ArrayEntry("World Size", 128, 256, 512));
            options.Add("theme", new ArrayEntry("World Theme", "Normal", "Forest", "Desert", "Plain", "Hell"));
            options.Add("type", new ArrayEntry("Terrain Type", "Island", "Box", "Mountain", "Irregular"));

            options.Add("unlockedskin", new boolEntry("Wear Suit", false));
            options.Add("skinon", new boolEntry("Wear Suit", false));

            options.Add("language", new ArrayEntry("Language", true, false, Localization.GetLanguages()));
            options["language"].setValue(Localization.GetSelectedLanguage());
            

            options["mode"].setChangeAction(value =>
                options["scoretime"].setVisible("Score".equals(value))
            );

            options["unlockedskin"].setChangeAction(value =>
                options["skinon"].setVisible((bool)value)
            );

            options.Add("textures", new ArrayEntry<string>("Textures", "Original", "Custom"));
            options["textures"].setSelection(0);
        }

        public static void Init() { }

        public static T Get<T>(string option)
        {
            return (T)Get(option);
        }

        // returns the value of the specified option
        public static object Get(string option)
        {
            return options[option.ToLower()].GetValue();
        }

        // returns the index of the value in the list of values for the specified option
        public static int GetIdx(string option)
        {
            return options[option.ToLower()].GetSelection();
        }

        // return the ArrayEntry object associated with the given option name.
        public static ArrayEntry GetEntry(string option)
        {
            return options[option.ToLower()];
        }

        // sets the value of the given option name, to the given value, provided it is a valid value for that option.
        public static void Set(string option, object value)
        {
            options[option.ToLower()].setValue(value);
        }

        // sets the index of the value of the given option, provided it is a valid index
        public static void SetIdx(string option, int idx)
        {
            options[option.ToLower()].setSelection(idx);
        }


        //TODO: Implement refresh rate
        private static int GetRefreshRate()
        {
            return 60;

            //if (GraphicsEnvironment.isHeadless()) return 60;

            //int hz = GraphicsEnvironment.getLocalGraphicsEnvironment().getDefaultScreenDevice().getDisplayMode().getRefreshRate();
            //if (hz == DisplayMode.REFRESH_RATE_UNKNOWN) return 60;
            //if (hz > 300) return 60;
            //if (10 > hz) return 60;
            //return hz;
        }
    }

}
