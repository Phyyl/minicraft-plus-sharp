using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{

    public class Snake : EnemyMob
    {
        private static new readonly MobSprite[][][] sprites;

        static Snake()
        {
            sprites = new MobSprite[4][][];

            for (int i = 0; i < 4; i++)
            {
                MobSprite[][] list = MobSprite.CompileMobSpriteAnimations(8, 8 + (i * 2));
                sprites[i] = list;
            }
        }

        public Snake(int lvl)
            : base(lvl, sprites, lvl > 1 ? 8 : 7, 100)
        {
        }

        protected override void TouchedBy(Entity entity)
        {
            if (entity is Player player)
            {
                int damage = lvl + Settings.GetIdx("diff");

                player.Hurt(this, damage);
            }
        }

        public override void Die()
        {
            int num = Settings.Get("diff").Equals("Hard") ? 1 : 0;

            DropItem(num, num + 1, Items.Items.Get("scale"));

            if (random.NextInt(24 / lvl / (Settings.GetIdx("diff") + 1)) == 0)
            {
                DropItem(1, 1, Items.Items.Get("key"));
            }

            base.Die();
        }
    }
}
