using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Java;
using MinicraftPlusSharp.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities
{
    public abstract class Entity : Tickable
    {

        /* I guess I should explain something real quick. The coordinates between tiles and entities are different.
         * The world coordinates for tiles is 128x128
         * The world coordinates for entities is 2048x2048
         * This is because each tile is 16x16 pixels big
         * 128 x 16 = 2048.
         * When ever you see a ">>", it means that it is a right shift operator. This means it shifts bits to the right (making them smaller)
         * x >> 4 is the equivalent to x / (2^4). Which means it's dividing the X value by 16. (2x2x2x2 = 16)
         * xt << 4 is the equivalent to xt * (2^4). Which means it's multiplying the X tile value by 16.
         *
         * These bit shift operators are used to easily get the X & Y coordinates of a tile that the entity is standing on.
         */

        // entity coordinates are per pixel, not per tile; each tile is 16x16 entity pixels.
        protected readonly JavaRandom random = new JavaRandom();
        public int x, y; // x, y entity coordinates on the map
        private int xr, yr; // x, y radius of entity
        private bool removed; // Determines if the entity is removed from it's level; checked in Level.java
        protected Level level; // the level that the entity is on
        public int col; // current color.

        public int eid; // this is intended for multiplayer, but I think it could be helpful in single player, too. certainly won't harm anything, I think... as long as finding a valid id doesn't take long...
        private string prevUpdates = ""; // holds the last value returned from getUpdateString(), for comparison with the next call.
        private string curDeltas = ""; // holds the updates returned from the last time getUpdates() was called.
        private bool accessedUpdates = false;
        private long lastUpdate;

        /**
         * Default constructor for the Entity class.
         * Assings null/none values to the instace variables.
         * The exception is removed which is set to true, and
         * lastUpdate which is set to System.nanoTime().
         * @param xr X radius of entity.
         * @param yr Y radius of entity.
         */
        public Entity(int xr, int yr)
        { // add color to this later, in color update
            this.xr = xr;
            this.yr = yr;

            level = null;
            removed = true;
            col = 0;

            eid = -1;
            lastUpdate = JavaSystem.NanoTime();
        }

        public abstract void Render(Screen screen); // used to render the entity on screen.

        public abstract void Tick(); // used to update the entity.

        /**
         * Returns true if the entity is removed from the level, otherwise false.
         * @return removed
         */
        public bool IsRemoved()
        {
            return removed/* || level == null*/;
        }

        /**
         * Returns the level which this entity belongs in.
         * @return level
         */
        public Level GetLevel()
        {
            return level;
        }

        /** Returns a Rectangle instance using the defined bounds of the entity. */
        protected Rectangle GetBounds()
        {
            return new Rectangle(x, y, xr * 2, yr * 2, Rectangle.CENTER_DIMS);
        }
        /** returns true if this entity is found in the rectangle specified by given two coordinates. */
        public virtual bool IsTouching(Rectangle area)
        {
            return area.Intersects(GetBounds());
        }
        /** returns if this entity stops other solid entities from moving. */
        public virtual bool IsSolid()
        {
            return true;
        } // most entities are solid
        /** Determines if the given entity should prevent this entity from moving. */
        public virtual bool Blocks(Entity e)
        {
            return IsSolid() && e.IsSolid();
        }

        public virtual bool CanSwim() { return false; } // Determines if the entity can swim (extended in sub-classes)
        public virtual bool CanWool() { return false; } // This, strangely enough, determines if the entity can walk on wool; among some other things..?

        public virtual int GetLightRadius() { return 0; } // used for lanterns... and player? that might be about it, though, so idk if I want to put it here.


        /** if this entity is touched by another entity (extended by sub-classes) */
        protected virtual void TouchedBy(Entity entity) { }

        /**
         * Interacts with the entity this method is called on
         * @param player The player attacking
         * @param item The item the player attacked with
         * @param attackDir The direction to interact
         * @return If the interaction was successful
         */
        public virtual bool Interact(Player player, Item item, Direction attackDir)
        {
            return false;
        }

        /** Moves an entity horizontally and vertically. Returns whether entity was unimpeded in it's movement.  */
        public virtual bool Move(int xd, int yd)
        {
            if (Updater.saving || (xd == 0 && yd == 0))
            {
                return true; // pretend that it kept moving
            }

            bool stopped = true; // used to check if the entity has BEEN stopped, COMPLETELY; below checks for a lack of collision.

            if (Move2(xd, 0)) stopped = false; // becomes false if horizontal movement was successful.
            if (Move2(0, yd)) stopped = false; // becomes false if vertical movement was successful.

            if (!stopped)
            {
                int xt = x >> 4; // the x tile coordinate that the entity is standing on.
                int yt = y >> 4; // the y tile coordinate that the entity is standing on.
                level.GetTile(xt, yt).SteppedOn(level, xt, yt, this); // Calls the steppedOn() method in a tile's class. (used for tiles like sand (footprints) or lava (burning))
            }

            return !stopped;
        }

        /**
         * Moves the entity a long only one direction.
         * If xd != 0 then ya should be 0.
         * If xd = 0 then ya should be != 0.
         * Will throw exception otherwise.
         * @param xd Horizontal move.
         * @param yd Vertical move.
         * @return true if the move was successful, false if not.
         */
        protected bool Move2(int xd, int yd)
        {
            if (xd == 0 && yd == 0) return true; // was not stopped

            bool interact = true;//!Game.isValidClient() || this instanceof ClientTickable;

            // gets the tile coordinate of each direction from the sprite...
            int xto0 = ((x) - this.xr) >> 4; // to the left
            int yto0 = ((y) - this.yr) >> 4; // above
            int xto1 = ((x) + this.xr) >> 4; // to the right
            int yto1 = ((y) + this.yr) >> 4; // below

            // gets same as above, but after movement.
            int xt0 = ((x + xd) - this.xr) >> 4;
            int yt0 = ((y + yd) - this.yr) >> 4;
            int xt1 = ((x + xd) + this.xr) >> 4;
            int yt1 = ((y + yd) + this.yr) >> 4;

            //bool blocked = false; // if the next tile can block you.
            for (int yt = yt0; yt <= yt1; yt++)
            { // cycles through y's of tile after movement
                for (int xt = xt0; xt <= xt1; xt++)
                { // cycles through x's of tile after movement
                    if (xt >= xto0 && xt <= xto1 && yt >= yto0 && yt <= yto1)
                    {
                        continue; // skip this position if this entity's sprite is touching it
                    }
                    // tile positions that make it here are the ones that the entity will be in, but are not in now.
                    if (interact)
                    {
                        level.GetTile(xt, yt).BumpedInto(level, xt, yt, this); // Used in tiles like cactus
                    }

                    if (!level.GetTile(xt, yt).MayPass(level, xt, yt, this))
                    { // if the entity can't pass this tile...
                      //blocked = true; // then the entity is blocked
                        return false;
                    }
                }
            }

            // these lists are named as if the entity has already moved-- it hasn't, though.
            List<Entity> wasInside = level.GetEntitiesInRect(GetBounds()); // gets all of the entities that are inside this entity (aka: colliding) before moving.

            int xr = this.xr, yr = this.yr;

            if (Game.IsValidClient() && this is Player)
            {
                xr++;
                yr++;
            }

            List<Entity> isInside = level.GetEntitiesInRect(new Rectangle(x + xd, y + yd, xr * 2, yr * 2, Rectangle.CENTER_DIMS)); // gets the entities that this entity will touch once moved.

            if (interact)
            {
                foreach (Entity e in isInside)
                {
                    /// cycles through entities about to be touched, and calls touchedBy(this) for each of them.
                    if (e == this)
                    {
                        continue; // touching yourself doesn't count.
                    }

                    if (e is Player player)
                    {
                        if (this is not Player)
                        {
                            TouchedBy(player);
                        }
                    }
                    else
                    {
                        e.TouchedBy(this); // call the method. ("touch" the entity)
                    }
                }
            }

            isInside.RemoveAll(wasInside.Contains); // remove all the entities that this one is already touching before moving.

            foreach (Entity e in isInside)
            {
                if (e == this)
                {
                    continue; // can't interact with yourself
                }

                if (e.Blocks(this))
                {
                    return false; // if the entity prevents this one from movement, don't move.
                }
            }

            // finally, the entity moves!
            x += xd;
            y += yd;

            return true; // the move was successful.
        }

        /** This exists as a way to signify that the entity has been removed through player action and/or world action; basically, it's actually gone, not just removed from a level because it's out of range or something. Calls to this method are used to, say, drop items. */
        public virtual void Die()
        {
            Remove();
        }

        /** Removes the entity from the level. */
        public virtual void Remove()
        {
            if (removed && !(this is ItemEntity)) // apparently this happens fairly often with item entities.
            {
                Console.WriteLine("Note: Remove() called on removed entity: " + this);
            }

            removed = true;

            if (level == null)
            {
                Console.WriteLine("Note: Remove() called on entity with no level reference: " + GetType().Name);
            }
            else
            {
                level.Remove(this);
            }
        }

        /** This should ONLY be called by the Level class. To properly remove an entity from a level, use level.remove(entity) */
        public void Remove(Level level)
        {
            if (level != this.level)
            {
                if (Game.debug)
                {
                    Console.WriteLine("Tried to remove entity " + this + " from level it is not in: " + level + "; in level " + this.level);
                }
            }
            else
            {
                removed = true; // should already be set.
                this.level = null;
            }
        }

        /** This should ONLY be called by the Level class. To properly add an entity to a level, use level.add(entity) */
        public void SetLevel(Level level, int x, int y)
        {
            if (level == null)
            {
                Console.WriteLine("Tried to set level of entity " + this + " to a null level; Should use remove(level)");
                return;
            }
            else if (level != this.level && Game.IsValidServer() && this.level != null)
            {
                Game.server.BroadcastEntityRemoval(this, this.level, this is not Player);
            }

            this.level = level;
            removed = false;
            this.x = x;
            this.y = y;

            if (eid < 0)
            {
                eid = Network.GenerateUniqueEntityId();
            }
        }

        public bool IsWithin(int tileRadius, Entity other)
        {
            if (level == null || other.GetLevel() == null)
            {
                return false;
            }

            if (level.depth != other.GetLevel().depth)
            {
                return false; // obviously, if they are on different levels, they can't be next to each other.
            }

            double distance = Math.Abs(Math.Sqrt(Math.Pow(x - other.x, 2) + Math.Pow(y - other.y, 2))); // calculate the distance between the two entities, in entity coordinates.

            return ((int)Math.Round(distance) >> 4) <= tileRadius; // compare the distance (converted to tile units) with the specified radius.
        }

        /**
         * Returns the closest player to this entity.
         * @return the closest player.
         */
        protected Player GetClosestPlayer()
        {
            return GetClosestPlayer(true);
        }

        /**
         * Returns the closes player to this entity.
         * If this is called on a player it can return itself.
         * @param returnSelf determines if the method can return itself.
         * @return The closest player to this entity.
         */
        protected Player GetClosestPlayer(bool returnSelf)
        {
            if (this is Player player && returnSelf)
            {
                return player;
            }

            if (level == null)
            {
                return null;
            }

            return level.GetClosestPlayer(x, y);
        }

        /**
         * I think this is used to update a entity over a network.
         * The server will send a correction of this entity's state
         * which will then be updated.
         * @param deltas A string representation of the new entity state.
         */
        public void Update(string deltas)
        {
            foreach (string field in deltas.Split(";"))
            {
                string fieldName = field.Substring(0, field.IndexOf(","));
                string val = field[(field.IndexOf(",") + 1)..];
                UpdateField(fieldName, val);
            }

            if (Game.IsValidClient() && this is MobAi)
            {
                lastUpdate = JavaSystem.NanoTime();
            }
        }

        /**
         * Updates one of the entity's fields based on a string pair.
         * Used to parse data from a server.
         * @param fieldName Which variable is being updated.
         * @param val The new value.
         * @return true if a variable was updated, false if not.
         */
        protected virtual bool UpdateField(string fieldName, string val)
        {
            switch (fieldName)
            {
                case "eid": eid = int.Parse(val); return true;
                case "x": x = int.Parse(val); return true;
                case "y": y = int.Parse(val); return true;
                case "level":
                    if (val.Equals("null")) return true; // this means no level.
                    Level newLvl = World.levels[int.Parse(val)];
                    if (newLvl != null && level != null)
                    {
                        if (newLvl.depth == level.depth)
                        {
                            return true;
                        }

                        level.Remove(this);
                        newLvl.Add(this);
                    }
                    return true;
            }
            return false;
        }

        /// I think I'll make these "getUpdates()" methods be an established thing, that returns all the things that can change that you need to account for when updating entities across a server.
        /// by extension, the update() method should always account for all the variables specified here.
        /**
         * Converts this entity to a string representation which can be sent to
         * a server or client.
         * @return Networking string representation of this entity.
         */
        protected virtual string GetUpdateString()
        {
            return "x," + x + ";"
            + "y," + y + ";"
            + "level," + (level == null ? "null" : World.LvlIdx(level.depth));
        }

        /**
         * Returns a string representation of this entity.
         * @param fetchAll true if all variables should be returned, false if only the ones who have changed should be returned.
         * @return Networking string representation of this entity.
         */
        public string GetUpdates(bool fetchAll)
        {
            if (accessedUpdates)
            {
                return fetchAll ? prevUpdates : curDeltas;
            }
            else
            {
                return fetchAll ? GetUpdateString() : GetUpdates();
            }
        }

        /**
         * Determines what has been updated and only return that.
         * @return String representation of all the variables which has changed since last time.
         */
        public string GetUpdates()
        {
            // if the updates have already been fetched and written, but not flushed, then just return those.
            if (accessedUpdates) return curDeltas;
            else accessedUpdates = true; // after this they count as accessed.

            /// first, get the current string of values, which includes any subclasses.
            string updates = GetUpdateString();

            if (this.prevUpdates.Length == 0)
            {
                // if there were no values saved last call, our job is easy. But this is only the case the first time this is run.
                this.prevUpdates = curDeltas = updates; // set the update field for next time
                return updates; // and we're done!
            }

            /// if we did have updates last time, then save them as an array, before overwriting the update field for next time.
            string[] curUpdates = updates.Split(";");
            string[] prevUpdates = this.prevUpdates.Split(";");
            this.prevUpdates = updates;

            /// now, we have the current values, and the previous values, as arrays of key-value pairs sep. by commas. Now, the goal is to separate which are actually *updates*, meaning they are different from last time.

            StringBuilder deltas = new StringBuilder();
            for (int i = 0; i < curUpdates.Length; i++)
            { // b/c the string always contains the same number of pairs (and the same keys, in the same order), the indexes of cur and prev updates will be the same.
                /// loop though each of the updates this call. If it is different from the last one, then add it to the list.
                if (!curUpdates[i].Equals(prevUpdates[i]))
                {
                    deltas.Append(curUpdates[i]).Append(";");
                }
            }

            curDeltas = deltas.ToString();

            if (curDeltas.Length > 0) curDeltas = curDeltas[0..^1]; // cuts off extra ";"

            return curDeltas;
        }

        /// this marks the entity as having a new state to fetch.
        public void FlushUpdates()
        {
            accessedUpdates = false;
        }

        public override string ToString()
        {
            return GetType().Name + GetDataPrints();
        }

        protected virtual List<string> GetDataPrints()
        {
            List<string> prints = new();
            prints.Add("eid=" + eid);
            return prints;
        }

        public override bool Equals(object other)
        {
            return other is Entity && GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return eid;
        }
    }
}
