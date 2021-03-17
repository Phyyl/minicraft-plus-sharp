using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Entities;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Levels;
using MinicraftPlusSharp.SaveLoad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Collections.Concurrent;
using System.Collections;
using System.Runtime.CompilerServices;
using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.Levels.Tiles;

namespace MinicraftPlusSharp.Levels
{

    public class Level
    {
        private JavaRandom random = new JavaRandom();

        private static readonly string[] levelNames = new[] { "Sky", "Surface", "Iron", "Gold", "Lava", "Dungeon" };

        public static string GetLevelName(int depth)
        {
            return levelNames[-1 * depth + 1];
        }
        public static string GetDepthString(int depth)
        {
            return "Level " + (depth < 0 ? "B" + (-depth) : depth);
        }

        private static readonly int MOB_SPAWN_FACTOR = 100; // the chance of a mob actually trying to spawn when trySpawn is called equals: mobCount / maxMobCount * MOB_SPAWN_FACTOR. so, it basically equals the chance, 1/number, of a mob spawning when the mob cap is reached. I hope that makes params sense[] 

        public int w, h; // width and height of the level
        private long seed; // the used seed that was used to generate the world

        public byte[] tiles; // an array of all the tiles in the world.
        public byte[] data; // an array of the data of the tiles in the world.

        public readonly int depth; // depth level of the level
        public int monsterDensity = 16; // affects the number of monsters that are on the level, bigger the number the less monsters spawn.
        public int maxMobCount;
        public int chestCount;
        public int mobCount = 0;

        /**
		 * I will be using this lock to avoid concurrency exceptions in entities and sparks set
		 */
        private readonly object entityLock = new object();
        private List<Entity> entities = new(); // A list of all the entities in the world
        private List<Spark> sparks = new(); // A list of all the sparks in the world
        private List<Player> players = new(); // A list of all the players in the world
        private List<Entity> entitiesToAdd = new(); /// entities that will be added to the level on next tick are stored here. This is for the sake of multithreading optimization. (hopefully)
        private List<Entity> entitiesToRemove = new(); /// entities that will be removed from the level on next tick are stored here. This is for the sake of multithreading optimization. (hopefully)
        // creates a sorter for all the entities to be rendered.
        private static Comparer<Entity> spriteSorter = new EntityComparer();

        public Entity[] GetEntitiesToSave()
        {
            Entity[] allEntities = new Entity[entities.Count + sparks.Count + entitiesToAdd.Count];
            Entity[] toAdd = entitiesToAdd.ToArray();
            Entity[] current = GetEntityArray();

            Array.Copy(current, 0, allEntities, 0, current.Length);
            Array.Copy(toAdd, 0, allEntities, current.Length, toAdd.Length);

            return allEntities;
        }

        // This is a solely debug method I made, to make printing repetitive stuff easier.
        // should be changed to accept prepend and entity, or a tile (as an Object). It will get the coordinates and class name from the object, and will divide coords by 16 if passed an entity.
        public void PrintLevelLoc(string prefix, int x, int y)
        {
            PrintLevelLoc(prefix, x, y, "");
        }

        public void PrintLevelLoc(string prefix, int x, int y, string suffix)
        {
            string levelName = GetLevelName(depth);

            Console.WriteLine(prefix + " on " + levelName + " level (" + x + "," + y + ")" + suffix);
        }

        public void PrintTileLocs(Tile t)
        {
            for (int x = 0; x < w; x++)
            {
                {
                    for (int y = 0; y < h; y++)
                    {
                        if (GetTile(x, y).id == t.id)
                        {
                            PrintLevelLoc(t.name, x, y);
                        }

                    }
                }
            }
        }

        public void PrintEntityLocs<T>() where T : Entity
        {
            int numfound = 0;

            foreach (Entity entity in GetEntityArray())
            {
                if (entity is T t)
                {
                    PrintLevelLoc(entity.ToString(), entity.x >> 4, entity.y >> 4);
                    numfound++;
                }
            }

            Console.WriteLine("Found " + numfound + " entities in level of depth " + depth);
        }

        private void UpdateMobCap()
        {
            maxMobCount = 150 + 150 * Settings.GetIdx("diff");

            if (depth == 1)
            {
                maxMobCount /= 2;
            }

            if (depth == 0 || depth == -4)
            {
                maxMobCount = maxMobCount * 2 / 3;
            }
        }

