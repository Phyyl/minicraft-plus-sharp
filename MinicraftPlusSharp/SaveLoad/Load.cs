using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Core.IO;
using MinicraftPlusSharp.Entities;
using MinicraftPlusSharp.Entities.Furniture;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Entities.Particle;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MinicraftPlusSharp.SaveLoad
{
    public class Load
    {
        private string location = Game.gameDir;

        private static readonly string extension = Save.extension;
        private float percentInc;

        private List<string> data = new();
        private List<string> extradata = new(); // These two are changed when loading a new file. (see loadFromFile())

        private Version worldVer = null;
        private bool HasGlobalPrefs;

        public Load(string worldname)
            : this(worldname, true)
        {
        }

        public Load(string worldname, bool loadGame)
        {
            LoadFromFile(location + "/saves/" + worldname + "/Game" + extension);

            if (data[0].Contains("."))
            {
                worldVer = new Version(data[0]);
            }

            if (worldVer == null)
            {
                worldVer = new Version("1.8");
            }

            if (!HasGlobalPrefs)
            {
                HasGlobalPrefs = worldVer.CompareTo(new Version("1.9.2")) >= 0;
            }

            if (!loadGame)
            {
                return;
            }

            if (worldVer.CompareTo(new Version("1.9.2")) < 0)
            {
                new LegacyLoad(worldname);
            }
            else
            {
                location += "/saves/" + worldname + "/";
                percentInc = 5 + World.levels.length - 1; // for the methods below, and world.
                percentInc = 100f / percentInc;

                LoadingDisplay.SetPercentage(0);
                LoadGame("Game"); // more of the version will be determined here
                LoadWorld("Level");
                LoadEntities("Entities");
                LoadInventory("Inventory", Game.player.GetInventory());
                LoadPlayer("Player", Game.player);

                if (Game.IsMode("creative"))
                {
                    Items.Items.FillCreativeInv(Game.player.GetInventory(), false);
                }
            }
        }

        public Load(string worldname, MinicraftServer server)
        {
            location += "/saves/" + worldname + "/";

            FileInfo testFile = new FileInfo(location + "ServerConfig" + extension);

            if (testFile.Exists)
            {
                loadServerConfig("ServerConfig", server);
            }

            worldVer = null;

            data = new();
            extradata = new();

            File.Exists(location + "/Preferences" + extension);
        }

        public Load()
            : this(Game.VERSION)
        {
        }

        public Load(Version worldVersion)
            : this(false)
        {
            worldVer = worldVersion;
        }

        public Load(bool loadConfig)
        {
            if (!loadConfig)
            {
                return;
            }

            location += "/";

            if (HasGlobalPrefs)
            {
                LoadPrefs("Preferences");
            }
            else
            {
                new Save();
            }

            FileInfo testFileOld = new FileInfo(location + "unlocks" + extension);
            FileInfo testFile = new FileInfo(location + "Unlocks" + extension);

            if (testFileOld.Exists && !testFile.Exists)
            {
                testFileOld.MoveTo(testFile.FullName, true);

                new LegacyLoad(testFile);
            }
            else if (!testFile.Exists)
            {
                try
                {
                    testFile.Create();
                }
                catch (IOException ex)
                {
                    Console.Error.WriteLine("Could not create Unlocks" + extension + ":");
                    ex.PrintStackTrace();
                }
            }

            LoadUnlocks("Unlocks");
        }

        public Version GetWorldVersion()
        {
            return worldVer;
        }

        public static string[] LoadFile(string filename)
        {
            try
            {
                return File.ReadAllLines(filename);

            }
            catch
            {
                return new string[0];
            }
        }

        private void LoadFromFile(string filename)
        {
            data.Clear();
            extradata.Clear();

            string total;

            try
            {
                total = LoadFromFile(filename, true);
                if (total.Length > 0)
                {
                    data.AddRange(total.Split(","));
                }
            }
            catch (IOException ex)
            {
                ex.PrintStackTrace();
            }

            if (filename.Contains("Level"))
            {
                try
                {
                    total = Load.LoadFromFile(filename.Substring(0, filename.LastIndexOf("/") + 7) + "data" + extension, true);
                    extradata.AddRange(total.Split(","));
                }
                catch (IOException ex)
                {
                    ex.PrintStackTrace();
                }
            }

            LoadingDisplay.Progress(percentInc);
        }

        public static string LoadFromFile(string filename, bool isWorldSave)
        {
            StringBuilder total = new StringBuilder();

            try
            {
                StreamReader reader = new StreamReader(File.OpenRead(filename));

                string curLine;

                while ((curLine = reader.ReadLine()) != null)
                {
                    total.Append(curLine).Append(isWorldSave ? "" : "\n");
                }
            }
            catch { }

            return total.ToString();
        }



        private void LoadUnlocks(string filename)
        {
            LoadFromFile(location + filename + extension);

            foreach (string unlock in data)
            {
                string value = unlock;

                if (value.Equals("AirSkin"))
                {
                    Settings.Set("unlockedskin", true);
                }

                value = value.Replace("HOURMODE", "H_ScoreTime").Replace("MINUTEMODE", "M_ScoreTime").Replace("M_ScoreTime", "_ScoreTime").Replace("2H_ScoreTime", "120_ScoreTime");

                if (value.Contains("_ScoreTime"))
                {
                    Settings.GetEntry("scoretime").SetValueVisibility(int.Parse(value.Substring(0, value.IndexOf("_"))), true);
                }
            }
        }

        private void LoadGame(string filename)
        {
            LoadFromFile(location + filename + extension);

            worldVer = new Version(data.RemoveAtGet(0)); // gets the world version

            if (worldVer.CompareTo(new Version("2.0.4-dev8")) >= 0)
            {
                LoadMode(data.RemoveAtGet(0));
            }

            Updater.SetTime(int.Parse(data.RemoveAtGet(0)));

            Updater.gameTime = int.Parse(data.RemoveAtGet(0));

            if (worldVer.CompareTo(new Version("1.9.3-dev2")) >= 0)
            {
                Updater.pastDay1 = Updater.gameTime > 65000;
            }
            else
            {
                Updater.gameTime = 65000; // prevents time cheating.
            }

            int diffIdx = int.Parse(data.RemoveAtGet(0));
            
            if (worldVer.CompareTo(new Version("1.9.3-dev3")) < 0)
            {
                diffIdx--; // account for change in difficulty
            }

            Settings.SetIdx("diff", diffIdx);

            AirWizard.beaten = bool.Parse(data.RemoveAtGet(0));
        }

        public static BitmapImage[] LoadSpriteSheets()
        {
            BitmapImage[]
            images = new BitmapImage[4];

            FileInfo itemFile = new FileInfo(Game.gameDir + "/resources/items.png");
            if (itemFile.Exists)
            {
                images[0] = JavaImageIO.Read(itemFile.FullName);
            }
            FileInfo tileFile = new FileInfo(Game.gameDir + "/resources/tiles.png");
            if (tileFile.Exists)
            {
                images[1] = JavaImageIO.Read(tileFile.FullName);
            }
            FileInfo entityFile = new FileInfo(Game.gameDir + "/resources/entities.png");
            if (entityFile.Exists)
            {
                images[2] = JavaImageIO.Read(entityFile.FullName);
            }
            FileInfo guiFile = new FileInfo(Game.gameDir + "/resources/gui.png");
            if (guiFile.Exists)
            {
                images[3] = JavaImageIO.Read(guiFile.FullName);
            }
            return images;
        }

        private void LoadMode(string modedata)
        {
            int mode;

            if (modedata.Contains(";"))
            {
                string[] modeinfo = modedata.Split(";");

                mode = int.Parse(modeinfo[0]);

                if (worldVer.CompareTo(new Version("2.0.3")) <= 0)
                {
                    mode--; // we changed the min mode idx from 1 to 0.
                }

                if (mode == 3)
                {
                    Updater.scoreTime = int.Parse(modeinfo[1]);

                    if (worldVer.CompareTo(new Version("1.9.4")) >= 0)
                    {
                        Settings.Set("scoretime", modeinfo[2]);
                    }
                }
            }
            else
            {
                mode = int.Parse(modedata);

                if (worldVer.CompareTo(new Version("2.0.3")) <= 0)
                {
                    mode--; // we changed the min mode idx from 1 to 0.
                }

                if (mode == 3)
                {
                    Updater.scoreTime = 300;
                }
            }

            Settings.SetIdx("mode", mode);
        }

        private void LoadPrefs(string filename)
        {
            LoadFromFile(location + filename + extension);

            Version prefVer = new Version("2.0.2"); // the default, b/c this doesn't really matter much being specific past this if it's not set below.

            // TODO reformat the preferences file so that it uses key-value pairs. or json. JSON would be good.
            // TODO then, allow multiple saved accounts.
            // TODO do both of these in the same version (likely 2.0.5-dev1) because I also want to Make another iteration of LegacyLoad.

            if (!data.get(2).contains(";")) // signifies that this file was last written to by a version after 2.0.2.
                prefVer = new Version(data.remove(0));

            Settings.set("sound", bool.parsebool(data.remove(0)));
            Settings.set("autosave", bool.parsebool(data.remove(0)));

            if (prefVer.compareTo(new Version("2.0.4-dev2")) >= 0)
                Settings.set("fps", int.Parse(data.remove(0)));

            List<string> subdata;

            if (prefVer.compareTo(new Version("2.0.3-dev1")) < 0)
            {
                subdata = data;
            }
            else
            {
                MultiplayerDisplay.savedIP = data.remove(0);
                if (prefVer.compareTo(new Version("2.0.3-dev3")) > 0)
                {
                    MultiplayerDisplay.savedUUID = data.remove(0);
                    MultiplayerDisplay.savedUsername = data.remove(0);
                }

                if (prefVer.compareTo(new Version("2.0.4-dev3")) >= 0)
                {
                    string lang = data.remove(0);
                    Settings.set("language", lang);
                    Localization.changeLanguage(lang);
                }

                string keyData = data.get(0);
                subdata = Arrays.asList(keyData.split(":"));
            }

            for (string keymap : subdata)
            {
                string[] map = keymap.split(";");
                Game.input.setKey(map[0], map[1]);
            }
        }

        private void loadServerConfig(string filename, MinicraftServer server)
        {
            LoadFromFile(location + filename + extension);

            server.setPlayerCap(int.Parse(data.get(0)));
        }

        private void loadWorld(string filename)
        {
            for (int l = World.maxLevelDepth; l >= World.minLevelDepth; l--)
            {
                LoadingDisplay.setMessage(Level.getDepthString(l));
                int lvlidx = World.lvlIdx(l);
                LoadFromFile(location + filename + lvlidx + extension);

                int lvlw = int.Parse(data.get(0));
                int lvlh = int.Parse(data.get(1));

                bool hasSeed = worldVer.compareTo(new Version("2.0.7-dev2")) >= 0;
                long seed = hasSeed ? Long.parseLong(data.get(2)) : 0;
                Settings.set("size", lvlw);

                byte[] tiles = new byte[lvlw * lvlh];
                byte[] tdata = new byte[lvlw * lvlh];

                for (int x = 0; x < lvlw; x++)
                {
                    for (int y = 0; y < lvlh; y++)
                    {
                        int tileArrIdx = y + x * lvlw;
                        int tileidx = x + y * lvlw; // the tiles are saved with x outer loop, and y inner loop, meaning that the list reads down, then right one, rather than right, then down one.
                        string tilename = data.get(tileidx + (hasSeed ? 4 : 3));
                        if (worldVer.compareTo(new Version("1.9.4-dev6")) < 0)
                        {
                            int tileID = int.Parse(tilename); // they were id numbers, not names, at this point
                            if (Tiles.oldids.get(tileID) != null)
                                tilename = Tiles.oldids.get(tileID);
                            else
                            {
                                Console.WriteLine("Tile list doesn't contain tile " + tileID);
                                tilename = "grass";
                            }
                        }

                        if (tilename.equalsIgnoreCase("WOOL") && worldVer.compareTo(new Version("2.0.6-dev4")) < 0)
                        {
                            switch (int.Parse(extradata.get(tileidx)))
                            {
                                case 1:
                                    tilename = "Red Wool";
                                    break;
                                case 2:
                                    tilename = "Yellow Wool";
                                    break;
                                case 3:
                                    tilename = "Green Wool";
                                    break;
                                case 4:
                                    tilename = "Blue Wool";
                                    break;
                                case 5:
                                    tilename = "Black Wool";
                                    break;
                                default:
                                    tilename = "Wool";
                            }
                        }

                        if (l == World.minLevelDepth + 1 && tilename.equalsIgnoreCase("LAPIS") && worldVer.compareTo(new Version("2.0.3-dev6")) < 0)
                        {
                            if (Math.random() < 0.8) // don't replace *all* the lapis
                                tilename = "Gem Ore";
                        }
                        tiles[tileArrIdx] = Tiles.get(tilename).id;
                        tdata[tileArrIdx] = byte.parseByte(extradata.get(tileidx));
                    }
                }

                Level parent = World.levels[World.lvlIdx(l + 1)];
                World.levels[lvlidx] = new Level(lvlw, lvlh, seed, l, parent, false);

                Level curLevel = World.levels[lvlidx];
                curLevel.tiles = tiles;
                curLevel.data = tdata;

                if (Game.debug) curLevel.printTileLocs(Tiles.get("Stairs Down"));

                if (parent == null) continue;
                /// confirm that there are stairs in all the places that should have stairs.
                for (minicraft.gfx.Point p: parent.getMatchingTiles(Tiles.get("Stairs Down")))
                {
                    if (curLevel.getTile(p.x, p.y) != Tiles.get("Stairs Up"))
                    {
                        curLevel.printLevelLoc("INCONSISTENT STAIRS detected; placing stairsUp", p.x, p.y);
                        curLevel.setTile(p.x, p.y, Tiles.get("Stairs Up"));
                    }
                }
                for (minicraft.gfx.Point p: curLevel.getMatchingTiles(Tiles.get("Stairs Up")))
                {
                    if (parent.getTile(p.x, p.y) != Tiles.get("Stairs Down"))
                    {
                        parent.printLevelLoc("INCONSISTENT STAIRS detected; placing stairsDown", p.x, p.y);
                        parent.setTile(p.x, p.y, Tiles.get("Stairs Down"));
                    }
                }
            }
        }

        public void loadPlayer(string filename, Player player)
        {
            LoadingDisplay.setMessage("Player");
            LoadFromFile(location + filename + extension);
            loadPlayer(player, data);
        }
        public void loadPlayer(Player player, List<string> origData)
        {
            List<string> data = new(origData);
            player.x = int.Parse(data.remove(0));
            player.y = int.Parse(data.remove(0));
            player.spawnx = int.Parse(data.remove(0));
            player.spawny = int.Parse(data.remove(0));
            player.health = int.Parse(data.remove(0));
            if (worldVer.compareTo(new Version("2.0.4-dev7")) >= 0)
                player.hunger = int.Parse(data.remove(0));
            player.armor = int.Parse(data.remove(0));

            if (worldVer.compareTo(new Version("2.0.5-dev5")) >= 0 || player.armor > 0 || worldVer.compareTo(new Version("2.0.5-dev4")) == 0 && data.size() > 5)
            {
                if (worldVer.compareTo(new Version("2.0.4-dev7")) < 0)
                {
                    // reverse order b/c we are taking from the end
                    player.curArmor = (ArmorItem)Items.get(data.remove(data.size() - 1));
                    player.armorDamageBuffer = int.Parse(data.remove(data.size() - 1));
                }
                else
                {
                    player.armorDamageBuffer = int.Parse(data.remove(0));
                    player.curArmor = (ArmorItem)Items.get(data.remove(0), true);
                }
            }
            player.setScore(int.Parse(data.remove(0)));

            if (worldVer.compareTo(new Version("2.0.4-dev7")) < 0)
            {
                int arrowCount = int.Parse(data.remove(0));
                if (worldVer.compareTo(new Version("2.0.1-dev1")) < 0)
                    player.getInventory().add(Items.get("arrow"), arrowCount);
            }

            Game.currentLevel = int.Parse(data.remove(0));
            Level level = World.levels[Game.currentLevel];
            if (!player.isRemoved()) player.remove(); // removes the user player from the level, in case they would be added twice.
            if (!Game.IsValidServer() || player != Game.player)
            {
                if (level != null)
                    level.add(player);
                else if (Game.debug)
                    Console.WriteLine(Network.OnlinePrefix() + "game level to add player " + player + " to is null.");
            }

            if (worldVer.compareTo(new Version("2.0.4-dev8")) < 0)
            {
                string modedata = data.remove(0);
                if (player == Game.player)
                    LoadMode(modedata); // only load if you're loading the main player
            }

            string potioneffects = data.remove(0);
            if (!potioneffects.equals("PotionEffects[]"))
            {
                string[] effects = potioneffects.replace("PotionEffects[", "").replace("]", "").split(":");

                for (string s : effects)
                {
                    string[] effect = s.split(";");
                    PotionType pName = Enum.valueOf(PotionType.GetType(), effect[0]);
                    PotionItem.applyPotion(player, pName, int.Parse(effect[1]));
                }
            }

            if (worldVer.compareTo(new Version("1.9.4-dev4")) < 0)
            {
                string colors = data.remove(0).replace("[", "").replace("]", "");
                string[] color = colors.split(";");
                int[] cols = new int[color.length];
                for (int i = 0; i < cols.length; i++)
                    cols[i] = int.Parse(color[i]) / 50;

                string col = "" + cols[0] + cols[1] + cols[2];
                Console.WriteLine("Getting color as " + col);
                player.shirtColor = int.Parse(col);
            }
            else if (worldVer.compareTo(new Version("2.0.6-dev4")) < 0)
            {
                string color = data.remove(0);
                int[] colors = new int[3];
                for (int i = 0; i < 3; i++)
                    colors[i] = int.Parse(string.valueOf(color.charAt(i)));
                player.shirtColor = Color.get(1, colors[0] * 51, colors[1] * 51, colors[2] * 51);
            }
            else
                player.shirtColor = int.Parse(data.remove(0));

            player.skinon = bool.parsebool(data.remove(0));
        }

        protected static string subOldName(string name, Version worldVer)
        {
            if (worldVer.compareTo(new Version("1.9.4-dev4")) < 0)
            {
                name = name.replace("Hatchet", "Axe").replace("Pick", "Pickaxe").replace("Pickaxeaxe", "Pickaxe").replace("Spade", "Shovel").replace("Pow glove", "Power Glove").replace("II", "").replace("W.Bucket", "Water Bucket").replace("L.Bucket", "Lava Bucket").replace("G.Apple", "Gold Apple").replace("St.", "Stone").replace("Ob.", "Obsidian").replace("I.Lantern", "Iron Lantern").replace("G.Lantern", "Gold Lantern").replace("BrickWall", "Wall").replace("Brick", " Brick").replace("Wall", " Wall").replace("  ", " ");
                if (name.equals("Bucket"))
                    name = "Empty Bucket";
            }

            if (worldVer.compareTo(new Version("1.9.4")) < 0)
            {
                name = name.replace("I.Armor", "Iron Armor").replace("S.Armor", "Snake Armor").replace("L.Armor", "Leather Armor").replace("G.Armor", "Gold Armor").replace("BrickWall", "Wall");
            }

            if (worldVer.compareTo(new Version("2.0.6-dev3")) < 0)
            {
                name = name.replace("Fishing Rod", "Wood Fishing Rod");
            }

            // Only runs if the version is less than 2.0.7-dev1.
            if (worldVer.compareTo(new Version("2.0.7-dev1")) < 0)
            {
                if (name.startsWith("Seeds"))
                    name = name.replace("Seeds", "Wheat Seeds");
            }

            return name;
        }

        public void loadInventory(string filename, Inventory inventory)
        {
            LoadFromFile(location + filename + extension);
            loadInventory(inventory, data);
        }
        public void loadInventory(Inventory inventory, List<string> data)
        {
            inventory.clearInv();

            for (string item : data)
            {
                if (item.length() == 0)
                {
                    Console.Error.WriteLine("loadInventory: Item in data list is \"\", skipping item");
                    continue;
                }

                if (worldVer.compareTo(new Version("2.0.7-dev1")) < 0)
                {
                    item = subOldName(item, worldVer);
                }

                if (item.contains("Power Glove")) continue; // just pretend it doesn't exist. Because it doesn't. :P

                // Console.WriteLine("Loading item: " + item);

                if (worldVer.compareTo(new Version("2.0.4")) <= 0 && item.contains(";"))
                {
                    string[] curData = item.split(";");
                    string itemName = curData[0];

                    Item newItem = Items.get(itemName);

                    int count = int.Parse(curData[1]);

                    if (newItem is StackableItem)
                    {
                        ((StackableItem)newItem).count = count;
                        inventory.add(newItem);
                    }
                    else inventory.add(newItem, count);
                }
                else
                {
                    Item toAdd = Items.get(item);
                    inventory.add(toAdd);
                }
            }
        }

        private void loadEntities(string filename)
        {
            LoadingDisplay.setMessage("Entities");
            LoadFromFile(location + filename + extension);

            for (int i = 0; i < World.levels.length; i++)
            {
                World.levels[i].clearEntities();
            }
            for (string name : data)
            {
                if (name.startsWith("Player")) continue;
                loadEntity(name, worldVer, true);
            }

            for (int i = 0; i < World.levels.length; i++)
            {
                World.levels[i].checkChestCount();
                World.levels[i].checkAirWizard();
            }
        }


        public static Entity loadEntity(string entityData, bool isLocalSave)
        {
            if (isLocalSave) Console.WriteLine("Warning: Assuming version of save file is current while loading entity: " + entityData);
            return Load.loadEntity(entityData, Game.VERSION, isLocalSave);
        }

        public static Entity loadEntity(string entityData, Version worldVer, bool isLocalSave)
        {
            entityData = entityData.trim();
            if (entityData.length() == 0) return null;

            string[] stuff = entityData.substring(entityData.indexOf("[") + 1, entityData.indexOf("]")).split(":"); // this gets everything inside the "[...]" after the entity name.
            List<string> info = new(Arrays.asList(stuff));

            string entityName = entityData.substring(0, entityData.indexOf("[")); // this gets the text before "[", which is the entity name.

            if (entityName.equals("Player") && Game.debug && Game.IsValidClient())
                Console.WriteLine("CLIENT WARNING: Loading regular player: " + entityData);

            int x = int.Parse(info.get(0));
            int y = int.Parse(info.get(1));

            int eid = -1;
            if (!isLocalSave)
            {
                eid = int.Parse(info.remove(2));

                // If I find an entity that is loaded locally, but on another level in the entity data provided, then I ditch the current entity and make a new one from the info provided.
                Entity existing = Network.GetEntity(eid);
                int entityLevel = int.Parse(info.get(info.size() - 1));

                if (existing != null)
                {
                    // existing one is out of date; replace it.
                    existing.remove();
                    Game.levels[Game.currentLevel].add(existing);
                    return null;
                }

                if (Game.IsValidClient())
                {
                    if (eid == Game.player.eid)
                        return Game.player; // don't reload the main player via an entity addition, though do add it to the level (will be done elsewhere)
                    if (Game.player is RemotePlayer &&
                        !((RemotePlayer)Game.player).shouldTrack(x >> 4, y >> 4, World.levels[entityLevel])
                        )
                    {
                        // the entity is too far away to bother adding to the level.
                        if (Game.debug) Console.WriteLine("CLIENT: Entity is too far away to bother loading: " + eid);
                        Entity dummy = new Cow();
                        dummy.eid = eid;
                        return dummy; /// we need a dummy b/c it's the only way to pass along to entity id.
                    }
                }
            }

            Entity newEntity = null;

            if (entityName.equals("RemotePlayer"))
            {
                if (isLocalSave)
                {
                    Console.Error.WriteLine("Remote player found in local save file.");
                    return null; // don't load them; in fact, they shouldn't be here.
                }
                string username = info.get(2);
                java.net.InetAddress ip;
                try
                {
                    ip = java.net.InetAddress.getByName(info.get(3));
                    int port = int.Parse(info.get(4));
                    newEntity = new RemotePlayer(null, ip, port);
                    ((RemotePlayer)newEntity).setUsername(username);
                    if (Game.debug) Console.WriteLine("Prob CLIENT: Loaded remote player");
                }
                catch (java.net.UnknownHostException ex)
                {
                    Console.Error.WriteLine("LOAD could not read ip address of remote player in file.");
                    ex.printStackTrace();
                }
            }
            else if (entityName.equals("Spark") && !isLocalSave)
            {
                int awID = int.Parse(info.get(2));
                Entity sparkOwner = Network.GetEntity(awID);
                if (sparkOwner is AirWizard)
                    newEntity = new Spark((AirWizard)sparkOwner, x, y);

                else
                {
                    Console.Error.WriteLine("failed to load spark; owner id doesn't point to a correct entity");
                    return null;
                }
            }
            else
            {
                int mobLvl = 1;
                Class c = null;
                if (!Crafter.names.contains(entityName))
                {
                    try
                    {
                        c = Class.forName("minicraft.entity.mob." + entityName);
                    }
                    catch (ClassNotFoundException ignored) { }
                }

                newEntity = getEntity(entityName.substring(entityName.lastIndexOf(".") + 1), mobLvl);
            }

            if (newEntity == null)
                return null;

            if (newEntity is Mob && !(newEntity is RemotePlayer))
            { // This is structured the same way as in Save.java.
                Mob mob = (Mob)newEntity;
                mob.health = int.Parse(info.get(2));

                Class c = null;
                try
                {
                    c = Class.forName("minicraft.entity.mob." + entityName);
                }
                catch (ClassNotFoundException e)
                {
                    e.printStackTrace();
                }

                if (EnemyMob.GetType().isAssignableFrom(c))
                {
                    EnemyMob enemyMob = ((EnemyMob)mob);
                    enemyMob.lvl = int.Parse(info.get(info.size() - 2));

                    if (enemyMob.lvl == 0)
                    {
                        if (Game.debug) Console.WriteLine("Level 0 mob: " + entityName);
                        enemyMob.lvl = 1;
                    }
                    else if (enemyMob.lvl > enemyMob.getMaxLevel())
                    {
                        enemyMob.lvl = enemyMob.getMaxLevel();
                    }

                    mob = enemyMob;
                }
                else if (worldVer.compareTo(new Version("2.0.7-dev1")) >= 0)
                { // If the version is more or equal to 2.0.7-dev1
                    if (newEntity is Sheep)
                    {
                        Sheep sheep = ((Sheep)mob);
                        if (info.get(3).equalsIgnoreCase("true"))
                        {

                            sheep.cut = true;
                        }

                        mob = sheep;
                    }
                }

                newEntity = mob;
            }
            else if (newEntity is Chest)
            {
                Chest chest = (Chest)newEntity;
                bool isDeathChest = chest is DeathChest;
                bool isDungeonChest = chest is DungeonChest;
                List<string> chestInfo = info.subList(2, info.size() - 1);

                int endIdx = chestInfo.size() - (isDeathChest || isDungeonChest ? 1 : 0);
                for (int idx = 0; idx < endIdx; idx++)
                {
                    string itemData = chestInfo.get(idx);
                    if (worldVer.compareTo(new Version("2.0.7-dev1")) < 0)
                        itemData = subOldName(itemData, worldVer);

                    if (itemData.contains("Power Glove")) continue; // ignore it.

                    Item item = Items.get(itemData);
                    chest.getInventory().add(item);
                }

                if (isDeathChest)
                {
                    ((DeathChest)chest).time = int.Parse(chestInfo.get(chestInfo.size() - 1));
                }
                else if (isDungeonChest)
                {
                    ((DungeonChest)chest).setLocked(bool.parsebool(chestInfo.get(chestInfo.size() - 1)));
                    if (((DungeonChest)chest).isLocked()) World.levels[int.Parse(info.get(info.size() - 1))].chestCount++;
                }

                newEntity = chest;
            }
            else if (newEntity is Spawner)
            {
                MobAi mob = (MobAi)getEntity(info.get(2).substring(info.get(2).lastIndexOf(".") + 1), int.Parse(info.get(3)));
                if (mob != null)
                    newEntity = new Spawner(mob);
            }
            else if (newEntity is Lantern && worldVer.compareTo(new Version("1.9.4")) >= 0 && info.size() > 3)
            {
                newEntity = new Lantern(Lantern.Type.values()[int.Parse(info.get(2))]);
            }

            if (!isLocalSave)
            {
                if (newEntity is Arrow)
                {
                    int ownerID = int.Parse(info.get(2));
                    Mob m = (Mob)Network.GetEntity(ownerID);
                    if (m != null)
                    {
                        Direction dir = Direction.values[int.Parse(info.get(3))];
                        int dmg = int.Parse(info.get(5));
                        newEntity = new Arrow(m, x, y, dir, dmg);
                    }
                }
                if (newEntity is ItemEntity)
                {
                    Item item = Items.get(info.get(2));
                    double zz = double.Parse(info.get(3));
                    int lifetime = int.Parse(info.get(4));
                    int timeleft = int.Parse(info.get(5));
                    double xa = double.Parse(info.get(6));
                    double ya = double.Parse(info.get(7));
                    double za = double.Parse(info.get(8));
                    newEntity = new ItemEntity(item, x, y, zz, lifetime, timeleft, xa, ya, za);
                }
                if (newEntity is TextParticle)
                {
                    int textcol = int.Parse(info.get(3));
                    newEntity = new TextParticle(info.get(2), x, y, textcol);
                    //if (Game.debug) Console.WriteLine("Loaded text particle; color: "+Color.toString(textcol)+", text: " + info.get(2));
                }
            }

            newEntity.eid = eid; // this will be -1 unless set earlier, so a new one will be generated when adding it to the level.
            if (newEntity is ItemEntity && eid == -1)
                Console.WriteLine("Warning: Item entity was loaded with no eid");

            int curLevel = int.Parse(info.get(info.size() - 1));
            if (World.levels[curLevel] != null)
            {
                World.levels[curLevel].add(newEntity, x, y);
                if (Game.debug && newEntity is RemotePlayer)
                    World.levels[curLevel].printEntityStatus("Loaded ", newEntity, "mob.RemotePlayer");
            }
            else if (newEntity is RemotePlayer && Game.IsValidClient())
                Console.WriteLine("CLIENT: Remote player not added because on null level");

            return newEntity;
        }

        private static Entity GetEntity(string @string, int moblvl)
        {
            switch (@string)
            {
                case "Player": return null;
                case "RemotePlayer": return null;
                case "Cow": return new Cow();
                case "Sheep": return new Sheep();
                case "Pig": return new Pig();
                case "Zombie": return new Zombie(moblvl);
                case "Slime": return new Slime(moblvl);
                case "Creeper": return new Creeper(moblvl);
                case "Skeleton": return new Skeleton(moblvl);
                case "Knight": return new Knight(moblvl);
                case "Snake": return new Snake(moblvl);
                case "AirWizard": return new AirWizard(moblvl > 1);
                case "Spawner": return new Spawner(new Zombie(1));
                case "Workbench": return new Crafter(Crafter.Type.Workbench);
                case "Chest": return new Chest();
                case "DeathChest": return new DeathChest();
                case "DungeonChest": return new DungeonChest(false);
                case "Anvil": return new Crafter(Crafter.Type.Anvil);
                case "Enchanter": return new Crafter(Crafter.Type.Enchanter);
                case "Loom": return new Crafter(Crafter.Type.Loom);
                case "Furnace": return new Crafter(Crafter.Type.Furnace);
                case "Oven": return new Crafter(Crafter.Type.Oven);
                case "Bed": return new Bed();
                case "Tnt": return new Tnt();
                case "Lantern": return new Lantern(Lantern.Type.NORM);
                case "Arrow": return new Arrow(new Skeleton(0), 0, 0, Direction.NONE, 0);
                case "ItemEntity": return new ItemEntity(Items.Items.Get("unknown"), 0, 0);
                case "FireParticle": return new FireParticle(0, 0);
                case "SmashParticle": return new SmashParticle(0, 0);
                case "TextParticle": return new TextParticle("", 0, 0, 0);
                default:
                    Console.Error.WriteLine("LOAD ERROR: Unknown or outdated entity requested: " + @string);
                    return null;
            }
        }
    }
}
