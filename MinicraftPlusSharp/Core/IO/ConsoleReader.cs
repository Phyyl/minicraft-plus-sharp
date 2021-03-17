using MinicraftPlusSharp.Entities;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.Levels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Core.IO
{
    public class ConsoleReader : JavaThread
    {
        public abstract class Config : JavaEnum<Config>
        {
            public static readonly Config PLAYERCAP = new PlayerCapConfig();
            public static readonly Config AUTOSAVE = new AutoSaveConfig();

            private class PlayerCapConfig : Config
            {
                public override string GetValue()
                {
                    return Game.server.GetPlayerCap().ToString();
                }

                public override bool SetValue(string val)
                {
                    try
                    {
                        Game.server.SetPlayerCap(int.Parse(val));

                        return true;
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("\"" + val + "\" is not a valid number.");
                    }

                    return false;
                }
            }
            private class AutoSaveConfig : Config
            {
                public override string GetValue()
                {
                    return Settings.Get<bool>("autosave").ToString();
                }

                public override bool SetValue(string val)
                {
                    Settings.Set("autosave", bool.Parse(val));

                    return true;
                }
            };

            private Config([CallerMemberName] string name = default) : base(name)
            {
            }

            public abstract string GetValue();

            public abstract bool SetValue(string val);
        }

        public abstract class Command : JavaEnum<Command>
        {
            public static readonly Command HELP = new HelpCommand();
            public static readonly Command STATUS = new StatusCommand();
            public static readonly Command CONFIG = new ConfigCommand();
            public static readonly Command STOP = new StopCommand();
            public static readonly Command RESTART = new RestartCommand();
            public static readonly Command SAVE = new SaveCommand();
            public static readonly Command GAMEMODE = new GameModeCommand();
            public static readonly Command TIME = new TimeCommand();
            public static readonly Command MSG = new MsgCommand();
            public static readonly Command TP = new TpCommand();
            public static readonly Command PING = new PingCommand();
            public static readonly Command KILL = new KillCommand();

            private string generalHelp, detailedHelp, usage;

            private Command(string usage, string general, string[] specific = default, [CallerMemberName] string name = default)
                : base(name)
            {
                string _name = this.Name.ToLower();
                string sep = " - ";

                generalHelp = _name + sep + general;

                this.usage = usage == null ? _name : _name + " " + usage;
                usage = usage != null ? sep + "Usage: " + _name + " " + usage : "";

                detailedHelp = _name + usage + sep + general;

                if (specific != null && specific.Length > 0)
                {
                    detailedHelp += Environment.NewLine + "\t" + string.Join(Environment.NewLine + "\t", specific);
                }
            }

            public abstract void Run(string[] args);

            public string GetUsage()
            {
                return usage;
            }

            public string GetGeneralHelp()
            {
                return generalHelp;
            }

            public string GetDetailedHelp()
            {
                return detailedHelp;
            }

            public static void PrintHelp(Command cmd)
            {
                Console.WriteLine("Usage: " + cmd.GetUsage());
                Console.WriteLine("Type \"help " + cmd + "\" for more info.");
            }

            private static int GetCoordinate(string coord, int baseline)
            {
                if (coord.Contains("~"))
                {
                    if (coord.Equals("~"))
                    {
                        return baseline;
                    }
                    else
                    {
                        return int.Parse(coord.Replace("~", "")) + baseline;
                    }
                }
                else
                {
                    return int.Parse(coord);
                }
            }

            private static List<Entity> TargetEntities(string[] args)
            {
                List<Entity> matches = new();

                if (args.Length == 0)
                {
                    Console.WriteLine("Cannot target entities without arguments.");
                    return null;
                }

                if (args.Length == 1)
                {
                    // must be player name
                    MinicraftServerThread thread = Game.server.GetAssociatedThread(args[0]);

                    if (thread != null)
                    {
                        matches.Add(thread.GetClient());
                    }

                    return matches;
                }

                // must specify @_ as first argument

                if (!args[0].StartsWith("@"))
                {
                    Console.WriteLine("Invalid entity targeting format. Please read help.");
                    return null;
                }

                string target = args[0][1..].ToLower(Localization.GetSelectedLocale()); // cut off "@"
                List<Entity> allEntities = new();

                if (args.Length == 2)
                {
                    // specified @_ level
                    try
                    {
                        allEntities.AddRange(Game.levels[int.Parse(args[1])].GetEntityArray());
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid entity targeting format: Specified level is not an integer: " + args[1]);
                        return null;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        Console.WriteLine("Invalid entity targeting format: Specified level does not exist: " + args[1]);
                        return null;
                    }
                }

                if (args.Length == 3)
                {
                    // @_ playername radius
                    MinicraftServerThread thread = Game.server.GetAssociatedThread(args[1]);

                    RemotePlayer rp = thread?.getClient();

                    if (rp == null)
                    {
                        Console.WriteLine("Invalid entity targeting format: Remote player does not exist: " + args[1]);
                        return null;
                    }

                    try
                    {
                        int radius = int.Parse(args[2]);

                        allEntities.AddRange(rp.GetLevel().GetEntitiesInRect(new Rectangle(rp.x, rp.y, radius * 2, radius * 2, Rectangle.CENTER_DIMS)));
                        allEntities.Remove(rp);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Invalid entity targeting format: Specified radius is not an integer: " + args[2]);
                        return null;
                    }
                }

                bool invert = false;

                if (target.StartsWith("!"))
                {
                    invert = true;
                    target = target[1..];
                }

                List<Entity> remainingEntities = new(allEntities);

                switch (target)
                {
                    case "all": break; // target all entities

                    case "entity": // target only non-mobs
                        allEntities.RemoveAll(entity => entity is Mob);
                        break;

                    case "mob": // target only mobs
                        allEntities.RemoveAll(entity => entity is not Mob);
                        break;

                    case "player": // target only players
                        allEntities.RemoveAll(entity => entity is not Player);
                        break;

                    default:
                        Console.WriteLine("Invalid entity targeting format: @_ argument is not valid: @" + target);
                        return null;
                }

                remainingEntities.RemoveAll(allEntities.Contains);

                if (invert)
                {
                    return remainingEntities;
                }

                return allEntities;
            }

            private class HelpCommand : Command
            {
                public HelpCommand()
                    : base("--all | [cmd]", "describes the function of each command. Specify a command name to read more about how to use it.", new[] { "no arguments: prints a list of all available commands, with a short description of each.", "cmd: a command name. will print the short description of that command, along with usage details such as what parameters/arguments it uses, and what function each argument has, and what the defualt behavior is if a given argument is ommitted.", "--all: prints the long description of all the commands.", "Usage symbol meanings:", "\t| = OR; specifies two possible choices for a given argument.", "\t[] = Optional; the arguments within may be specified, but they are not required.", "\t<> = Required; you must include the arguments within for the command to work.", "Note that the usage symbols may be nested, so a <> inside a [] is only required if you do whatever else is inside the []." })
                {
                }

                public override void Run(string[] args)
                {
                    if (args.Length == 0)
                    {
                        Console.WriteLine("Available commands:");

                        foreach (Command cmd in Command.All)
                        {
                            Console.WriteLine(cmd.GetGeneralHelp());
                        }

                        return;
                    }

                    Command query = GetCommandByName(args[0]); // prints its own error message if the command wasn't found.

                    if (query != null)
                    {
                        Console.WriteLine(query.GetDetailedHelp());
                    }
                }
            }

            private class StatusCommand : Command
            {
                public StatusCommand()
                    : base(null, "display some server stats.", new[] { "displays game version, server fps, and number of players connected." })
                {
                }

                public override void Run(string[] args)
                {
                    Console.WriteLine("Running " + Game.NAME + ' ' + Game.VERSION + (Game.debug ? " (debug mode)" : ""));
                    Console.WriteLine("Fps: " + Initializer.GetCurFps());
                    Console.WriteLine("Players connected: " + Game.server.GetNumPlayers());

                    foreach (string info in Game.server.GetClientInfo())
                    {
                        Console.WriteLine("\t" + info);
                    }
                }
            }

            private class ConfigCommand : Command
            {
                public ConfigCommand()
                    : base("[option_name [value]]", "change various server settings.", new[] { "no arguments: displays all config options and their current values", "option_name: displays the current value of that option", "option_name value:, will set the option to the specified value, provided it is a valid value for that option." })
                {
                }

                public override void Run(string[] args)
                {
                    if (args.Length == 0)
                    {
                        foreach (Config c in Config.All)
                        {
                            Console.WriteLine("\t" + c.Name + " = " + c.GetValue());
                        }
                    }
                    else
                    {
                        Config configOption = null;

                        if (!Config.TryGetValue(args[0].ToUpper(Localization.GetSelectedLocale())))
                        {
                            Console.WriteLine("\"" + args[0] + "\" is not a valid config option. run \"config\" for a list of the available config options.");
                        }

                        if (configOption == null)
                        {
                            return;
                        }

                        if (args.Length > 1)
                        { // we want to set the config option.
                            if (args.Length > 2)
                            {
                                Console.WriteLine("Note: Additional arguments (more than two) will be ignored.");
                            }

                            bool set = configOption.SetValue(args[1]);

                            if (set)
                            {
                                Console.WriteLine(configOption.Name + " set successfully.");
                                // HERE is where we save the modified config options.
                                new Save(WorldSelectDisplay.GetWorldName(), Game.server);
                                new Save();
                            }
                            else
                            {
                                Console.WriteLine("Failed to set " + configOption.Name);
                            }
                        }
                    }
                }
            }

            private class StopCommand : Command
            {
                public StopCommand()
                    : base(null, "close the server.")
                {
                }

                public override void Run(string[] args)
                {
                    Console.WriteLine("Shutting down server...");
                    Game.server.EndConnection();
                }
            }

            private class RestartCommand : Command
            {
                public RestartCommand()
                    : base(null, "restart the server.", "closes the server, then starts it back up again.")
                {
                }

                public override void Run(string[] args)
                {
                    STOP.Run(null); // shuts down the server.

                    try
                    {
                        Thread.Sleep(500); // give the computer some time to, uh, recuperate? idk, I think it's a good idea.
                    }
                    catch
                    {
                    }

                    Network.StartMultiplayerServer(); // start the server back up.
                }
            }

            private class SaveCommand : Command
            {
                public SaveCommand()
                    : base(null, "Save the world to file.")
                {
                }

                public override void Run(string[] args)
                {
                    Game.server.SaveWorld();
                    Console.WriteLine("World Saved.");
                }
            }

            private class GameModeCommand : Command
            {
                public GameModeCommand()
                    : base("<mode>", "change the server gamemode.", new[] { "mode: one of the following: c(reative), su(rvivial), t(imed) / score, h(ardcore)" })
                {
                }

                public override void Run(string[] args)
                {
                    if (args.Length != 1)
                    {
                        Console.WriteLine("Incorrect number of arguments. Please specify the game mode in one word:");
                        PrintHelp(this);
                        return;
                    }

                    switch (args[0].ToLower())
                    {
                        case "s":
                        case "survival":
                            Settings.Set("mode", "Survival");
                            break;

                        case "c":
                        case "creative":
                            Settings.Set("mode", "Creative");
                            break;

                        case "h":
                        case "hardcore":
                            Settings.Set("mode", "Hardcore");
                            break;

                        case "t":
                        case "timed":
                        case "score":
                            Settings.Set("mode", "Score");
                            break;

                        default:
                            Console.WriteLine(args[0] + " is not a valid game mode.");
                            PrintHelp(this);
                            break;
                    }

                    Game.server.UpdateGameVars();
                }
            }

            private class TimeCommand : Command
            {
                public TimeCommand()
                    : base("[timeString]", "sets or prints the time of day.", new[] { "no arguments: prints the current time of day, in ticks.", "timeString: sets the time of day to the given value; it can be a number, in which case it is a tick count from 0 to 64000 or so, or one of the following strings: Morning, Day, Evening, Night. the time of day will be set to the beginning of the given time period." })
                {
                }

                public override void Run(string[] args)
                {
                    if (args.Length == 0)
                    {
                        Console.WriteLine("Time of day is: " + Updater.tickCount + " (" + Updater.GetTime() + ")");
                        return;
                    }

                    int targetTicks = -1;

                    if (args[0].Length > 0)
                    {
                        try
                        {
                            string firstLetter = args[0][0].ToString().ToUpper();
                            string remainder = args[0].Substring(1).ToLower();

                            if (!Updater.Time.TryGetValue(firstLetter + remainder, out Updater.Time time))
                            {
                                throw new ArgumentException();
                            }

                            targetTicks = time.tickTime;
                        }
                        catch (ArgumentException)
                        {
                            try
                            {
                                targetTicks = int.Parse(args[0]);
                            }
                            catch (FormatException)
                            {
                            }
                        }
                    }

                    if (targetTicks >= 0)
                    {
                        Updater.SetTime(targetTicks);
                        Game.server.UpdateGameVars();
                    }
                    else
                    {
                        Console.WriteLine("Time specified is in an invalid format.");
                        Command.PrintHelp(this);
                    }
                }
            }

            private class MsgCommand : Command
            {
                public MsgCommand()
                    : base("[username] <message>", "make a message appear on other players' screens.", new[] { "w/o username: sends to all players,", "with username: sends to that player only." })
                {
                }

                public override void Run(string[] args)
                {
                    if (args.Length == 0)
                    {
                        Console.WriteLine("Please specify a message to send.");
                        return;
                    }

                    List<string> usernames = new();

                    if (args.Length > 1)
                    {
                        usernames.AddRange(args);
                    }
                    else
                    {
                        Game.server.BroadcastNotification(args[0], 50);

                        return;
                    }

                    string message = args[^1];

                    foreach (MinicraftServerThread clientThread in Game.server.GetAssociatedThreads(usernames, true))
                    {
                        clientThread.sendNotification(message, 50);
                    }
                }
            }

            private class TpCommand : Command
            {
                public TpCommand()
                    : base("<playername> <x y [level] | playername>", "teleports a player to a given location in the world.", new[] { "the first player name is the player that will be teleported. the second argument can be either another player, or a set of world coordinates.", "if the second argument is a player name, then the first player will be teleported to the second player, possibly traversing different levels.", "if world coordinates are specified, an x and y coordinate are required. A level depth may optionally be specified to go to a different level; if not specified, the current level is assumed.", "the symbol \"~\" may be used in place of an x or y coordinate, or a level, to mean the current player position on that axis. additionally, an offset may be specified by writing it like so: \"~-3 ~\". this means 3 tiles to the left of the current player position." })
                {
                }

                public override void Run(string[] args)
                {
                    if (args.Length == 0)
                    {
                        Console.WriteLine("You must specify a username, and coordinates or another username to teleport to.");
                        PrintHelp(this);
                        return;
                    }

                    MinicraftServerThread clientThread = Game.server.GetAssociatedThread(args[0]);

                    if (clientThread == null)
                    {
                        Console.WriteLine("Could not find player with username \"" + args[0] + "\"");
                        return;
                    }

                    int xt, yt;
                    Level level = clientThread.getClient().getLevel();

                    if (args.Length > 2)
                    {
                        try
                        {
                            xt = GetCoordinate(args[1], clientThread.getClient().x >> 4);
                            yt = GetCoordinate(args[2], clientThread.getClient().y >> 4);

                            if (args.Length == 4)
                            {
                                try
                                {
                                    int lvl = GetCoordinate(args[3], (level != null ? level.depth : 0));
                                    level = World.levels[World.LvlIdx(lvl)];
                                }
                                catch (FormatException)
                                {
                                    Console.WriteLine("Specified level index is not a number: " + args[3]);

                                    return;
                                }
                                catch (IndexOutOfRangeException)
                                {
                                    Console.WriteLine("Invalid level index: " + args[3]);

                                    return;
                                }
                            }
                        }
                        catch (FormatException ex)
                        {
                            Console.WriteLine("Invalid command syntax; specify a player or world coordinates for tp destination.");
                            PrintHelp(this);
                            return;
                        }
                    }
                    else
                    {
                        // user specified the username of another player to tp to.
                        MinicraftServerThread destClientThread = Game.server.GetAssociatedThread(args[1]);

                        if (destClientThread == null)
                        {
                            Console.WriteLine("Could not find player with username \"" + args[0] + "\" for tp destination.");

                            return;
                        }

                        RemotePlayer rp = destClientThread.GetClient();

                        if (rp == null)
                        {
                            Console.WriteLine("Client no longer exists...");

                            return;
                        }

                        xt = rp.x >> 4;
                        yt = rp.y >> 4;
                        level = rp.GetLevel();
                    }

                    if (xt >= 0 && yt >= 0 && level != null && xt < level.w && yt < level.h)
                    {
                        // perform teleport
                        RemotePlayer playerToMove = clientThread.GetClient();

                        if (playerToMove == null)
                        {
                            Console.WriteLine("Can't perform teleport; Client no longer exists...");

                            return;
                        }

                        if (!level.GetTile(xt, yt).MayPass(level, xt, yt, playerToMove))
                        {
                            Console.WriteLine("Specified tile is solid and cannot be moved though.");

                            return;
                        }

                        Level pLevel = playerToMove.GetLevel();

                        int nx = xt * 16 + 8;
                        int ny = yt * 16 + 8;

                        if (pLevel == null || pLevel.depth != level.depth)
                        {
                            playerToMove.Remove();
                            level.Add(playerToMove, nx, ny);
                        }
                        else
                        {
                            int oldxt = playerToMove.x >> 4;
                            int oldyt = playerToMove.y >> 4;
                            playerToMove.x = nx;
                            playerToMove.y = ny;
                            Game.server.BroadcastEntityUpdate(playerToMove, true);
                            playerToMove.UpdatePlayers(oldxt, oldyt);
                            playerToMove.UpdateSyncArea(oldxt, oldyt);
                        }

                        Console.WriteLine("Teleported player " + playerToMove.GetUsername() + " to tile coordinates " + xt + "," + yt + ", on level " + level.depth);
                    }
                    else
                    {
                        Console.WriteLine("Could not perform teleport; Coordinates are not valid...");
                    }
                }
            }

            private class PingCommand : Command
            {
                public PingCommand()
                    : base("", "Pings all the clients, and prints a message when each responds.")
                {
                }

                public override void Run(string[] args)
                {
                    Game.server.PingClients();
                }
            }

            private class KillCommand : Command
            {
                public KillCommand()
                    : base("<playername> | @[!]<all|entity|player|mob> <level | <playername> <radius>>", "Kills the specified entities.", new[] { "Specifying only a playername will kill that player.", "In the second form, use @all to refer to all entities, @entity to refer to all non-mob entities, @mob to refer to only mob entities, and @player to refer to all players.", "the \"!\" reverses the effect.", "@_ level will target all matching entities for that level.", "using a playername and radius will target all matching entities within the given radius of the player, the radius being a number of tiles." })
                {
                }

                public override void Run(string[] args)
                {
                    List<Entity> entities = TargetEntities(args);

                    if (entities == null)
                    {
                        PrintHelp(this);
                        return;
                    }

                    int count = entities.Count;

                    foreach (Entity e in entities)
                    {
                        e.Die();
                    }

                    Console.WriteLine("Removed " + count + " entities.");
                }
            }
        }

        private bool shouldRun;

        public ConsoleReader()
            : base("ConsoleReader")
        {
            shouldRun = true;
        }

        protected override void ThreadMain()
        {
            TextReader stdin = Console.In;
            try
            {
                Thread.Sleep(500); // this is to let it get past the debug statements at world load, and any others, maybe, if not in debug mode.
            }
            catch (ThreadInterruptedException)
            {
            }

            Console.WriteLine("Type \"help\" for a list of commands...");

            while (shouldRun/* && stdin.hasNext()*/)
            {
                Console.WriteLine();
                Console.Write("Enter a command: ");

                string command = stdin.ReadLine().Trim();

                if (command.Length == 0)
                {
                    continue;
                }

                List<string> parsed = new();

                parsed.AddRange(command.Split(" "));

                int lastIdx = -1;

                string removed;

                for (int i = 0; i < parsed.Count; i++)
                {
                    if (parsed[i].Contains("\""))
                    {
                        if (lastIdx >= 0)
                        { // closing a quoted String
                            while (i > lastIdx)
                            { // join the words together
                                removed = parsed[lastIdx + 1];
                                parsed.RemoveAt(lastIdx + 1);
                                parsed[lastIdx] = parsed[lastIdx] + " " + removed;
                                i--;
                            }

                            lastIdx = -1; // reset the "last quote" variable.
                        }
                        else // start the quoted String
                        {
                            lastIdx = i; // set the "last quote" variable.
                        }

                        parsed[i] = parsed[i].ReplaceFirst("\"", ""); // remove the parsed quote character from the string.
                        i--; // so that this string can be parsed again, in case there is another quote.
                    }
                }
                //if (Game.debug) Console.WriteLine("Parsed command: " + parsed.toString());

                removed = parsed[0];
                parsed.RemoveAt(0);

                Command cmd = GetCommandByName(removed); // will print its own error message if not found.

                if (cmd == null)
                {
                    Command.HELP.Run(new string[0]);
                }
                else if (Game.IsValidServer() || cmd == Command.HELP)
                {
                    cmd.Run(parsed.ToArray());
                }
                else
                {
                    Console.WriteLine("No server running.");
                }

                if (cmd == Command.STOP) shouldRun = false;
            }

            stdin.Close();
            Game.Quit();
        }

        public static Command GetCommandByName(string name)
        {
            if (!Command.TryGetValue(name.ToUpper(), out Command command))
            {
                Console.WriteLine("Unknown command: \"" + name + "\"");
            }

            return command;
        }
    }
}