        public Level(int w, int h, long seed, int level, Level parentLevel, bool makeWorld)
        {
            depth = level;
            this.w = w;
            this.h = h;
            this.seed = seed;
            byte[][] maps; // multidimensional array (an array within a array), used for the map

            if (level != -4 && level != 0)
            {
                monsterDensity = 8;
            }

            UpdateMobCap();

            if (!makeWorld)
            {
                int arrsize = w * h;
                tiles = new byte[arrsize];
                data = new byte[arrsize];
                return;
            }

            if (Game.debug)
            {
                Console.WriteLine("Making level " + level + "...");
            }

            maps = LevelGen.CreateAndValidateMap(w, h, level);

            if (maps == null)
            {
                Console.WriteLine("Level Gen ERROR: Returned maps array is null");
                return;
            }

            tiles = maps[0]; // assigns the tiles in the map
            data = maps[1]; // assigns the data of the tiles

            if (level < 0)
            {
                GenerateSpawnerStructures();
            }

            if (level == 0)
            {
                GenerateVillages();
            }

            if (parentLevel != null)
            { // If the level above this one is not null (aka, if this isn't the sky level)
                for (int y = 0; y < h; y++)
                { // loop through height
                    for (int x = 0; x < w; x++)
                    { // loop through width
                        if (parentLevel.GetTile(x, y) == Tiles.Tiles.Get("Stairs Down"))
                        { // If the tile in the level above the current one is a stairs down params then[] 
                            if (level == -4) /// make the obsidian wall formation around the stair in the dungeon level
                            {
                                Structure.dungeonGate.Draw(this, x, y);
                            }
                            else if (level == 0)
                            { // surface
                                if (Game.debug)
                                {
                                    Console.WriteLine("Setting tiles around " + x + "," + y + " to hard rock");
                                }

                                SetAreaTiles(x, y, 1, Tiles.Tiles.Get("Hard Rock"), 0); // surround the sky stairs with hard rock
                            }
                            else // any other level, the up-stairs should have dirt on all sides.
                            {
                                SetAreaTiles(x, y, 1, Tiles.Tiles.Get("dirt"), 0);
                            }

                            SetTile(x, y, Tiles.Tiles.Get("Stairs Up")); // set a stairs up tile in the same position on the current level
                        }
                    }
                }
            }
            else
            { // this is the sky level
                bool placedHouse = false;

                while (!placedHouse)
                {
                    int x = random.NextInt(this.w - 7);
                    int y = random.NextInt(this.h - 5);

                    if (this.GetTile(x - 3, y - 2) == Tiles.Tiles.Get("Cloud") && this.GetTile(x + 3, y - 2) == Tiles.Tiles.Get("Cloud"))
                    {
                        if (this.GetTile(x - 3, y + 2) == Tiles.Tiles.Get("Cloud") && this.GetTile(x + 3, y + 2) == Tiles.Tiles.Get("Cloud"))
                        {
                            Structure.airWizardHouse.Draw(this, x, y);
                            placedHouse = true;
                        }
                    }
                }
            }

            CheckChestCount(false);

            CheckAirWizard();

            if (Game.debug)
            {
                PrintTileLocs(Tiles.Tiles.Get("Stairs Down"));
            }
        }

        public Level(int w, int h, int level, Level parentLevel, bool makeWorld)
            : this(w, h, 0, level, parentLevel, makeWorld)
        {
        }

        /** Level which the world is contained in */
        public Level(int w, int h, int level, Level parentLevel)
            : this(w, h, level, parentLevel, true)
        {
        }

        public long GetSeed()
        {
            return seed;
        }

        public void CheckAirWizard()
        {
            CheckAirWizard(true);
        }

        private void CheckAirWizard(bool check)
        {
            if (depth == 1 && !AirWizard.beaten)
            { // Add the airwizard to the surface

                bool found = false;

                if (check)
                {
                    foreach (Entity e in entitiesToAdd)
                    {
                        if (e is AirWizard)
                        {
                            found = true;
                        }
                    }

                    foreach (Entity e in entities)
                    {
                        if (e is AirWizard)
                        {
                            found = true;
                        }
                    }
                }

                if (!found)
                {
                    AirWizard aw = new AirWizard(false);
                    Add(aw, w / 2, h / 2, true);
                }
            }
        }

        public void CheckChestCount()
        {
            CheckChestCount(true);
        }

