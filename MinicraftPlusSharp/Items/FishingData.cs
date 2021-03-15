using MinicraftPlusSharp.Java;
using System.IO;

namespace MinicraftPlusSharp.Items
{
    public class FishingData
    {
        public static readonly string[] fishData = GetData("fish");

        public static readonly string[] toolData = GetData("tool");

        public static readonly string[] junkData = GetData("junk");

        public static readonly string[] rareData = GetData("rare");

        public static string[] GetData(string name)
        {
            try
            {
                return File.ReadAllLines("/resources/fishing/" + name + "_loot.txt");
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }

            return null;
        }
    }
}