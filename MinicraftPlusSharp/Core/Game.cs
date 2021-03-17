using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Levels;
using MinicraftPlusSharp.Levels.Tiles;
using MinicraftPlusSharp.Networking;
using MinicraftPlusSharp.SaveLoad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Version = MinicraftPlusSharp.SaveLoad.Version;

namespace MinicraftPlusSharp.Core
{
    public class Game
    {
        private Game() // can't instantiate the Game class.
        {
        }

        public static bool debug = false;
        public static bool packet_debug = false;
        public static bool HAS_GUI = true;

        public static readonly string NAME = "Minicraft Plus"; // This is the name on the application window.
        public static readonly Version VERSION = new Version("2.0.7-dev2");

        public static InputHandler input; // input used in Game, Player, and just about all the *Menu classes.
        public static Player player;

        public static string gameDir; // The directory in which all the game files are stored
        public static List<string> notifications = new();

        public static int MAX_FPS = Settings.Get<int>("fps");

        /**
		 * This specifies a custom port instead of default to server-side using
		 * --port parameter if something goes wrong in setting the new port
		 * it'll use the default one {@link MinicraftProtocol#PORT}
		 */
        public static int CUSTOM_PORT = MinicraftProtocol.PORT;

        static Display menu = null, newMenu = null; // the current menu you are on.
                                                    // Sets the current menu.
        public static void SetMenu(Display display)
        {
            newMenu = display;
        }

        public static void ExitMenu()
        {
            if (menu == null)
            {
                if (debug)
                {
                    Console.WriteLine("Game.exitMenu(): No menu found, returning!");
                }

                return; // no action required; cannot exit from no menu
            }

            Sound.back.Play();

            newMenu = menu.GetParent();
        }

        public static Display GetMenu()
        {
            return newMenu;
        }

        public static bool IsMode(string mode)
        {
            return Settings.Get<string>("mode").Equals(mode, StringComparison.InvariantCultureIgnoreCase);
        }


        // MULTIPLAYER

        public static bool ISONLINE = false;
        public static bool ISHOST = false;

        public static MinicraftClient client = null;

        public static bool IsValidClient()
        {
            return ISONLINE && client != null;
        }

        public static bool IsConnectedClient()
        {
            return IsValidClient() && client.isConnected();
        }

        public static MinicraftServer server = null;

        /** Checks if you are a host and the game is a server */
        public static bool IsValidServer()
        {
            return ISONLINE && ISHOST && server != null;
        }

        public static bool HasConnectedClients()
        {
            return IsValidServer() && server.HasClients();
        }


        // LEVEL

        public static Level[] levels = new Level[6]; // This array stores the different levels.
        public static int currentLevel = 3; // This is the level the player is on. It defaults to 3, the surface.

        static bool gameOver = false; // If the player wins this is set to true.

        static bool running = true;

        public static void Quit()
        {
            if (IsConnectedClient())
            {
                client.EndConnection();
            }

            if (IsValidServer())
            {
                server.EndConnection();
            }

            running = false;
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                //TODO
            };

            Initializer.ParseArgs(args);

            input = new InputHandler(Renderer.canvas);

            Tiles.InitTileList();
            Sound.Init();
            Settings.Init();

            World.ResetGame(); // "half"-starts a new game, to set up initial variables
            player.eid = 0;
            new Load(true); // this loads any saved preferences.


            if (Network.autoclient)
            {
                SetMenu(new MultiplayerDisplay("localhost"));
            }
            else if (!HAS_GUI)
            {
                Network.StartMultiplayerServer();
            }
            else
            {
                SetMenu(new TitleDisplay()); //sets menu to the title screen.
            }

            Initializer.CreateAndDisplayFrame();

            Renderer.InitScreen();

            Initializer.Run();

            if (debug)
            {
                Console.WriteLine("Main game loop ended; Terminating application...");
            }
        }
    }
}
