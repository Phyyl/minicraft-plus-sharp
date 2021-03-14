using MinicraftPlusSharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Core
{
    public class Game
    {
        public static bool debug = false;
        public static bool packet_debug = false;
        public static bool HAS_GUI = true;

        public static readonly string NAME = "Minicraft Plus";
        public static readonly string VERSION = "2.0.7-dev2";

        public static bool ISONLINE = false;
        public static bool ISHOST = false;

        public static MinicraftClient client = null;
        public static bool IsValidClient() => ISONLINE && client is not null;
        public static bool IsConnectedClient() => IsValidClient() && client.IsConnected();

        public static MinicraftServer server = null;

        public static bool IsValidServer() => ISONLINE && ISHOST && server is not null;
        public static HasConnectedClients() => IsValidServer() && server.HasClients();

        public static Level[] levels = new Level[6];
        public static int currentLevel = 3;

        private static bool gameOver = false;
        private static bool running = true;

        public static void Quit()
        {
            if (IsConnectedClient())
            {
                client.EndConnection;
            }

            if (IsValidServer())
            {
                server.EndConnection();
            }

            running = false;
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                //TODO
            };

            Initializer.ParseArgs(args);

            input
        }
    }
}
