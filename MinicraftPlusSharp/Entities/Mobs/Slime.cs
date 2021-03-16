using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Slime : EnemyMob
    {
        private static MobSprite[][][] sprites;
        static Slime()
        {
            sprites = new MobSprite[4][1][2];
            for (int i = 0; i < 4; i++)
            {
                MobSprite[] list = MobSprite.compileSpriteList(0, 0 + (i * 2), 2, 2, 0, 2);
                sprites[i][0] = list;
            }
        }

        private int jumpTime = 0; // jumpTimer, also acts as a rest timer before the next jump

        /**
         * Creates a slime of the given level.
         * @param lvl Slime's level.
         */
        public Slime(int lvl)
            : base(lvl, sprites, 1, true, 50, 60, 40)
        {
        }

        public override void Tick()
        {
            base.Tick();

            /// jumpTime from 0 to -10 (or less) is the slime deciding where to jump.
            /// 10 to 0 is it jumping.

            if (jumpTime <= -10 && (xmov != 0 || ymov != 0))
            {
                jumpTime = 10;
            }

            jumpTime--;

            if (jumpTime == 0)
            {
                xmov = ymov = 0;
            }
        }

        public override void RandomizeWalkDir(bool byChance)
        {
            if (jumpTime > 0) return; // direction cannot be changed if slime is already jumping.
            base.RandomizeWalkDir(byChance);
        }

        public override bool Move(int xd, int yd)
        {
            bool result = base.Move(xd, yd);

            dir = Direction.DOWN;

            return result;
        }

        public override void Render(Screen screen)
        {
            int oldy = y;

            if (jumpTime > 0)
            {
                walkDist = 8; // set to jumping sprite.
                y -= 4; // raise up a bit.
            }
            else
            {
                walkDist = 0; // set to ground sprite.
            }

            dir = Direction.DOWN;

            base.Render(screen);

            y = oldy;
        }

        public override void Die()
        {
            DropItem(1, Game.IsMode("score") ? 2 : 4 - Settings.GetIdx("diff"), Items.Items.Get("slime"));

            base.Die(); // Parent death call
        }

        protected override string GetUpdateString()
        {
            string updates = base.GetUpdateString() + ";";

            updates += "jumpTime," + jumpTime;

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
                case "jumpTime":
                    jumpTime = int.Parse(val);
                    return true;
            }

            return false;
        }
    }
}
