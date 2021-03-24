using MinicraftPlusSharp.Entities;
using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.Levels;
using MinicraftPlusSharp.SaveLoad;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Core
{
    public class Network : Game
    {

        private Network() { }

        private static readonly JavaRandom random = new JavaRandom();

        public static bool autoclient = false; // Used in the initScreen method; jumps to multiplayer menu as client

        private static VersionInfo latestVersion = null;

        // obviously, this can be null.
        public static VersionInfo GetLatestVersion()
        {
            return latestVersion;
        }


        public static void FindLatestVersion(Action callback)
        {
            new Thread(() =>
            {

                if (debug)
                {
                    Console.WriteLine("Fetching release list from GitHub..."); // Fetch the latest version from GitHub
                }

                try
                {
                    HttpWebResponse response = (HttpWebResponse)HttpWebRequest.CreateHttp("https://api.github.com/repos/chrisj42/minicraft-plus-revived/releases").GetResponse();

                    string body = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    JObject @obj = JsonConvert.DeserializeObject<JObject>(body);

                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Console.Error.WriteLine("Version request returned status code " + response.StatusCode + ": " + response.StatusDescription);
                        Console.Error.WriteLine("Response body: " + @obj);

                        latestVersion = new VersionInfo(VERSION, "", "");
                    }
                    else
                    {
                        latestVersion = new VersionInfo(@obj.Value<JObject[]>()[0]);
                    }
                }
                catch (Exception e)
                {
                    e.PrintStackTrace();
                    latestVersion = new VersionInfo(VERSION, "", "");
                }

                callback(); // finished.
            }).Start();
        }

        public static Entity GetEntity(int eid)
        {
            foreach (Level level in levels)
            {
                if (level == null)
                {
                    continue;
                }

                foreach (Entity e in level.GetEntityArray())
                {
                    if (e.eid == eid)
                    {
                        return e;
                    }
                }
            }

            return null;
        }

        public static int GenerateUniqueEntityId()
        {
            int eid;
            int tries = 0; // just in case it gets out of hand.

            do
            {
                tries++;

                if (tries == 1000)
                {
                    Console.WriteLine("Note: Trying 1000th time to find valid entity id...(Will continue)");
                }

                eid = random.NextInt();
            } while (!IdIsAvailable(eid));

            return eid;
        }

        public static bool IdIsAvailable(int eid)
        {
            if (eid == 0)
            {
                return false; // this is reserved for the main player... kind of...
            }

            if (eid < 0)
            {
                return false; // id's must be positive numbers.
            }

            foreach (Level level in levels)
            {
                if (level == null)
                {
                    continue;
                }

                foreach (Entity e in level.GetEntityArray())
                {
                    if (e.eid == eid)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static string OnlinePrefix()
        {
            if (!ISONLINE)
            {
                return "";
            }

            string prefix = "From ";

            if (IsValidServer())
            {
                prefix += "Server";
            }
            else if (IsValidClient())
            {
                prefix += "Client";
            }
            else
            {
                prefix += "nobody";
            }

            prefix += ": ";

            return prefix;
        }

        public static void StartMultiplayerServer()
        {
            if (debug)
            {
                Console.WriteLine("Starting multiplayer server...");
            }

            if (HAS_GUI)
            {
                // here is where we need to start the new client.
                string exeFilePath = typeof(Game).Assembly.Location;

                List<string> arguments = new();

                if (debug)
                {
                    arguments.Add("--debug");
                }

                // this will just always be added.
                arguments.Add("--savedir");
                arguments.Add(FileHandler.systemGameDir);

                arguments.Add("--localclient");

                // this *should* start a new JVM from the running jar file...
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = exeFilePath,
                        Arguments = string.Join(" ", arguments)
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Problem starting new jar file process:");
                    ex.PrintStackTrace();
                }
            }
            else
            {
                SetMenu(new LoadingDisplay()); // gets things going to load up a (server) world
            }

            // now that that's done, let's turn *this* running JVM into a server:
            server = new Networking.MinicraftServer(Game.CUSTOM_PORT);

            new Load(WorldSelectDisplay.GetWorldName(), server); // load server config

            if (latestVersion == null)
            {
                Console.WriteLine("VERSIONCHECK: Checking for updates...");
                FindLatestVersion(() =>
                {
                    if (latestVersion.version.CompareTo(VERSION) > 0) // link new version
                    {
                        Console.WriteLine("VERSIONCHECK: Found newer version: Version " + latestVersion.releaseName + " Available! Download direct from \"" + latestVersion.releaseUrl + "\". Can also be found with change log at \"https://www.github.com/chrisj42/minicraft-plus-revived/releases\".");
                    }
                    else if (latestVersion.releaseName.Length > 0)
                    {
                        Console.WriteLine("VERSIONCHECK: No updates found, you have the latest version.");
                    }
                    else
                    {
                        Console.WriteLine("VERSIONCHECK: Connection failed, could not check for updates.");
                    }
                });
            }
        }
    }
}
