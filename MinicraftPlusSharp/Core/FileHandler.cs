using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Core
{
    public class FileHandler : Game
    {
        private FileHandler() { }

        public static readonly int REPLACE_EXISTING = 0;
        public static readonly int RENAME_COPY = 1;
        public static readonly int SKIP = 2;

        static readonly string OS;
        private static readonly string localGameDir;
        static readonly string systemGameDir;

        static FileHandler()
        {
            string local = "playminicraft/mods/Minicraft_Plus";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                systemGameDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            else
            {
                systemGameDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    local = "." + local; // linux
                }
            }

            localGameDir = "/" + local;

            //System.out.println("system game dir: " + systemGameDir);
        }


        static void DetermineGameDir(string saveDir)
        {
            gameDir = saveDir + localGameDir;

            if (debug)
            {
                Console.WriteLine("Determined gameDir: " + gameDir);
            }

            DirectoryInfo testFile = new DirectoryInfo(gameDir);
            testFile.Create();

            DirectoryInfo oldFolder = new DirectoryInfo(saveDir + "/.playminicraft/mods/Minicraft Plus");

            if (oldFolder.Exists)
            {
                try
                {
                    CopyFolderContents(oldFolder, testFile, RENAME_COPY, true);
                }
                catch (IOException e)
                {
                    e.PrintStackTrace();
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                oldFolder = new DirectoryInfo(saveDir + "/.playminicraft");

                if (oldFolder.Exists)
                {
                    try
                    {
                        CopyFolderContents(oldFolder, testFile, RENAME_COPY, true);
                    }
                    catch (IOException e)
                    {
                        e.PrintStackTrace();
                    }
                }
            }
        }

        public static void CopyFolderContents(DirectoryInfo origFolder, DirectoryInfo newFolder, int ifExisting, bool deleteOriginal)
        {
            // I can determine the local folder structure with origFolder.relativize(file), then use newFolder.resolve(relative).
            if (Game.debug)
            {
                Console.WriteLine("Copying contents of folder " + origFolder + " to new folder " + newFolder);
            }

            foreach (var fileSystemInfo in WalkFileTree(origFolder))
            {
                string file = fileSystemInfo.FullName;
                string newFilename = Path.Combine(newFolder.FullName, Path.GetRelativePath(origFolder.FullName, file));

                if (File.Exists(newFilename))
                {
                    if (ifExisting == SKIP)
                    {
                        continue;
                    }
                    else if (ifExisting == RENAME_COPY)
                    {
                        newFilename = newFilename.Substring(0, newFilename.LastIndexOf("."));

                        do
                        {
                            newFilename += "(Old)";
                        } while (File.Exists(newFilename));

                        newFilename += Save.extension;
                    }
                }

                string newFile = newFilename;
                //if (Game.debug) System.out.println("visiting file " + file + "; translating to " + newFile);
                try
                {
                    File.Copy(file, newFile, true);
                }
                catch (Exception ex)
                {
                    ex.PrintStackTrace();
                }
            }

            if (deleteOriginal && origFolder != null)
            {
                origFolder.Delete(true);
            }
        }


        private static IEnumerable<FileSystemInfo> WalkFileTree(DirectoryInfo directory)
        {
            return directory.GetFiles("", new EnumerationOptions { RecurseSubdirectories = true }).OfType<FileInfo>();
        }
    }
}
