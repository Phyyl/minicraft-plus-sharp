using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Zombie : EnemyMob
    {
        private static new readonly MobSprite[][][] sprites;

        static Zombie()
        {
            sprites = new MobSprite[4][][];

            for (int i = 0; i < 4; i++)
            {
                MobSprite[][] list = MobSprite.CompileMobSpriteAnimations(8, 0 + (i * 2));
                sprites[i] = list;
            }
        }

        /**
         * Creates a zombie of the given level.
         * @param lvl Zombie's level.
         */
        public Zombie(int lvl)
            : base(lvl, sprites, 5, 100)
        {
        }

        public override void Die()
        {
            if (Settings.Get("diff").Equals("Easy"))
            {
                DropItem(2, 4, Items.Items.Get("cloth"));
            }

            if (Settings.Get("diff").Equals("Normal"))
            {
                DropItem(1, 3, Items.Items.Get("cloth"));
            }

            if (Settings.Get("diff").Equals("Hard"))
            {
                DropItem(1, 2, Items.Items.Get("cloth"));
            }

            if (random.NextInt(60) == 2)
            {
                level.DropItem(x, y, Items.Items.Get("iron"));
            }

            if (random.NextInt(40) == 19)
            {
                int rand = random.NextInt(3);

                if (rand == 0)
                {
                    level.DropItem(x, y, Items.Items.Get("green clothes"));
                }
                else if (rand == 1)
                {
                    level.DropItem(x, y, Items.Items.Get("red clothes"));
                }
                else if (rand == 2)
                {
                    level.DropItem(x, y, Items.Items.Get("blue clothes"));
                }
            }

            if (random.NextInt(100) < 4)
            {
                level.DropItem(x, y, Items.Items.Get("Potato"));
            }

            base.Die();
        }
    }

}
