using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class Sheep : PassiveMob
    {
        private static new readonly MobSprite[][] sprites = MobSprite.CompileMobSpriteAnimations(0, 28);
        private static readonly MobSprite[][] cutSprites = MobSprite.CompileMobSpriteAnimations(0, 24);

        private static readonly int WOOL_GROW_TIME = 3 * 60 * Updater.normSpeed; // Three minutes

        public bool cut = false;
        private int ageWhenCut = 0;

        /**
         * Creates a sheep entity.
         */
        public Sheep()
            : base(sprites)
        {
        }

        public override void Render(Screen screen)
        {
            int xo = x - 8;
            int yo = y - 11;

            MobSprite[][] curAnim = cut ? cutSprites : sprites;
            MobSprite curSprite = curAnim[dir.GetDir()][(walkDist >> 3) % curAnim[dir.GetDir()].Length];

            if (hurtTime > 0)
            {
                curSprite.render(screen, xo, yo, true);
            }
            else
            {
                curSprite.render(screen, xo, yo);
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (age - ageWhenCut > WOOL_GROW_TIME)
            {
                cut = false;
            }
        }

        public override bool Interact(Player player, Item item, Direction attackDir)
        {
            if (cut)
            {
                return false;
            }

            if (item is ToolItem toolItem)
            {
                if (toolItem.type == ToolType.Shear)
                {
                    cut = true;

                    DropItem(1, 3, Items.Items.Get("Wool"));

                    ageWhenCut = age;

                    return true;
                }
            }

            return false;
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

            if (!cut)
            {
                DropItem(min, max, Items.Items.Get("wool"));
            }

            DropItem(min, max, Items.Items.Get("Raw Beef"));

            base.Die();
        }
    }
}
