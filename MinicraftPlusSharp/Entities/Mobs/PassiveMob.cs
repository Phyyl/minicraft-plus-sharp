using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels;
using MinicraftPlusSharp.Levels.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class PassiveMob : MobAi
    {
        protected int color;

        /**
         * Constructor for a non-hostile (passive) mob.
         * healthFactor = 3.
         * @param sprites The mob's sprites.
         */
        public PassiveMob(MobSprite[][] sprites)
            : this(sprites, 3)
        {
        }

        /**
         * Constructor for a non-hostile (passive) mob.
         * @param sprites The mob's sprites.
         * @param healthFactor Determines the mobs health. Will be multiplied by the difficulty
         * and then added with 5.
         */
        public PassiveMob(MobSprite[][] sprites, int healthFactor)
            : base(sprites, 5 + healthFactor * Settings.GetIdx("diff"), 5 * 60 * Updater.normSpeed, 45, 40)
        {
        }

        public override void Render(Screen screen)
        {
            base.Render(screen);
        }

        public override void RandomizeWalkDir(bool byChance)
        {
            if (xmov == 0 && ymov == 0 && random.NextInt(5) == 0 || byChance || random.NextInt(randomWalkChance) == 0)
            {
                randomWalkTime = randomWalkDuration;
                // multiple at end ups the chance of not moving by 50%.
                xmov = (random.NextInt(3) - 1) * random.NextInt(2);
                ymov = (random.NextInt(3) - 1) * random.NextInt(2);
            }
        }

        public override void Die()
        {
            base.Die(15);
        }

        /**
         * Checks a given position in a given level to see if the mob can spawn there.
         * Passive mobs can only spawn on grass or flower tiles.
         * @param level The level which the mob wants to spawn in.
         * @param x X map spawn coordinate.
         * @param y Y map spawn coordinate.
         * @return true if the mob can spawn here, false if not.
         */
        public static bool CheckStartPos(Level level, int x, int y)
        {
            int r = (Game.IsMode("score") ? 22 : 15) + (Updater.GetTime() == Updater.Time.Night ? 0 : 5); // get no-mob radius by

            if (!MobAi.CheckStartPos(level, x, y, 80, r))
            {
                return false;
            }

            Tile tile = level.GetTile(x >> 4, y >> 4);

            return tile == Tiles.Get("grass") || tile == Tiles.Get("flower");

        }

        public override int GetMaxLevel()
        {
            return 1;
        }
    }

}