        private void CheckChestCount(bool check)
        {
            // If the level is the dungeon, and we're not just loading the params world[] 
            if (depth != -4)
            {
                return;
            }

            int numChests = 0;

            if (check)
            {
                foreach (Entity e in entitiesToAdd)
                {
                    if (e is DungeonChest)
                    {
                        numChests++;
                    }
                }

                foreach (Entity e in entities)
                {
                    if (e is DungeonChest)
                    {
                        numChests++;
                    }
                }

                if (Game.debug)
                {
                    Console.WriteLine("Found " + numChests + " chests.");
                }
            }

            /// make DungeonChests!
            for (int i = numChests; i < 10 * (w / 128); i++)
            {
                DungeonChest d = new DungeonChest(true);

                bool addedchest = false;

                while (!addedchest)
                { // keep running until we successfully add a DungeonChest
                  //pick a random tile:
                    int x2 = random.NextInt(16 * w) / 16;
                    int y2 = random.NextInt(16 * h) / 16;

                    if (GetTile(x2, y2) == Tiles.Tiles.Get("Obsidian"))
                    {
                        bool xaxis = random.NextBool();

                        if (xaxis)
                        {
                            for (int s = x2; s < w - s; s++)
                            {
                                if (GetTile(s, y2) == Tiles.Tiles.Get("Obsidian Wall"))
                                {
                                    d.x = s * 16 - 24;
                                    d.y = y2 * 16 - 24;
                                }
                            }
                        }
                        else
                        { // y axis
                            for (int s = y2; s < y2 - s; s++)
                            {
                                if (GetTile(x2, s) == Tiles.Tiles.Get("Obsidian Wall"))
                                {
                                    d.x = x2 * 16 - 24;
                                    d.y = s * 16 - 24;
                                }
                            }
                        }

                        if (d.x == 0 && d.y == 0)
                        {
                            d.x = x2 * 16 - 8;
                            d.y = y2 * 16 - 8;
                        }

                        if (GetTile(d.x / 16, d.y / 16) == Tiles.Tiles.Get("Obsidian Wall"))
                        {
                            SetTile(d.x / 16, d.y / 16, Tiles.Tiles.Get("Obsidian"));
                        }

                        Add(d);

                        chestCount++;
                        addedchest = true;
                    }
                }
            }
        }

        private void TickEntity(Entity entity)
        {
            if (entity == null)
            {
                return;
            }

            if (Game.HasConnectedClients() && entity is Player && entity is not RemotePlayer)
            {
                if (Game.debug)
                {
                    Console.WriteLine("SERVER is removing regular player " + entity + " from level " + this);
                }

                entity.Remove();
            }

            if (Game.IsValidServer() && entity is Particle)
            {
                // there is no need to track this.
                if (Game.debug)
                {
                    Console.WriteLine("SERVER warning: Found particle in entity list: " + entity + ". Removing from level " + this);
                }

                entity.Remove();
            }

            if (entity.IsRemoved())
            {
                Remove(entity);
                return;
            }

            if (entity != Game.player)
            { // player is ticked separately, others are ticked on server
                if (!Game.IsValidClient())
                {
                    entity.Tick(); /// the main entity tick call.
                }
                else if (entity is ClientTickable clientTickable)
                {
                    clientTickable.clientTick();
                }
            }

            if (entity.IsRemoved() || entity.GetLevel() != this)
            {
                Remove(entity);
                return;
            }

            if (Game.HasConnectedClients()) // this means it's a server
            {
                Game.server.BroadcastEntityUpdate(entity);
            }
        }

        public void Tick(bool fullTick)
        {
            int count = 0;

            while (entitiesToAdd.Count > 0)
            {
                Entity entity = entitiesToAdd[0];
                bool inLevel = entities.Contains(entity);

                if (!inLevel)
                {
                    if (Game.IsValidServer())
                    {
                        Game.server.BroadcastEntityAddition(entity, true);
                    }


                    if (!Game.IsValidServer() || entity is not Particle)
                    {
                        if (Game.debug)
                        {
                            if (entity is DungeonChest || entity is AirWizard || entity is Player)
                            {
                                PrintEntityStatus("Adding ", entity);
                            }
                        }

                        lock (entityLock)
                        {
                            if (entity is Spark spark)
                            {
                                sparks.Add(spark);
                            }
                            else
                            {
                                entities.Add(entity);

                                if (entity is Player player)
                                {
                                    players.Add(player);
                                }
                            }
                        }
                    }
                }

                entitiesToAdd.Remove(entity);
            }

            if (fullTick && (!Game.IsValidServer() || GetPlayers().Length > 0))
            {
                // this prevents any entity (or tile) tick action from happening on a server level with no players.

                if (!Game.IsValidClient())
                {
                    for (int i = 0; i < w * h / 50; i++)
                    {
                        int xt = random.NextInt(w);
                        int yt = random.NextInt(w);
                        bool notableTick = GetTile(xt, yt).Tick(this, xt, yt);

                        if (Game.IsValidServer() && notableTick)
                        {
                            Game.server.BroadcastTileUpdate(this, xt, yt);
                        }
                    }
                }

                // entity loop
                foreach (Entity e in entities)
                {
                    TickEntity(e);

                    if (e is Mob)
                    {
                        count++;
                    }
                }

                // spark loop
                foreach (var spark in sparks)
                {
                    TickEntity(spark);
                }
            }

            while (count > maxMobCount)
            {
                Entity removeThis = entities[random.NextInt(entities.Count)];

                if (removeThis is MobAi)
                {
                    // make sure there aren't any close players
                    bool playerClose = false;

                    foreach (Player player in players)
                    {
                        if (Math.Abs(player.x - removeThis.x) < 128 && Math.Abs(player.y - removeThis.x) < 76)
                        {
                            playerClose = true;
                            break;
                        }
                    }

                    if (!playerClose)
                    {
                        Remove(removeThis);
                        count--;
                    }
                }
            }

            while (entitiesToRemove.Count > 0)
            {
                Entity entity = entitiesToRemove[0];

                if (Game.IsValidServer() && !(entity is Particle) && entity.GetLevel() == this)
                {
                    Game.server.BroadcastEntityRemoval(entity, this, true);
                }

                if (Game.debug && entity is Player)
                {
                    PrintEntityStatus("Removing ", entity);
                }

                entity.Remove(this); // this will safely fail if the entity's level doesn't match this one.

                lock (entityLock)
                {
                    if (entity is Spark spark)
                    {
                        sparks.Remove(spark);
                    }
                    else
                    {
                        entities.Remove(entity);
                    }
                }

                if (entity is Player player)
                {
                    players.Remove(player);
                }

                entitiesToRemove.Remove(entity);
            }

            mobCount = count;

            if (Game.IsValidServer() && players.Count == 0)
            {
                return; // don't try to spawn any mobs when there's no player on the level, on a server.
            }

            if (fullTick && count < maxMobCount && !Game.IsValidClient())
            {
                TrySpawn();
            }
        }

