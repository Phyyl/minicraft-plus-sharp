using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Furniture;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Mobs
{
    public class RemotePlayer : Player, ClientTickable
    {

        /// these are used by the server to determine the distance limit for an entity/tile to be updated/added for a given player.
        private static readonly int xSyncRadius = 12;
        private static readonly int ySyncRadius = 10;
        private static readonly int entityTrackingBuffer = 0;

        private string username = "";
        private readonly IPAddress ipAddress;
        private readonly int port;

        public RemotePlayer(Player previous, IPAddress ip, int port)
            : this(previous, false, ip, port)
        {
        }

        public RemotePlayer(Player previous, bool isMainPlayer, IPAddress ip, int port)
            : base(previous, (isMainPlayer ? Game.input : new InputHandler()))
        {
            this.ipAddress = ip;
            this.port = port;
        }

        public RemotePlayer(bool isMainPlayer, RemotePlayer model)
            : this(model, isMainPlayer, model.ipAddress, model.port)
        {
            eid = model.eid;
            SetUsername(model.GetUsername());
        }

        public void SetUsername(string username)
        {
            this.username = username;
        }

        public string GetUsername()
        {
            return username;
        }

        public IPAddress GetIpAddress()
        {
            return ipAddress;
        }

        public string GetData()
        {
            return username + ":" + ipAddress.getCanonicalHostName() + ":" + port;
        }

        public void ClientTick()
        {
            // a minimal thing for render update purposes.
            if (attackTime > 0)
            {
                attackTime--;

                if (attackTime == 0)
                {
                    attackItem = null; // null the attackItem once we are done attacking.
                }
            }
            if (hurtTime > 0) hurtTime--; // to update the attack animation.
        }

        public override void ResetMultiplier()
        {
            base.ResetMultiplier();

            if (Game.IsMode("score") && Game.IsValidServer())
            {
                Game.server.GetAssociatedThread(this).sendEntityUpdate(this, "mult," + GetMultiplier());
            }
        }

        public override void AddMultiplier(int value)
        {
            base.AddMultiplier(value);

            if (Game.IsMode("score") && Game.IsValidServer())
            {
                Game.server.GetAssociatedThread(this).sendEntityUpdate(this, "mult," + GetMultiplier());
            }
        }

        public override void AddScore(int points)
        {
            base.AddScore(points);

            if (Game.IsMode("score") && Game.IsValidServer())
            {
                Game.server.GetAssociatedThread(this).sendEntityUpdate(this, "score," + GetScore());
            }
        }

        /// this is simply to broaden the access permissions.
        public override void Attack()
        {
            base.Attack();
        }

        public override bool Move(int xd, int yd)
        {
            int oldxt = x >> 4, oldyt = y >> 4;

            bool moved = base.Move(xd, yd);

            if (!(oldxt == x >> 4 && oldyt == y >> 4) && Game.IsConnectedClient() && this == Game.player)
            {
                // if moved (and is client), then check any tiles no longer loaded, and remove any entities on them.
                UpdateSyncArea(oldxt, oldyt);
            }

            return moved;
        }

        public override void Render(Screen screen)
        {
            base.Render(screen);

            new FontStyle(Color.Get(1, 204)).SetShadowType(Color.BLACK, true).SetXPos(x - Font.TextWidth(username) / 2).SetYPos(y - 20).Draw(username, screen); // draw the username of the player above their head
        }

        public override void Die()
        {
            if (Game.IsValidServer())
            {
                Game.server.GetAssociatedThread(this).SendPlayerHurt(eid, health, Direction.NONE);
            }
            else
            {
                base.Die();
            }
        }

        /// this determines if something at a given coordinate should be synced to this client, or if it is too far away to matter.
        public bool ShouldSync(int xt, int yt, Level level)
        {
            return ShouldSync(level, xt, yt, 0);
        }

        public bool ShouldTrack(int xt, int yt, Level level)
        {
            return ShouldSync(level, xt, yt, entityTrackingBuffer); /// this means that there is one tile past the syncRadii in all directions, which marks the distance at which entities are added or removed.
        }

        private bool ShouldSync(Level level, int xt, int yt, int offset)
        { // IDEA make this isWithin(). Decided not to b/c different x and y radii.
            if (level == null)
            {
                return false;
            }

            if (GetLevel() == null)
            {
                if (!Bed.InBed(this))
                {
                    return false; // no excuse
                }

                if (level != Bed.GetBedLevel(this))
                {
                    return false;
                }
            }
            else if (level != GetLevel())
            {
                return false;
            }

            int px = x >> 4, py = y >> 4;
            int xdist = Math.Abs(xt - px);
            int ydist = Math.Abs(yt - py);
            
            return xdist <= xSyncRadius + offset && ydist <= ySyncRadius + offset;
        }

        protected override List<string> GetDataPrints()
        {
            List<string> prints = base.GetDataPrints();

            prints.Insert(0, "user=" + username);

            return prints;
        }

        public void UpdateSyncArea(int oldxt, int oldyt)
        {
            if (level == null)
            {
                Console.Error.WriteLine("CLIENT: Couldn't check world around player because player has no level: " + this);
                return;
            }

            int xt = x >> 4;
            int yt = y >> 4;
            if (xt == oldxt && yt == oldyt) // no change is needed.
            {
                return;
            }

            bool isServer = Game.IsValidServer();
            bool isClient = Game.IsConnectedClient();

            int xr = xSyncRadius + entityTrackingBuffer;
            int yr = ySyncRadius + entityTrackingBuffer;

            int xt0, yt0, xt1, yt1;
            if (isServer)
            {
                xt0 = oldxt;
                yt0 = oldyt;
                xt1 = xt;
                yt1 = yt;
            }
            else if (isClient)
            {
                xt0 = xt;
                yt0 = yt;
                xt1 = oldxt;
                yt1 = oldyt;
            }
            else
            {
                Console.Error.WriteLine("ERROR: RemotePlayer sync method called when game is not client or server. Could be a disconnected client.");
                return;
            }

            /// the Math.mins and maxes make it so it doesn't try to update tiles outside of the level bounds.
            int xmin = Math.Max(xt1 - xr, 0);
            int xmax = Math.Min(xt1 + xr, level.w - 1);
            int ymin = Math.Max(yt1 - yr, 0);
            int ymax = Math.Min(yt1 + yr, level.h - 1);

            List<Entity> loadableEntites = level.GetEntitiesInTiles(xmin, ymin, xmax, ymax);

            for (int y = ymin; y <= ymax; y++)
            {
                for (int x = xmin; x <= xmax; x++)
                {
                    /// server loops through current tiles, and filters out old tiles, so only new tiles are left.
                    /// client loops through old tiles, and filters out current tiles, so only old tiles are left.
                    if (xt0 < 0 || yt0 < 0 || x > xt0 + xr || x < xt0 - xr || y > yt0 + yr || y < yt0 - yr)
                    {

                        /// SERVER NOTE: don't worry about removing entities that go to unloaded tiles; the client will do that. Now, as for mobs (or players) that wander out of or into a player's loaded tiles without the player moving, the Mob class deals with that.

                        foreach (Entity e in loadableEntites)
                        {
                            if (e != this && e.x >> 4 == x && e.y >> 4 == y)
                            {
                                if (isServer)
                                {
                                    /// send any entities on that new tile to be added.
                                    Game.server.GetAssociatedThread(this).SendEntityAddition(e);
                                }

                                if (isClient)
                                {
                                    /// remove each entity on that tile.
                                    e.Remove();
                                }
                            }
                        }
                    } // end range checker conditional
                }
            } // end tile iteration loops
        }
    }
}
