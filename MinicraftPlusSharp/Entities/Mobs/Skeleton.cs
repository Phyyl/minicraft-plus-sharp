using MinicraftPlusSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Skeleton : EnemyMob
    {
        private static new readonly MobSprite[][][] sprites;

        static Skeleton()
        {
            sprites = new MobSprite[4][][];

            for (int i = 0; i < 4; i++)
            {
                MobSprite[][] list = MobSprite.CompileMobSpriteAnimations(16, 0 + (i * 2));
                sprites[i] = list;
            }
        }

        private int arrowtime;
        private int artime;

        /**
         * Creates a skeleton of a given level.
         * @param lvl The skeleton's level.
         */
        public Skeleton(int lvl)
            : base(lvl, sprites, 6, true, 100, 45, 200)
        {
            arrowtime = 500 / (lvl + 5);
            artime = arrowtime;
        }

        public override void Tick()
        {
            base.Tick();

            if (SkipTick())
            {
                return;
            }

            Player player = GetClosestPlayer();
            if (player != null && randomWalkTime == 0 && !Game.IsMode("Creative"))
            { // Run if there is a player nearby, the skeleton has finished their random walk, and gamemode is not creative.
                artime--;

                int xd = player.x - x;
                int yd = player.y - y;

                if (xd * xd + yd * yd < 100 * 100)
                {
                    if (artime < 1)
                    {
                        level.Add(new Arrow(this, dir, lvl));
                        artime = arrowtime;
                    }
                }
            }
        }

        public override void Die()
        {
            int[] diffrands = { 20, 20, 30 };
            int[] diffvals = { 13, 18, 28 };
            int diff = Settings.GetIdx("diff");

            int count = random.NextInt(3 - diff) + 1;
            int bookcount = random.NextInt(1) + 1;
            int rand = random.NextInt(diffrands[diff]);
            
            if (rand <= diffvals[diff])
            {
                level.DropItem(x, y, count, Items.Items.Get("bone"), Items.Items.Get("arrow"));
            }
            else if (diff == 0 && rand >= 19) // rare chance of 10 arrows on easy mode
            {
                level.DropItem(x, y, 10, Items.Items.Get("arrow"));
            }
            else
            {
                level.DropItem(x, y, bookcount, Items.Items.Get("Antidious"), Items.Items.Get("arrow"));
            }

            base.Die();
        }
    }
}
