using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class Spawner : Furniture
    {
        private static readonly int ACTIVE_RADIUS = 8 * 16;
        private static readonly int minSpawnInterval = 200, maxSpawnInterval = 500;
        private static readonly int minMobSpawnChance = 10; // 1 in minMobSpawnChance chance of calling trySpawn every interval.

        public MobAi mob;
        private int health, lvl, maxMobLevel;
        private int spawnTick;

        /**
         * Initializes the spawners variables to the corresponding values from the mob.
         * @param m The mob which this spawner will spawn.
         */
        private void InitMob(MobAi m)
        {
            mob = m;
            sprite.color = col = mob.col;

            if (m is EnemyMob enemyMob)
            {
                lvl = enemyMob.lvl;
                maxMobLevel = mob.getMaxLevel();
            }
            else
            {
                lvl = 1;
                maxMobLevel = 1;
            }

            if (lvl > maxMobLevel)
            {
                lvl = maxMobLevel;
            }
        }

        /**
         * Creates a new spawner for the mob m.
         * @param m Mob which will be spawned.
         */
        public Spawner(MobAi m)
            : base(GetClassName(m.GetType()) + " Spawner", new Sprite(8, 28, 2, 2, 2), 7, 2)
        {
            health = 100;

            InitMob(m);
            ResetSpawnInterval();
        }

        /**
         * Returns the classname of a class.
         * @param c The class.
         * @return String representation of the classname.
         */
        private static string GetClassName(Type c)
        {
            return c.Name;
        }

        public override void Tick()
        {
            base.Tick();

            spawnTick--;

            if (spawnTick <= 0)
            {
                int chance = (int)(minMobSpawnChance * Math.Pow(level.mobCount, 2) / Math.Pow(level.maxMobCount, 2)); // this forms a quadratic function that determines the mob spawn chance.

                if (chance <= 0 || random.NextInt(chance) == 0)
                {
                    TrySpawn();
                }

                ResetSpawnInterval();
            }
        }

        /**
         * Resets the spawner so it can spawn another mob.
         */
        private void ResetSpawnInterval()
        {
            spawnTick = random.NextInt(maxSpawnInterval - minSpawnInterval + 1) + minSpawnInterval;
        }

        /**
         * Tries to spawn a new mob.
         */
        private void TrySpawn()
        {
            if (level == null || Game.IsValidClient())
            {
                return;
            }

            if (level.mobCount >= level.maxMobCount)
            {
                return; // can't spawn more entities
            }

            Player player = GetClosestPlayer();

            if (player == null)
            {
                return;
            }

            int xd = player.x - x;
            int yd = player.y - y;

            if (xd * xd + yd * yd > ACTIVE_RADIUS * ACTIVE_RADIUS) return;

            MobAi newmob;

            try
            {
                if (mob is EnemyMob)
                {
                    newmob = Activator.CreateInstance(mob.GetType(), lvl);
                }
                else
                {
                    newmob = Activator.CreateInstance(mob.GetType());
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Spawner ERROR: could not spawn mob; error initializing mob instance:");
                ex.PrintStackTrace();

                return;
            }

            Point pos = new Point(x >> 4, y >> 4);
            Point[] areaPositions = level.GetAreaTilePositions(pos.x, pos.y, 1);
            List<Point> validPositions = new();

            foreach (Point p in areaPositions)
            {
                if (!(!level.GetTile(p.x, p.y).MayPass(level, p.x, p.y, newmob) || mob is EnemyMob && level.GetTile(p.x, p.y).GetLightRadius(level, p.x, p.y) > 0))
                {
                    validPositions.Add(p);
                }
            }

            if (validPositions.Count == 0)
            {
                return; // cannot spawn mob.
            }

            Point spawnPos = validPositions[random.NextInt(validPositions.Count)];

            newmob.x = spawnPos.x << 4;
            newmob.y = spawnPos.y << 4;
            //if (Game.debug) level.printLevelLoc("spawning new " + mob, (newmob.x>>4), (newmob.y>>4), "...");

            level.Add(newmob);
            Sound.monsterHurt.Play();

            for (int i = 0; i < 6; i++)
            {
                int randX = random.NextInt(16);
                int randY = random.NextInt(12);

                level.Add(new FireParticle(x - 8 + randX, y - 6 + randY));
            }
        }

        public override bool Interact(Player player, Item item, Direction attackDir)
        {
            if (item is ToolItem tool)
            {
                Sound.monsterHurt.Play();

                int dmg;

                if (Game.IsMode("creative"))
                {
                    dmg = health;
                }
                else
                {
                    dmg = tool.level + random.NextInt(2);

                    if (tool.type == ToolType.Pickaxe)
                    {
                        dmg += random.NextInt(5) + 2;
                    }

                    if (player.potioneffects.ContainsKey(PotionType.Haste))
                    {
                        dmg *= 2;
                    }
                }

                health -= dmg;
                level.Add(new TextParticle("" + dmg, x, y, Color.Get(-1, 200, 300, 400)));

                if (health <= 0)
                {
                    level.Remove(this);

                    Sound.playerDeath.Play();

                    player.AddScore(500);
                }

                return true;
            }

            if (item is PowerGloveItem && Game.IsMode("creative"))
            {
                level.Remove(this);

                if (player.activeItem is not PowerGloveItem)
                {
                    player.GetInventory().Add(0, player.activeItem);
                }

                player.activeItem = new FurnitureItem(this);

                return true;
            }

            if (item == null)
            {
                return Use(player);
            }

            return false;
        }

        public override bool Use(Player player)
        {
            if (Game.IsMode("creative") && mob is EnemyMob)
            {
                lvl++;

                if (lvl > maxMobLevel)
                {
                    lvl = 1;
                }

                try
                {
                    EnemyMob newmob = (EnemyMob)Activator.CreateInstance(mob.GetType(), lvl);

                    InitMob(newmob);
                }
                catch (Exception ex)
                {
                    ex.PrintStackTrace();
                }

                return true;
            }

            return false;
        }

        public override Furniture Clone()
        {
            return new Spawner(mob);
        }

        protected override string GetUpdateString()
        {
            string updates = base.GetUpdateString() + ";";

            updates += "health," + health + ";lvl," + lvl;

            return updates;
        }

        protected override bool UpdateField(string field, string val)
        {
            if (base.UpdateField(field, val))
            {
                return true;
            }

            switch (field)
            {
                case "health":
                    health = int.Parse(val);
                    return true;
                case "lvl":
                    lvl = int.Parse(val);
                    return true;
            }

            return false;
        }
    }
}