        public void PrintEntityStatus(string entityMessage, Entity entity)
        {
            if (entity is AirWizard airWizard && airWizard.secondform)
            {
                entityMessage += "II";
            }

            PrintLevelLoc(Network.OnlinePrefix() + entityMessage, entity.x >> 4, entity.y >> 4, ": " + entity);
        }

        public void DropItem(int x, int y, int mincount, int maxcount, params Item[] items)
        {
            DropItem(x, y, mincount + random.NextInt(maxcount - mincount + 1), items);
        }

        public void DropItem(int x, int y, int count, params Item[] items)
        {
            for (int i = 0; i < count; i++)
            {
                DropItem(x, y, items);
            }
        }

        public void DropItem(int x, int y, params Item[] items)
        {
            foreach (Item i in items)
                DropItem(x, y, i);
        }

        public ItemEntity DropItem(int x, int y, Item i)
        {
            if (Game.IsValidClient())
            {
                Console.WriteLine("Dropping item on client: " + i);
            }

            int ranx, rany;

            do
            {
                ranx = x + random.NextInt(11) - 5;
                rany = y + random.NextInt(11) - 5;
            } while (ranx >> 4 != x >> 4 || rany >> 4 != y >> 4);

            ItemEntity ie = new ItemEntity(i, ranx, rany);

            Add(ie);

            return ie;
        }

        public void RenderBackground(Screen screen, int xScroll, int yScroll)
        {
            int xo = xScroll >> 4; // latches to the nearest tile coordinate
            int yo = yScroll >> 4;
            int w = (Screen.w) >> 4; // there used to be a "+15" as in below method
            int h = (Screen.h) >> 4;

            screen.SetOffset(xScroll, yScroll);

            for (int y = yo; y <= h + yo; y++)
            {
                for (int x = xo; x <= w + xo; x++)
                {
                    GetTile(x, y).Render(screen, this, x, y);
                }
            }

            screen.SetOffset(0, 0);
        }

        public void RenderSprites(Screen screen, int xScroll, int yScroll)
        {
            int xo = xScroll >> 4; // latches to the nearest tile coordinate
            int yo = yScroll >> 4;
            int w = (Screen.w + 15) >> 4;
            int h = (Screen.h + 15) >> 4;

            screen.SetOffset(xScroll, yScroll);
            SortAndRender(screen, GetEntitiesInTiles(xo, yo, xo + w, yo + h));
            screen.SetOffset(0, 0);
        }

        public void RenderLight(Screen screen, int xScroll, int yScroll, int brightness)
        {
            int xo = xScroll >> 4;
            int yo = yScroll >> 4;
            int w = (Screen.w + 15) >> 4;
            int h = (Screen.h + 15) >> 4;
            int r = 4;

            screen.SetOffset(xScroll, yScroll);

            List<Entity> entities = GetEntitiesInTiles(xo - r, yo - r, w + xo + r, h + yo + r);

            foreach (Entity e in entities)
            {
                int lr = e.GetLightRadius();
                if (lr > 0) screen.RenderLight(e.x - 1, e.y - 4, lr * brightness);
            }

            for (int y = yo - r; y <= h + yo + r; y++)
            {
                for (int x = xo - r; x <= w + xo + r; x++)
                {
                    if (x < 0 || y < 0 || x >= this.w || y >= this.h)
                    {
                        continue;
                    }

                    int lr = GetTile(x, y).GetLightRadius(this, x, y);

                    if (lr > 0)
                    {
                        screen.RenderLight(x * 16 + 8, y * 16 + 8, lr * brightness);
                    }
                }
            }

            screen.SetOffset(0, 0);
        }

