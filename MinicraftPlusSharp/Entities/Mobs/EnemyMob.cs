using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Furniture;
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
    public class EnemyMob : MobAi
    {
        public int lvl;
        protected MobSprite[][][] lvlSprites;
        public int detectDist;

        /**
         * Constructor for a hostile (enemy) mob. The level determines what the mob does. sprites contains all the graphics and animations for the mob.
         * lvlcols is the different color the mob has depending on its level. isFactor determines if the mob's health should be affected by the level and
         * the difficulty.
         * @param lvl The mob's level.
         * @param lvlSprites The mob's sprites (ordered by level, then direction, then animation frame).
         * @param health How much health the mob has.
         * @param isFactor false if maxHealth=health, true if maxHealth=health*level*level*difficulty
         * @param detectDist The distance where the mob will detect the player and start moving towards him/her.
         * @param lifetime How many ticks this mob will live.
         * @param rwTime How long the mob will walk in a random direction. (random walk duration)
         * @param rwChance The chance of this mob will walk in a random direction (random walk chance)
         */
        public EnemyMob(int lvl, MobSprite[][][] lvlSprites, int health, bool isFactor, int detectDist, int lifetime, int rwTime, int rwChance)
            : base(lvlSprites[0], isFactor ? (lvl == 0 ? 1 : lvl * lvl) * health * (int)Math.Pow(2, Settings.getIdx("diff")) : health, lifetime, rwTime, rwChance)
        {
            this.lvl = lvl == 0 ? 1 : lvl;
            this.lvlSprites = lvlSprites.ToArray();
            this.detectDist = detectDist;
        }

        /**
         * Constructor for a hostile (enemy) mob. 
         * Lifetime will be set to 60 * Game.normSpeed.
         * @param lvl The mob's level.
         * @param lvlSprites The mob's sprites (ordered by level, then direction, then animation frame).
         * @param health How much health the mob has.
         * @param isFactor false if maxHealth=health, true if maxHealth=health*level*level*difficulty
         * @param detectDist The distance where the mob will detect the player and start moving towards him/her.
         * @param rwTime How long the mob will walk in a random direction. (random walk duration)
         * @param rwChance The chance of this mob will walk in a random direction (random walk chance)
         */
        public EnemyMob(int lvl, MobSprite[][][] lvlSprites, int health, bool isFactor, int detectDist, int rwTime, int rwChance)
            : this(lvl, lvlSprites, health, isFactor, detectDist, 60 * Updater.normSpeed, rwTime, rwChance)
        {
        }

        /**
         * Constructor for a hostile (enemy) mob.
         * isFactor=true,
         * rwTime=60,
         * rwChance=200.
         * 
         * @param lvl The mob's level.
         * @param lvlSprites The mob's sprites (ordered by level, then direction, then animation frame).
         * @param health How much health the mob has.
         * @param detectDist The distance where the mob will detect the player and start moving towards him/her.
         */
        public EnemyMob(int lvl, MobSprite[][][] lvlSprites, int health, int detectDist)
            : this(lvl, lvlSprites, health, true, detectDist, 60, 200)
        {
        }

        public override void Tick()
        {
            base.Tick();

            Player player = GetClosestPlayer();

            if (player != null && !Bed.Sleeping() && randomWalkTime <= 0 && !Game.IsMode("Creative"))
            { // checks if player is on zombie's level, if there is no time left on randonimity timer, and if the player is not in creative.
                int xd = player.x - x;
                int yd = player.y - y;

                if (xd * xd + yd * yd < detectDist * detectDist)
                {
                    /// if player is less than 6.25 tiles away, then set move dir towards player
                    int sig0 = 1; // this prevents too precise estimates, preventing mobs from bobbing up and down.
                    this.xmov = this.ymov = 0;
                    if (xd < sig0) this.xmov = -1;
                    if (xd > sig0) this.xmov = +1;
                    if (yd < sig0) this.ymov = -1;
                    if (yd > sig0) this.ymov = +1;
                }
                else
                {
                    // if the enemy was following the player, but has now lost it, it stops moving.
                    //*that would be nice, but I'll just make it move randomly instead.
                    RandomizeWalkDir(false);
                }
            }
        }

        public override void Render(Screen screen)
        {
            sprites = lvlSprites[lvl - 1];

            base.Render(screen);
        }

        protected override void TouchedBy(Entity entity)
        { // if an entity (like the player) touches the enemy mob
            base.TouchedBy(entity);
            // hurts the player, damage is based on lvl.
            if (entity is Player player)
            {
                player.Hurt(this, lvl * (Settings.Get("diff").Equals("Hard") ? 2 : 1));
            }
        }

        public override void Die()
        {
            base.Die(50 * lvl, 1);
        }

        /**
         * Determines if the mob can spawn at the giving position in the given map. 
         * @param level The level which the mob wants to spawn in.
         * @param x X map spawn coordinate.
         * @param y Y map spawn coordinate.
         * @return true if the mob can spawn here, false if not.
         */
        public static bool CheckStartPos(Level level, int x, int y)
        { // Find a place to spawn the mob
            int r = (level.depth == -4 ? (Game.IsMode("score") ? 22 : 15) : 13);

            if (!MobAi.CheckStartPos(level, x, y, 60, r))
            {
                return false;
            }

            x = x >> 4;
            y = y >> 4;

            Tile t = level.GetTile(x, y);

            if (level.depth == -4)
            {
                if (t != Tiles.Get("Obsidian"))
                {
                    return false;
                }
            }
            else if (t != Tiles.Get("Stone Door") && t != Tiles.Get("Wood Door") && t != Tiles.Get("Obsidian Door") && t != Tiles.Get("wheat") && t != Tiles.Get("farmland"))
            {
                // prevents mobs from spawning on lit tiles, farms, or doors (unless in the dungeons)
                return !level.IsLight(x, y);
            }
            else
            {
                return false;
            }

            return true;
        }

        public override int GetMaxLevel()
        {
            return lvlSprites.Length;
        }
    }

}
