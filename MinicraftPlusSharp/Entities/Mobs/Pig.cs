using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Pig : PassiveMob
    {
        private static new MobSprite[][] sprites = MobSprite.compileMobSpriteAnimations(0, 30);

        /**
         * Creates a pig.
         */
        public Pig()
            : base(sprites)
        {
        }

        public override void Die()
        {
            int min = 0, max = 0;

            if (Settings.Get("diff").Equals("Easy"))
            {
                min = 1; max = 3;
            }

            if (Settings.Get("diff").Equals("Normal"))
            {
                min = 1; max = 2;
            }

            if (Settings.Get("diff").Equals("Hard"))
            {
                min = 0; max = 2;
            }

            DropItem(min, max, Items.Items.Get("raw pork"));

            base.Die();
        }
    }
}
