using MinicraftPlusSharp.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Knight : EnemyMob
    {
        private static new MobSprite[][][] sprites;
        static Knight()
        {
            sprites = new MobSprite[4][][];

            for (int i = 0; i < 4; i++)
            {
                MobSprite[][] list = MobSprite.CompileMobSpriteAnimations(0, 8 + (i * 2));
                sprites[i] = list;
            }
        }

        /**
         * Creates a knight of a given level.
         * @param lvl The knights level.
         */
        public Knight(int lvl)
            : base(lvl, sprites, 9, 100)
        {
        }

        public override void Die()
        {
            if (Settings.Get("diff").Equals("Easy"))
            {
                DropItem(1, 3, Items.Items.Get("shard"));
            }
            else
            {
                DropItem(0, 2, Items.Items.Get("shard"));
            }

            if (random.NextInt(24 / lvl / (Settings.GetIdx("diff") + 1)) == 0)
            {
                DropItem(1, 1, Items.Items.Get("key"));
            }

            base.Die();
        }
    }

}
