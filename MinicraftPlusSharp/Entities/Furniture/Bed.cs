using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class Bed : Furniture
    {


        private static int playersAwake = 1;
        private static readonly Dictionary<Player, Bed> sleepingPlayers = new();

        /**
         * Creates a new furniture with the name Bed and the bed sprite and color.
         */
        public Bed()
            : base("Bed", new Sprite(30, 26, 2, 2, 2), 3, 2)
        {
        }

        /** Called when the player attempts to get in bed. */
        public override bool Use(Player player)
        {
            if (checkCanSleep(player))
            { // if it is late enough in the day to sleep...

                // set the player spawn coord. to their current position, in tile coords (hence " >> 4")
                player.spawnx = player.x >> 4;
                player.spawny = player.y >> 4;

                sleepingPlayers.Add(player, this);

                if (Game.IsConnectedClient() && player == Game.player)
                {
                    Game.client.sendBedRequest(this);
                    playersAwake = -1;
                }

                if (Game.debug)
                {
                    Console.WriteLine(Network.OnlinePrefix() + "player got in bed: " + player);
                }

                player.Remove();

                if (!Game.ISONLINE)
                {
                    playersAwake = 0;
                }
                else if (Game.IsValidServer())
                {
                    playersAwake = GetPlayersAwake();
                    Game.server.UpdateGameVars();
                }
            }

            return true;
        }

        public static int GetPlayersAwake()
        {
            if (!Game.IsValidServer())
            {
                return playersAwake;
            }

            int total = Game.server.GetNumPlayers();

            return total - sleepingPlayers.Count;
        }

        public static void SetPlayersAwake(int count)
        {
            if (!Game.IsValidClient())
            {
                throw new Exception("Bed.setPlayersAwake() can only be called on a client runtime");
            }

            playersAwake = count;
        }

        public static bool checkCanSleep(Player player)
        {
            if (InBed(player))
            {
                return false;
            }

            if (!(Updater.tickCount >= Updater.sleepStartTime || Updater.tickCount < Updater.sleepEndTime && Updater.pastDay1))
            {
                // it is too early to sleep; display how much time is remaining.
                int sec = (int)Math.Ceiling((Updater.sleepStartTime - Updater.tickCount) * 1.0 / Updater.normSpeed); // gets the seconds until sleeping is allowed. // normSpeed is in tiks/sec.

                string note = "Can't sleep! " + (sec / 60) + "Min " + (sec % 60) + " Sec left!";

                if (!Game.IsValidServer())
                {
                    Game.notifications.Add(note); // add the notification displaying the time remaining in minutes and seconds.
                }
                else if (player is RemotePlayer remotePlayer)
                {
                    Game.server.GetAssociatedThread(remotePlayer).sendNotification(note, 0);
                }
                else
                {
                    Console.WriteLine("WARNING: regular player found trying to get into bed on server; not a RemotePlayer: " + player);
                }

                return false;
            }

            return true;
        }

        public static bool Sleeping()
        {
            return playersAwake == 0;
        }

        public static bool InBed(Player player)
        {
            return sleepingPlayers.ContainsKey(player);
        }

        public static Level GetBedLevel(Player player)
        {
            Bed bed = sleepingPlayers.GetValueOrDefault(player);

            if (bed == null)
            {
                return null;
            }

            return bed.GetLevel();
        }

        // get the player "out of bed"; used on the client only.
        public static void RemovePlayer(Player player)
        {
            sleepingPlayers.Remove(player);
        }

        public static void RemovePlayers()
        {
            sleepingPlayers.Clear();
        }

        // client should not call this.
        public static void RestorePlayer(Player player)
        {
            if (sleepingPlayers.TryGetValue(player, out Bed bed))
            {
                sleepingPlayers.Remove(player);

                if (bed.GetLevel() == null)
                {
                    Game.levels[Game.currentLevel].Add(player);
                }
                else
                {
                    bed.GetLevel().Add(player);
                }

                if (!Game.ISONLINE)
                {
                    playersAwake = 1;
                }
                else if (Game.IsValidServer())
                {
                    playersAwake = GetPlayersAwake();
                    Game.server.UpdateGameVars();
                }
            }
        }

        // client should not call this.
        public static void RestorePlayers()
        {
            foreach (Player p in sleepingPlayers.Keys)
            {
                Bed bed = sleepingPlayers[p];

                if (p is RemotePlayer remotePlayer && Game.IsValidServer() && !Game.server.GetAssociatedThread(remotePlayer).IsConnected())
                {
                    continue; // forget about it, don't add it to the level
                }

                bed.GetLevel().Add(p);
            }

            sleepingPlayers.Clear();

            if (!Game.ISONLINE)
            {
                playersAwake = 1;
            }
            else if (Game.IsValidServer())
            {
                playersAwake = Game.server.GetNumPlayers();
                Game.server.UpdateGameVars();
            }
        }
    }
}
