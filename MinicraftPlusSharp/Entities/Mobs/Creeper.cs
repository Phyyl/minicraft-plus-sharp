using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Creeper : EnemyMob
    {
        private static MobSprite[][][] sprites;

        static Creeper()
        {
            sprites = new MobSprite[4][][];

            for (int i = 0; i < 4; i++)
            {
                MobSprite[] list = MobSprite.CompileSpriteList(4, 0 + (i * 2), 2, 2, 0, 2);
                sprites[i][0] = list;
            }
        }

        private static readonly int MAX_FUSE_TIME = 60;
        private static readonly int TRIGGER_RADIUS = 64;
        private static readonly int BLAST_DAMAGE = 50;

        private int fuseTime = 0;
        private bool fuseLit = false;

        public Creeper(int lvl)
            : base(lvl, sprites, 10, 50)
        {
        }

        public bool Move(int xd, int yd)
        {
            bool result = base.Move(xd, yd);

            dir = Direction.DOWN;

            if (xd == 0 && yd == 0)
            {
                walkDist = 0;
            }

            return result;
        }

        public override void Tick()
        {
            base.Tick();

            if (Game.IsMode("Creative"))
            {
                return; // Creeper should not explode if player is in creative mode
            }

            if (fuseTime > 0)
            {
                fuseTime--; // fuse getting shorter...
                xmov = ymov = 0;
            }
            else if (fuseLit)
            { // fuseLit is set to true when fuseTime is set to max, so this happens after fuseTime hits zero, while fuse is lit.
                xmov = ymov = 0;

                bool playerInRange = false; // tells if any players are within the blast

                // Find if the player is in range and store it in playerInRange.
                foreach (Entity e in level.GetEntitiesOfClass<Mob>())
                {
                    Mob mob = (Mob)e;

                    int pdx = Math.Abs(mob.x - x);
                    int pdy = Math.Abs(mob.y - y);

                    if (pdx < TRIGGER_RADIUS && pdy < TRIGGER_RADIUS)
                    {
                        if (mob is Player)
                        {
                            playerInRange = true;
                        }
                    }
                }

                // basically, if there aren't any players it "defuses" itself and doesn't blow up
                if (playerInRange)
                {
                    // blow up
                    Sound.explode.Play();

                    // figure out which tile the mob died on
                    int xt = x >> 4;
                    int yt = (y - 2) >> 4;

                    // used for calculations
                    int radius = lvl;
                    int lvlDamage = BLAST_DAMAGE * lvl;

                    // hurt all the entities
                    List<Entity> entitiesInRange = level.GetEntitiesInTiles(xt, yt, radius);
                    List<Entity> spawners = new();
                    Point[] tilePositions = level.GetAreaTilePositions(xt, yt, radius);

                    foreach (Entity entity in entitiesInRange)
                    { // Hurts entities in range
                        if (entity is Mob mob)
                        {
                            int distx = Math.Abs(mob.x - x);
                            int disty = Math.Abs(mob.y - y);
                            float distDiag = (float)Math.Sqrt(distx * distx + disty * disty);

                            mob.Hurt(this, (int)(lvlDamage * (1 / (distDiag + 1)) + Settings.GetIdx("diff")));
                        }
                        else if (entity is Spawner)
                        {
                            spawners.Add(entity);
                        }

                        if (entity == this)
                        {
                            continue;
                        }

                        Point ePos = new Point(entity.x >> 4, entity.y >> 4);

                        foreach (Point p in tilePositions)
                        {
                            if (!p.Equals(ePos))
                            {
                                continue;
                            }

                            if (!level.GetTile(p.x, p.y).MayPass(level, p.x, p.y, entity))
                            {
                                entity.Die();
                            }
                        }
                    }

                    foreach (Point tilePosition in tilePositions)
                    { // Destroys tiles in range
                        bool hasSpawner = false;

                        foreach (Entity spawner in spawners)
                        {
                            if (spawner.x >> 4 == tilePosition.x && spawner.y >> 4 == tilePosition.y)
                            { // Check if current tile has a spawner on it
                                hasSpawner = true;
                                break;
                            }
                        }

                        if (!hasSpawner)
                        {
                            if (level.depth != 1)
                            {
                                level.SetAreaTiles(tilePosition.x, tilePosition.y, 0, Tiles.Get("hole"), 0);
                            }
                            else
                            {
                                level.SetAreaTiles(tilePosition.x, tilePosition.y, 0, Tiles.Get("Infinite Fall"), 0);
                            }

                        }
                    }

                    Die(); // dying now kind of kills everything. the super class will take care of it.
                }
                else
                {
                    fuseTime = 0;
                    fuseLit = false;
                }
            }
        }

        public override void Render(Screen screen)
        {
            /*if (fuseLit && fuseTime % 6 == 0) {
                super.lvlcols[lvl-1] = Color.get(-1, 252);
            }
            else
                super.lvlcols[lvl-1] = Creeper.lvlcols[lvl-1];

            sprites[0] = walkDist == 0 ? standing : walking;*/

            base.Render(screen);
        }

        protected override void TouchedBy(Entity entity)
        {
            if (Game.IsMode("Creative"))
            {
                return;
            }

            if (entity is Player player)
            {
                if (fuseTime == 0 && !fuseLit)
                {
                    Sound.fuse.Play();
                    fuseTime = MAX_FUSE_TIME;
                    fuseLit = true;
                }

                player.Hurt(this, 1);
            }
        }

        public override bool CanWool()
        {
            return false;
        }

        public override void Die()
        {
            // Only drop items if the creeper has not exploded
            if (!fuseLit)
            {
                DropItem(1, 4 - Settings.GetIdx("diff"), Items.Items.Get("Gunpowder"));
            }

            base.Die();
        }

        protected override string GetUpdateString()
        {
            String updates = base.GetUpdateString() + ";";
            updates += "fuseTime," + fuseTime + ";fuseLit," + fuseLit;

            return updates;
        }

        protected override bool UpdateField(String field, String val)
        {
            if (base.UpdateField(field, val))
            {
                return true;
            }

            switch (field)
            {
                case "fuseTime":
                    fuseTime = int.Parse(val);
                    return true;

                case "fuseLit":
                    bool wasLit = fuseLit;

                    fuseLit = bool.Parse(val);

                    if (fuseLit && !wasLit)
                    {
                        Sound.fuse.Play();
                    }
                    break;
            }

            return false;
        }
    }
}
