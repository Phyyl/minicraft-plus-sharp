using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Cow : PassiveMob
    {
        private static new MobSprite[][] sprites = MobSprite.CompileMobSpriteAnimations(0, 26);

        /**
         * Creates the cow with the right sprites and color.
         */
        public Cow()
            : base(sprites, 5)
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
                min = 0; max = 1;
            }

            DropItem(min, max, Items.Items.Get("leather"), Items.Items.Get("raw beef"));

            base.Die();
        }
    }

}
