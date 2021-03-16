using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Levels;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MinicraftPlusSharp.Entities.Furniture
{
    public class Tnt : Furniture
    {
        private static int FUSE_TIME = 90;
        private static int BLAST_RADIUS = 32;
        private static int BLAST_DAMAGE = 30;

        private int ftik = 0;
        private bool fuseLit = false;
        private Timer explodeTimer;
        private Level levelSave;

        private string[] explosionBlacklist = new string[] { "hard rock", "obsidian wall" };

        /**
         * Creates a new tnt furniture.
         */
        public Tnt()
            : base("Tnt", new Sprite(28, 26, 2, 2, 2), 3, 2)
        {
            fuseLit = false;
            ftik = 0;

            explodeTimer = new Timer(300);
            explodeTimer.Elapsed += ExplodeTimer_Elapsed;
        }

        private void ExplodeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            explodeTimer.Stop();

            int xt = x >> 4;
            int yt = (y - 2) >> 4;

            if (levelSave.depth != 1)
            {
                levelSave.SetAreaTiles(xt, yt, 1, Tiles.Get("hole"), 0, explosionBlacklist);
            }
            else
            {
                levelSave.SetAreaTiles(xt, yt, 1, Tiles.Get("Infinite Fall"), 0, explosionBlacklist);
            }

            levelSave = null;
        }

        public override void Tick()
        {
            base.Tick();

            if (fuseLit)
            {
                ftik++;

                if (ftik >= FUSE_TIME)
                {
                    // blow up
                    List<Entity> entitiesInRange = level.GetEntitiesInRect(new Rectangle(x, y, BLAST_RADIUS * 2, BLAST_RADIUS * 2, Rectangle.CENTER_DIMS));

                    foreach (Entity e in entitiesInRange)
                    {
                        float dist = (float)Math.Sqrt(Math.Pow(e.x - x, 2) + Math.Pow(e.y - y, 2));
                        int dmg = (int)(BLAST_DAMAGE * (1 - (dist / BLAST_RADIUS))) + 1;

                        if (e is Mob mob)
                        {
                            mob.Hurt(this, dmg);
                        }
                        if (e is Tnt tnt)
                        {
                            if (!tnt.fuseLit)
                            {
                                tnt.fuseLit = true;
                                Sound.fuse.Play();
                                tnt.ftik = FUSE_TIME * 2 / 3;
                            }
                        }
                    }

                    Sound.explode.Play();

                    int xt = x >> 4;
                    int yt = (y - 2) >> 4;

                    level.SetAreaTiles(xt, yt, 1, Tiles.Get("explode"), 0, explosionBlacklist);

                    levelSave = level;
                    explodeTimer.Start();

                    base.Remove();
                }
            }
        }

        public override void Render(Screen screen)
        {
            if (fuseLit)
            {
                int colFctr = 100 * ((ftik % 15) / 5) + 200;
                col = Color.Get(-1, colFctr, colFctr + 100, 555);
            }

            base.Render(screen);
        }

        public override bool Interact(Player player, Item heldItem, Direction attackDir)
        {
            if (!fuseLit)
            {
                fuseLit = true;
                Sound.fuse.Play();
                return true;
            }

            return false;
        }

        protected override string GetUpdateString()
        {
            string updates = base.GetUpdateString() + ";";
            updates += "fuseLit," + fuseLit + ";ftik," + ftik;

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
                case "fuseLit":
                    fuseLit = bool.Parse(val);
                    return true;
                case "ftik":
                    ftik = int.Parse(val);
                    return true;
            }

            return false;
        }
    }

}
