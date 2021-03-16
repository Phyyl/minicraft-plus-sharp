using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Levels.Tiles;
using MinicraftPlusSharp.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Markup;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Entities.Furniture;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public abstract class Mob : Entity
    {
        protected MobSprite[][] sprites; // This contains all the mob's sprites, sorted first by direction (index corresponding to the dir variable), and then by walk animation state.
        public int walkDist = 0; // How far we've walked currently, incremented after each movement. This is used to change the sprite; "(walkDist >> 3) & 1" switches between a value of 0 and 1 every 8 increments of walkDist.

        public Direction dir = Direction.DOWN; // The direction the mob is facing, used in attacking and rendering. 0 is down, 1 is up, 2 is left, 3 is right
        public int hurtTime = 0; // A delay after being hurt, that temporarily prevents further damage for a short time
        private int xKnockback, yKnockback; // The amount of vertical/horizontal knockback that needs to be inflicted, if it's not 0, it will be moved one pixel at a time.
        public int health;
        public readonly int maxHealth; // The amount of health we currently have, and the maximum.
        public int walkTime;
        public int speed;
        public int tickTime = 0; // Incremented whenever tick() is called, is effectively the age in ticks

        /**
         * Default constructor for a mob.
         * Default x radius is 4, and y radius is 3.
         * @param sprites All of this mob's sprites.
         * @param health The mob's max health.
         */
        public Mob(MobSprite[][] sprites, int health)
            : base(4, 3)
        {
            this.sprites = sprites;
            this.health = this.maxHealth = health;
            walkTime = 1;
            speed = 1;
        }

        /**
         * Updates the mob.
         */
        public override void Tick()
        {
            tickTime++; // Increment our tick counter

            if (IsRemoved())
            {
                return;
            }

            if (level != null && level.GetTile(x >> 4, y >> 4) == Tiles.Get("lava")) // If we are trying to swim in lava
            {
                Hurt(Tiles.Get("lava"), x, y, 4); // Inflict 4 damage to ourselves, sourced from the lava Tile, with the direction as the opposite of ours.
            }

            if (health <= 0)
            {
                Die(); // die if no health
            }

            if (hurtTime > 0)
            {
                hurtTime--; // If a timer preventing damage temporarily is set, decrement it's value
            }

            /// The code below checks the direction of the knockback, moves the Mob accordingly, and brings the knockback closer to 0.
            int xd = 0, yd = 0;
            if (xKnockback != 0)
            {
                xd = (int)Math.Ceiling(xKnockback / 2f);
                xKnockback -= xKnockback / Math.Abs(xKnockback);
            }
            if (yKnockback != 0)
            {
                yd = (int)Math.Ceiling(yKnockback / 2f);
                yKnockback -= yKnockback / Math.Abs(yKnockback);
            }

            // if the player moved via knockback, update the server
            if ((xd != 0 || yd != 0) && Game.IsConnectedClient() && this == Game.player)
            {
                Game.client.move((Player)this, x + xd, y + yd);
            }

            Move(xd, yd, false);
        }

        public override bool Move(int xd, int yd) // Move the mob, overrides from Entity
        {
            return Move(xd, yd, true);
        }

        private bool Move(int xd, int yd, bool changeDir)
        { // knockback shouldn't change mob direction
            if (level == null) return false; // stopped b/c there's no level to move in!

            int oldxt = x >> 4;
            int oldyt = y >> 4;

            if (!(Game.IsValidServer() && this is RemotePlayer))
            { // this will be the case when the client has sent a move packet to the server. In this case, we DO want to always move.
              // these should return true b/c the mob is still technically moving; these are just to make it move *slower*.
                if (tickTime % 2 == 0 && (IsSwimming() || (!(this is Player) && IsWooling())))
                {
                    return true;
                }

                if (tickTime % walkTime == 0 && walkTime > 1)
                {
                    return true;
                }
            }

            bool moved = true;

            if (hurtTime == 0 || this is Player) // If a mobAi has been hurt recently and hasn't yet cooled down, it won't perform the movement (by not calling super)
            {
                if (xd != 0 || yd != 0)
                {
                    if (changeDir)
                    {
                        dir = Direction.GetDirection(xd, yd); // set the mob's direction; NEVER set it to NONE
                    }

                    walkDist++;
                }

                // this part makes it so you can't move in a direction that you are currently being knocked back from.
                if (xKnockback != 0)
                {
                    xd = Math.CopySign(xd, xKnockback) * -1 != xd ? xd : 0; // if xKnockback and xd have different signs, do nothing, otherwise, set xd to 0.
                }

                if (yKnockback != 0)
                {
                    yd = Math.CopySign(yd, yKnockback) * -1 != yd ? yd : 0; // same as above.
                }

                moved = base.Move(xd, yd); // Call the move method from Entity
            }

            if (Game.IsValidServer() && (xd != 0 || yd != 0))
            {
                UpdatePlayers(oldxt, oldyt);
            }

            return moved;
        }

        public void UpdatePlayers(int oldxt, int oldyt)
        {
            if (!Game.IsValidServer())
            {
                return;
            }

            List<RemotePlayer> prevPlayers = Game.server.GetPlayersInRange(level, oldxt, oldyt, true);
            List<RemotePlayer> activePlayers = Game.server.GetPlayersInRange(this, true);

            for (int i = 0; i < prevPlayers.Count; i++)
            {
                if (activePlayers.Contains(prevPlayers[i]))
                {
                    var p = prevPlayers[i];

                    prevPlayers.Remove(p);
                    activePlayers.Remove(p);

                    i--;
                }
            }

            for (int i = 0; i < activePlayers.Count; i++)
            {
                if (prevPlayers.Contains(activePlayers[i]))
                {
                    var p = activePlayers[i];

                    activePlayers.Remove(p);
                    prevPlayers.Remove(p);

                    i--;
                }
            }

            // the lists should now only contain players that are now out of range, and players that are just now in range.
            foreach (RemotePlayer rp in prevPlayers)
            {
                Game.server.GetAssociatedThread(rp).SendEntityRemoval(this.eid);
            }

            foreach (RemotePlayer rp in activePlayers)
            {
                Game.server.GetAssociatedThread(rp).SendEntityAddition(this);
            }
        }

        private bool IsWooling()
        { // supposed to walk at half speed on wool
            if (level == null)
            {
                return false;
            }

            Tile tile = level.GetTile(x >> 4, y >> 4);
            return tile == Tiles.Get("wool");
        }

        /**
         * Checks if this Mob is currently on a light tile; if so, the mob sprite is brightened.
         * @return true if the mob is on a light tile, false if not.
         */
        public bool IsLight()
        {
            return level == null ? false : level.IsLight(x >> 4, y >> 4);
        }

        /**
         * Checks if the mob is swimming (standing on a liquid tile).
         * @return true if the mob is swimming, false if not.
         */
        public bool IsSwimming()
        {
            if (level == null)
            {
                return false;
            }

            Tile tile = level.GetTile(x >> 4, y >> 4); // Get the tile the mob is standing on (at x/16, y/16)
            return tile == Tiles.Get("water") || tile == Tiles.Get("lava"); // Check if the tile is liquid, and return true if so
        }

        /**
         * Do damage to the mob this method is called on.
         * @param tile The tile that hurt the player
         * @param x The x position of the mob
         * @param y The x position of the mob
         * @param damage The amount of damage to hurt the mob with
         */
        public void Hurt(Tile tile, int x, int y, int damage) // Hurt the mob, when the source of damage is a tile
        {
            Direction attackDir = Direction.GetDirection(dir.GetDir() ^ 1); // Set attackDir to our own direction, inverted. XORing it with 1 flips the rightmost bit in the variable, this effectively adds one when even, and subtracts one when odd.
            if (!(tile == Tiles.Get("lava") && this is Player player && player.potioneffects.ContainsKey(PotionType.Lava)))
            {
                DoHurt(damage, tile.MayPass(level, x, y, this) ? Direction.NONE : attackDir); // Call the method that actually performs damage, and set it to no particular direction
            }
        }

        /**
         * Do damage to the mob this method is called on.
         * @param mob The mob that hurt this mob
         * @param damage The amount of damage to hurt the mob with
         */
        public void Hurt(Mob mob, int damage)
        {
            Hurt(mob, damage, GetAttackDir(mob, this));
        }

        /**
         * Do damage to the mob this method is called on.
         * @param mob The mob that hurt this mob
         * @param damage The amount of damage to hurt the mob with
         * @param attackDir The direction this mob was attacked from
         */
        public void Hurt(Mob mob, int damage, Direction attackDir)
        { // Hurt the mob, when the source is another mob
            if (mob is Player && Game.IsMode("creative") && mob != this)
            {
                DoHurt(health, attackDir); // kill the mob instantly
            }

            else
            {
                DoHurt(damage, attackDir); // Call the method that actually performs damage, and use our provided attackDir
            }
        }

        public void Hurt(Tnt tnt, int dmg)
        {
            DoHurt(dmg, GetAttackDir(tnt, this));
        }

        /**
         * Hurt the mob, based on only damage and a direction
         * This is overridden in Player.java
         * @param damage The amount of damage to hurt the mob with
         * @param attackDir The direction this mob was attacked from
         */
        protected virtual void DoHurt(int damage, Direction attackDir)
        {
            if (IsRemoved() || hurtTime > 0)
            {
                return; // If the mob has been hurt recently and hasn't cooled down, don't continue
            }

            health -= damage; // Actually change the health
                              // add the knockback in the correct direction
            xKnockback = attackDir.GetX() * 6;
            yKnockback = attackDir.GetY() * 6;
            hurtTime = 10; // Set a delay before we can be hurt again
        }

        /**
         * Restores health to this mob.
         * @param heal How much health is restored.
         */
        public void Heal(int heal)
        { // Restore health on the mob
            if (hurtTime > 0) return; // If the mob has been hurt recently and hasn't cooled down, don't continue

            level.Add(new TextParticle("" + heal, x, y, Color.GREEN)); // Add a text particle in our level at our position, that is green and displays the amount healed
            health += heal; // Actually add the amount to heal to our current health
            if (health > maxHealth) health = maxHealth; // If our health has exceeded our maximum, lower it back down to said maximum
        }

        protected static Direction GetAttackDir(Entity attacker, Entity hurt)
        {
            return Direction.GetDirection(hurt.x - attacker.x, hurt.y - attacker.y);
        }

        protected override string GetUpdateString()
        {
            string updates = base.GetUpdateString() + ";";
            updates += "dir," + dir.Ordinal +
            ";health," + health +
            ";hurtTime," + hurtTime;

            return updates;
        }

        protected override bool UpdateField(string field, string val)
        {
            if (field.Equals("x") || field.Equals("y"))
            {
                walkDist++;
            }

            if (base.UpdateField(field, val))
            {
                return true;
            }

            switch (field)
            {
                case "dir": dir = Direction.All[int.Parse(val)]; return true;
                case "health": health = int.Parse(val); return true;
                case "hurtTime": hurtTime = int.Parse(val); return true;
            }

            return false;
        }
    }
}
