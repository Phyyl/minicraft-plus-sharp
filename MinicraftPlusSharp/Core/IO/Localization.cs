using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Core.IO
{
    public class Localization
    {
        private static readonly HashSet<string> knownUnlocalizedStrings = new();

        private static readonly Dictionary<string, string> localization = new();
        private static string selectedLanguage = "english";

        private static readonly Dictionary<string, CultureInfo> locales = new();
        private static readonly Dictionary<string, string> localizationFiles = new();

        private static readonly string[] loadedLanguages = GetLanguagesFromDirectory();

        static Localization()
        {
            if (loadedLanguages == null)
                loadedLanguages = new string[] { selectedLanguage };

            LoadSelectedLanguageFile();
        }

        public static string GetLocalized(string @string)
        {
            if (Regex.IsMatch(@string, @"^[ ]*$"))
            {
                return @string; // blank, or just whitespace
            }

            try
            {
                double num = double.Parse(@string);
                return @string; // this is a number; don't try to localize it
            }
            catch (FormatException)
            {
            }

            string localString = localization.GetValueOrDefault(@string);

            // foreach ($1 $2 in $3)ln(string +" to "+localString);

            if (Game.debug && localString == null)
            {
                if (!knownUnlocalizedStrings.Contains(@string))
                {
                    Console.WriteLine("The string \"" + @string + "\" is not localized, returning itself instead.");
                }

                knownUnlocalizedStrings.Add(@string);
            }

            return localString ?? @string;
        }

        public static CultureInfo GetSelectedLocale()
        {
            return CultureInfo.GetCultureInfo(selectedLanguage) ?? CultureInfo.DefaultThreadCurrentCulture;
        }


        public static string GetSelectedLanguage()
        {
            return selectedLanguage;
        }

        public static void ChangeLanguage(string newLanguage)
        {
            selectedLanguage = newLanguage;

            LoadSelectedLanguageFile();
        }

        private static void LoadSelectedLanguageFile()
        {
            string fileText = GetFileAsString();

            // foreach ($1 $2 in $3)ln("File:");
            // foreach ($1 $2 in $3)ln(fileText);

            string currentKey = "";

            foreach (string line in fileText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            {
                // # at the start of a line means the line is a comment.
                if (line.StartsWith("#"))
                {
                    continue;
                }

                if (Regex.IsMatch(line, @"^[ ]*$")) continue;

                if (currentKey.Equals(""))
                {
                    currentKey = line;
                }
                else
                {
                    localization.Add(currentKey, line);
                    currentKey = "";
                }
            }
        }

        private static string GetFileAsString()
        {
            string file = localizationFiles.GetValueOrDefault(selectedLanguage, "/resources/localization/english_en-us.mcpl");

            return File.ReadAllText(file);
        }

        public static string[] GetLanguages()
        {
            return loadedLanguages;
        }

        // Couldn't find a good way to find all the files in a directory when the program is
        // exported as a jar file so I copied this. Thanks!
        // https://stackoverflow.com/questions/1429172/how-do-i-list-the-files-inside-a-jar-file/1429275#1429275
        private static string[] GetLanguagesFromDirectory()
        {
            List<string> languages = new();

            try
            {
                foreach (var file in Directory.GetFiles(@"resources/localization/", "*.mcpl"))
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    int index = fileName.IndexOf('_');
                    string lang = fileName[0..index];
                    string value = fileName[(index + 1)..];
                    
                    languages.Add(lang);
                    localizationFiles.Add(lang, file);
                    locales.Add(lang, new CultureInfo(value));
                }
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
             
                return null;
            }

            return languages.ToArray();
        }
    }
}
