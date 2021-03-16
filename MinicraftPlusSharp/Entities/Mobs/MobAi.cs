using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public abstract class MobAi : Mob
    {
        public int randomWalkTime, randomWalkChance, randomWalkDuration;
        public int xmov, ymov;
        private int lifetime;
        protected int age = 0; // Not private because it is used in Sheep.java.

        private bool slowtick = false;

        /**
         * Constructor for a mob with an ai.
         * @param sprites All of this mob's sprites.
         * @param maxHealth Maximum health of the mob.
         * @param lifetime How many ticks this mob can live before its removed.
         * @param rwTime How long the mob will walk in a random direction. (random walk duration)
         * @param rwChance The chance of this mob will walk in a random direction (random walk chance)
         */
        protected MobAi(MobSprite[][] sprites, int maxHealth, int lifetime, int rwTime, int rwChance)
            : base(sprites, maxHealth)
        {
            this.lifetime = lifetime;
            randomWalkTime = 0;
            randomWalkDuration = rwTime;
            randomWalkChance = rwChance;
            xmov = 0;
            ymov = 0;
            walkTime = 2;
        }

        /**
         * Checks if the mob should sleep this tick.
         * @return true if mob should sleep, false if not.
         */
        protected bool SkipTick()
        {
            return slowtick && (tickTime + 1) % 4 == 0;
        }

        public override void Tick()
        {
            base.Tick();

            if (lifetime > 0)
            {
                age++;

                if (age > lifetime)
                {
                    Remove();
                    return;
                }
            }

            if (GetLevel() != null)
            {
                bool foundPlayer = false;

                foreach (Player p in level.GetPlayers())
                {
                    if (p.IsWithin(8, this) && p.potioneffects.ContainsKey(PotionType.Time))
                    {
                        foundPlayer = true;
                        break;
                    }
                }

                slowtick = foundPlayer;
            }

            if (SkipTick())
            {
                return;
            }

            if (Move(xmov * speed, ymov * speed))
            {
                xmov = 0;
                ymov = 0;
            }

            if (random.NextInt(randomWalkChance) == 0)
            { // if the mob could not or did not move, or a random small chance occurred...
                randomizeWalkDir(true); // set random walk direction.
            }

            if (randomWalkTime > 0) randomWalkTime--;
        }

        public override void Render(Screen screen)
        {
            int xo = x - 8;
            int yo = y - 11;

            MobSprite curSprite = sprites[dir.GetDir()][(walkDist >> 3) % sprites[dir.GetDir()].Length];

            if (hurtTime > 0)
            {
                curSprite.Render(screen, xo, yo, true);
            }
            else
            {
                curSprite.Render(screen, xo, yo);
            }
        }

        public override bool Move(int xd, int yd)
        {
            //noinspection SimplifiableIfStatement
            if (Game.IsValidClient())
            {
                return false; // client mobAi's should not move at all.
            }

            return base.Move(xd, yd);
        }

        protected override void DoHurt(int damage, Direction attackDir)
        {
            if (IsRemoved() || hurtTime > 0)
            {
                return; // If the mob has been hurt recently and hasn't cooled down, don't continue
            }

            Player player = GetClosestPlayer();

            if (player != null)
            { // If there is a player in the level
                /// play the hurt sound only if the player is less than 80 entity coordinates away; or 5 tiles away.
                int xd = player.x - x;
                int yd = player.y - y;
                if (xd * xd + yd * yd < 80 * 80)
                {
                    Sound.monsterHurt.Play();
                }
            }

            level.Add(new TextParticle("" + damage, x, y, Color.RED)); // Make a text particle at this position in this level, bright red and displaying the damage inflicted

            base.DoHurt(damage, attackDir);
        }

        public override bool CanWool()
        {
            return true;
        }

        /**
         * Sets the mob to walk in a random direction for a given amount of time.
         * @param byChance true if the mob should always get a new direction to walk, false if 
         * there should be a chance that the mob moves.
         */
        public virtual void RandomizeWalkDir(bool byChance)
        { // bool specifies if this method, from where it's called, is called every tick, or after a random chance.
            if (!byChance && random.NextInt(randomWalkChance) != 0)
            {
                return;
            }

            randomWalkTime = randomWalkDuration; // set the mob to walk about in a random direction for a time

            // set the random direction; randir is from -1 to 1.
            xmov = (random.NextInt(3) - 1);
            ymov = (random.NextInt(3) - 1);
        }

        /**
         * Adds some items to the level.
         * @param mincount Least amount of items to add.
         * @param maxcount Most amount of items to add.
         * @param items Which items should be added.
         */
        protected void DropItem(int mincount, int maxcount, params Item[] items)
        {
            int count = random.NextInt(maxcount - mincount + 1) + mincount;

            for (int i = 0; i < count; i++)
            {
                level.DropItem(x, y, items);
            }
        }

        /**
         * Determines if a friendly mob can spawn here.
         * @param level The level the mob is trying to spawn in.
         * @param x X map coordinate of spawn.
         * @param y Y map coordinate of spawn.
         * @param playerDist Max distance from the player the mob can be spawned in.
         * @param soloRadius How far out can there not already be any entities.
         * This is multiplied by the monster density of the level
         * @return true if the mob can spawn, false if not.
         */
        protected static bool CheckStartPos(Level level, int x, int y, int playerDist, int soloRadius)
        {
            Player player = level.GetClosestPlayer(x, y);

            if (player != null)
            {
                int xd = player.x - x;
                int yd = player.y - y;

                if (xd * xd + yd * yd < playerDist * playerDist) return false;
            }

            int r = level.monsterDensity * soloRadius; // get no-mob radius

            //noinspection SimplifiableIfStatement
            if (level.GetEntitiesInRect(new Rectangle(x, y, r * 2, r * 2, Rectangle.CENTER_DIMS)).Count > 0)
            {
                return false;
            }

            return level.GetTile(x >> 4, y >> 4).MaySpawn(); // the last check.
        }

        /**
         * Returns the maximum level of this mob.
         * @return max level of the mob.
         */
        public abstract int GetMaxLevel();

        protected void Die(int points)
        {
            Die(points, 0);
        }

        protected void Die(int points, int multAdd)
        {
            foreach (Player p in level.GetPlayers())
            {
                p.AddScore(points); // add score for mob death
                
                if (multAdd != 0)
                {
                    p.AddMultiplier(multAdd);
                }
            }

            base.Die();
        }
    }
}