        private void SortAndRender(Screen screen, List<Entity> list)
        {
            list.Sort(spriteSorter);

            for (int i = 0; i < list.Count; i++)
            {
                Entity e = list[i];

                if (e.GetLevel() == this && !e.IsRemoved())
                {
                    e.Render(screen);
                }
                else
                {
                    Remove(e);
                }
            }
        }

        public Tile GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= w || y >= h/* || (x + y * w) >= tiles.length*/)
            {
                return Tiles.Tiles.Get("connector tile");
            }

            int id = tiles[x + y * w];

            if (id < 0)
            {
                id += 256;
            }

            return Tiles.Tiles.Get(id);
        }

        public void SetTile(int x, int y, string tilewithdata)
        {
            if (!tilewithdata.Contains("_"))
            {
                SetTile(x, y, Tiles.Tiles.Get(tilewithdata));
                return;
            }

            string name = tilewithdata.Substring(0, tilewithdata.IndexOf("_"));
            int data = Tiles.Tiles.Get(name).GetData(tilewithdata[(name.Length + 1)..]);

            SetTile(x, y, Tiles.Tiles.Get(name), data);
        }

        public void SetTile(int x, int y, Tile t)
        {
            SetTile(x, y, t, t.GetDefaultData());
        }

        public void SetTile(int x, int y, Tile t, int dataVal)
        {
            if (x < 0 || y < 0 || x >= w || y >= h)
            {
                return;
            }

            if (Game.IsValidClient() && !Game.IsValidServer())
            {
                Console.WriteLine("Client requested a tile update for the " + t.name + " tile at " + x + "," + y);
            }
            else
            {
                tiles[x + y * w] = t.id;
                data[x + y * w] = (byte)dataVal;
            }

            if (Game.IsValidServer())
            {
                Game.server.BroadcastTileUpdate(this, x, y);
            }
        }

        public int GetData(int x, int y)
        {
            return x < 0 || y < 0 || x >= w || y >= h ? 0 : data[x + y * w] & 0xff;
        }

        public void SetData(int x, int y, int val)
        {
            if (x < 0 || y < 0 || x >= w || y >= h)
            {
                return;
            }

            data[x + y * w] = (byte)val;
        }

        public void Add(Entity e)
        {
            if (e == null)
            {
                return;
            }

            Add(e, e.x, e.y);
        }

        public void Add(Entity entity, int x, int y)
        {
            Add(entity, x, y, false);
        }

        public void Add(Entity entity, int x, int y, bool tileCoords)
        {
            if (entity == null)
            {
                return;
            }

            if (tileCoords)
            {
                x = x * 16 + 8;
                y = y * 16 + 8;
            }

            entity.SetLevel(this, x, y);
            entitiesToRemove.Remove(entity); // to make sure the most recent request is satisfied.

            if (!entitiesToAdd.Contains(entity))
            {
                entitiesToAdd.Add(entity);
            }
        }

        public void Remove(Entity e)
        {
            entitiesToAdd.Remove(e);
            if (!entitiesToRemove.Contains(e))
            {
                entitiesToRemove.Add(e);
            }
        }

        private void TrySpawn()
        {
            int spawnSkipChance = (int)(MOB_SPAWN_FACTOR * Math.Pow(mobCount, 2) / Math.Pow(maxMobCount, 2));

            if (spawnSkipChance > 0 && random.NextInt(spawnSkipChance) != 0)
            {
                return; // hopefully will make mobs spawn a lot slower.
            }

            bool spawned = false;

            for (int i = 0; i < 30 && !spawned; i++)
            {
                int minLevel = 1, maxLevel = 1;

                if (depth < 0)
                {
                    //TODO: Check if random.NextDouble is a correct replacement for Math.random()
                    maxLevel = (-depth) + ((random.NextDouble() > 0.75 && -depth != 4) ? 1 : 0);
                }
                if (depth > 0)
                {
                    minLevel = maxLevel = 4;
                }


                int lvl = random.NextInt(maxLevel - minLevel + 1) + minLevel;
                int rnd = random.NextInt(100);
                int nx = random.NextInt(w) * 16 + 8, ny = random.NextInt(h) * 16 + 8;

                //Console.WriteLine("trySpawn on level " + depth + " of lvl " + lvl + " mob w/ rand " + rnd + " at tile " + nx + "," + ny);

                // spawns the enemy mobs; first part prevents enemy mob spawn on surface on first day, more or less.
                if ((Updater.getTime() == Updater.Time.Night && Updater.pastDay1 || depth != 0) && EnemyMob.CheckStartPos(this, nx, ny))
                { // if night or underground, with a valid tile, spawn an enemy mob.
                    if (depth != -4)
                    { // normal mobs
                        if (rnd <= 40)
                        {
                            Add((new Slime(lvl)), nx, ny);
                        }
                        else if (rnd <= 75)
                        {
                            Add((new Zombie(lvl)), nx, ny);
                        }
                        else if (rnd >= 85)
                        {
                            Add((new Skeleton(lvl)), nx, ny);
                        }
                        else
                        {
                            Add((new Creeper(lvl)), nx, ny);
                        }
                    }
                    else
                    { // special dungeon mobs
                        if (rnd <= 40)
                        {
                            Add((new Snake(lvl)), nx, ny);
                        }
                        else if (rnd <= 75)
                        {
                            Add((new Knight(lvl)), nx, ny);
                        }
                        else if (rnd >= 85)
                        {
                            Add((new Snake(lvl)), nx, ny);
                        }
                        else
                        {
                            Add((new Knight(lvl)), nx, ny);
                        }
                    }

                    spawned = true;
                }

                if (depth == 0 && PassiveMob.checkStartPos(this, nx, ny))
                {
                    // spawns the friendly mobs.
                    if (rnd <= (Updater.getTime() == Updater.Time.Night ? 22 : 33))
                    {
                        Add((new Cow()), nx, ny);
                    }
                    else if (rnd >= 68)
                    {
                        Add((new Pig()), nx, ny);
                    }
                    else
                    {
                        Add((new Sheep()), nx, ny);
                    }

                    spawned = true;
                }
            }
        }

        public void RemoveAllEnemies()
        {
            foreach (Entity e in GetEntityArray())
            {
                if (e is EnemyMob)
                {
                    if (e is not AirWizard || Game.IsMode("creative")) // don't remove the airwizard bosses! Unless in creative, since you can spawn more.
                    {
                        e.Remove();
                    }
                }
            }
        }

        public void ClearEntities()
        {
            if (!Game.ISONLINE)
            {
                entities.Clear();
            }
            else
            {
                foreach (Entity e in GetEntityArray())
                {
                    e.Remove();
                }
            }
        }

        public Entity[] GetEntityArray()
        {
            Entity[] entityArray;
            int index = 0;

            lock (entityLock)
            {
                entityArray = new Entity[entities.Count + sparks.Count];

                foreach (Entity entity in entities)
                {
                    entityArray[index++] = entity;
                }

                foreach (Spark spark in sparks)
                {
                    entityArray[index++] = spark;
                }
            }

            return entityArray;
        }

        public List<Entity> GetEntitiesInTiles(int xt, int yt, int radius)
        {
            return GetEntitiesInTiles(xt, yt, radius, false, _ => true);
        }

        public List<Entity> GetEntitiesInTiles(int xt, int yt, int radius, bool includeGiven, Predicate<Entity> predicate)
        {
            return GetEntitiesInTiles(xt - radius, yt - radius, xt + radius, yt + radius, includeGiven, predicate);
        }

        public List<Entity> GetEntitiesInTiles(int xt0, int yt0, int xt1, int yt1)
        {
            return GetEntitiesInTiles(xt0, yt0, xt1, yt1, false, _ => true);
        }

        public List<Entity> GetEntitiesInTiles(int xt0, int yt0, int xt1, int yt1, bool includeGiven, Predicate<Entity> predicate)
        {
            List<Entity> contained = new();

            foreach (Entity e in GetEntityArray())
            {
                int xt = e.x >> 4;
                int yt = e.y >> 4;

                if (xt >= xt0 && xt <= xt1 && yt >= yt0 && yt <= yt1)
                {
                    bool matches = predicate?.Invoke(e) ?? true;

                    if (matches == includeGiven)
                    {
                        contained.Add(e);
                    }
                }
            }

            return contained;
        }

        public List<Entity> GetEntitiesInRect(Rectangle area)
        {
            List<Entity> result = new();

            foreach (Entity e in GetEntityArray())
            {
                if (e.IsTouching(area))
                {
                    result.Add(e);
                }
            }

            return result;
        }

        public List<Entity> GetEntitiesInRect(Predicate<Entity> filter, Rectangle area)
        {
            List<Entity> result = new();

            foreach (Entity entity in entities)
            {
                if (filter(entity) && entity.IsTouching(area))
                {
                    result.Add(entity);
                }
            }
            return result;
        }

        /// finds all entities that are an instance of the given entity.
        public Entity[] GetEntitiesOfClass<T>() where T : Entity
        {
            return GetEntityArray().OfType<T>().ToArray();
        }

        public Player[] GetPlayers()
        {
            return players.ToArray();
        }

        public Player GetClosestPlayer(int x, int y)
        {
            Player[] players = GetPlayers();

            if (players.Length == 0)
            {
                return null;
            }

            Player closest = players[0];

            int xd = closest.x - x;
            int yd = closest.y - y;

            for (int i = 1; i < players.Length; i++)
            {
                int curxd = players[i].x - x;
                int curyd = players[i].y - y;

                if (xd * xd + yd * yd > curxd * curxd + curyd * curyd)
                {
                    closest = players[i];
                    xd = curxd;
                    yd = curyd;
                }
            }

            return closest;
        }

        public Point[] GetAreaTilePositions(int x, int y, int r)
        {
            return GetAreaTilePositions(x, y, r, r);
        }

        public Point[] GetAreaTilePositions(int x, int y, int rx, int ry)
        {
            List<Point> local = new();

            for (int yp = y - ry; yp <= y + ry; yp++)
            {
                for (int xp = x - rx; xp <= x + rx; xp++)
                {
                    if (xp >= 0 && xp < w && yp >= 0 && yp < h)
                    {
                        local.Add(new Point(xp, yp));
                    }
                }
            }

            return local.ToArray();
        }

        public Tile[] GetAreaTiles(int x, int y, int r)
        {
            return GetAreaTiles(x, y, r, r);
        }

        public Tile[] GetAreaTiles(int x, int y, int rx, int ry)
        {
            List<Tile> local = new();

            foreach (Point p in GetAreaTilePositions(x, y, rx, ry))
            {
                local.Add(GetTile(p.x, p.y));
            }

            return local.ToArray();
        }

        public void SetAreaTiles(int xt, int yt, int r, Tile tile, int data)
        {
            SetAreaTiles(xt, yt, r, tile, data, false);
        }

        public void SetAreaTiles(int xt, int yt, int r, Tile tile, int data, bool overwriteStairs)
        {
            for (int y = yt - r; y <= yt + r; y++)
            {
                for (int x = xt - r; x <= xt + r; x++)
                {
                    if (overwriteStairs || (!GetTile(x, y).name.ToLower().Contains("stairs")))
                    {
                        SetTile(x, y, tile, data);
                    }
                }
            }
        }

        public void SetAreaTiles(int xt, int yt, int r, Tile tile, int data, string[] blacklist)
        {
            for (int y = yt - r; y <= yt + r; y++)
            {
                for (int x = xt - r; x <= xt + r; x++)
                {
                    if (!blacklist.Contains(GetTile(x, y).name.ToLower()))
                    {
                        SetTile(x, y, tile, data);
                    }
                }
            }
        }

        public delegate bool TileCheck(Tile t, int x, int y);

        public List<Point> GetMatchingTiles(Tile search)
        {
            return GetMatchingTiles((t, x, y) => t.Equals(search));
        }

        public List<Point> GetMatchingTiles(params Tile[] search)
        {
            return GetMatchingTiles((t, x, y) =>
            {
                foreach (Tile poss in search)
                {
                    if (t.Equals(poss))
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        public List<Point> GetMatchingTiles(TileCheck condition)
        {
            List<Point> matches = new();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (condition(GetTile(x, y), x, y))
                    {
                        matches.Add(new Point(x, y));
                    }
                }
            }

            return matches;
        }

        public bool IsLight(int x, int y)
        {
            foreach (Tile t in GetAreaTiles(x, y, 3))
            {
                if (t is TorchTile)
                {
                    return true;
                }
            }

            return false;
        }

        private bool NoStairs(int x, int y)
        {
            return GetTile(x, y) != Tiles.Tiles.Get("Stairs Down");
        }


        private void GenerateSpawnerStructures()
        {
            for (int i = 0; i < 18 / -depth * (w / 128); i++)
            {
                /// for generating spawner dungeons
                MobAi m;

                int r = random.NextInt(5);

                if (r == 1)
                {
                    m = new Skeleton(-depth);
                }
                else if (r == 2 || r == 0)
                {
                    m = new Slime(-depth);
                }
                else
                {
                    m = new Zombie(-depth);
                }

                Spawner sp = new Spawner(m);

                int x3 = random.NextInt(16 * w) / 16;
                int y3 = random.NextInt(16 * h) / 16;

                if (GetTile(x3, y3) == Tiles.Tiles.Get("dirt"))
                {
                    bool xaxis2 = random.NextBool();

                    if (xaxis2)
                    {
                        for (int s2 = x3; s2 < w - s2; s2++)
                        {
                            if (GetTile(s2, y3) == Tiles.Tiles.Get("rock"))
                            {
                                sp.x = s2 * 16 - 24;
                                sp.y = y3 * 16 - 24;
                            }
                        }
                    }
                    else
                    {
                        for (int s2 = y3; s2 < y3 - s2; s2++)
                        {
                            if (GetTile(x3, s2) == Tiles.Tiles.Get("rock"))
                            {
                                sp.x = x3 * 16 - 24;
                                sp.y = s2 * 16 - 24;
                            }
                        }
                    }

                    if (sp.x == 0 && sp.y == 0)
                    {
                        sp.x = x3 * 16 - 8;
                        sp.y = y3 * 16 - 8;
                    }

                    if (GetTile(sp.x / 16, sp.y / 16) == Tiles.Tiles.Get("rock"))
                    {
                        GetTile(sp.x / 16, sp.y / 16, Tiles.Tiles.Get("dirt"));
                    }

                    Structure.mobDungeonCenter.Draw(this, sp.x / 16, sp.y / 16);

                    if (GetTile(sp.x / 16, sp.y / 16 - 4) == Tiles.Tiles.Get("dirt"))
                    {
                        Structure.mobDungeonNorth.Draw(this, sp.x / 16, sp.y / 16 - 5);
                    }
                    if (GetTile(sp.x / 16, sp.y / 16 + 4) == Tiles.Tiles.Get("dirt"))
                    {
                        Structure.mobDungeonSouth.Draw(this, sp.x / 16, sp.y / 16 + 5);
                    }
                    if (GetTile(sp.x / 16 + 4, sp.y / 16) == Tiles.Tiles.Get("dirt"))
                    {
                        Structure.mobDungeonEast.Draw(this, sp.x / 16 + 5, sp.y / 16);
                    }
                    if (GetTile(sp.x / 16 - 4, sp.y / 16) == Tiles.Tiles.Get("dirt"))
                    {
                        Structure.mobDungeonWest.Draw(this, sp.x / 16 - 5, sp.y / 16);
                    }

                    Add(sp);

                    for (int rpt = 0; rpt < 2; rpt++)
                    {
                        if (random.NextInt(2) != 0)
                        {
                            continue;
                        }

                        Chest c = new Chest();
                        int chance = -depth;

                        c.populateInvRandom("minidungeon", chance);

                        Add(c, sp.x - 16, sp.y - 16);
                    }
                }
            }
        }

        private void GenerateVillages()
        {
            int lastVillageX = 0;
            int lastVillageY = 0;

            for (int i = 0; i < w / 128 * 2; i++)
            {
                // makes 2-8 villages based on world size

                for (int t = 0; t < 10; t++)
                {
                    // tries 10 times for each one

                    int x = random.NextInt(w);
                    int y = random.NextInt(h);

                    // makes sure the village isn't to close to the previous village
                    if (GetTile(x, y) == Tiles.Tiles.Get("grass") && (Math.Abs(x - lastVillageX) > 16 && Math.Abs(y - lastVillageY) > 16))
                    {
                        lastVillageX = x;
                        lastVillageY = y;

                        // a number between 2 and 4
                        int numHouses = random.NextInt(3) + 2;

                        // loops for each house in the village
                        for (int hs = 0; hs < numHouses; hs++)
                        {
                            bool hasChest = random.NextBool();
                            bool twoDoors = random.NextBool();
                            int overlay = random.NextInt(2) + 1;

                            // basically just gets what offset this house should have from the center of the village
                            int xo = hs == 0 || hs == 3 ? -4 : 4;
                            int yo = hs < 2 ? -4 : 4;

                            xo += random.NextInt(5) - 2;
                            yo += random.NextInt(5) - 2;

                            if (twoDoors)
                            {
                                Structure.villageHouseTwoDoor.Draw(this, x + xo, y + yo);
                            }
                            else
                            {
                                Structure.villageHouseNormal.Draw(this, x + xo, y + yo);
                            }

                            // make the village look ruined
                            if (overlay == 1)
                            {
                                Structure.villageRuinedOverlay1.Draw(this, x + xo, y + yo);
                            }
                            else if (overlay == 2)
                            {
                                Structure.villageRuinedOverlay2.Draw(this, x + xo, y + yo);
                            }

                            // add a chest to some of the houses
                            if (hasChest)
                            {
                                Chest c = new Chest();

                                c.populateInvRandom("villagehouse", 1);
                                Add(c, (x + random.NextInt(2) + xo) << 4, (y + random.NextInt(2) + yo) << 4);
                            }
                        }

                        break;
                    }
                }
            }
        }

        public override string ToString()
        {
            return "Level(depth=" + depth + ")";
        }

        private class EntityComparer : Comparer<Entity>
        {
            public override int Compare(Entity x, Entity y)
            {
                return x.eid - y.eid; //TODO: Check this
            }
        }
    }
}
