using MinicraftPlusSharp.Core;
using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using MinicraftPlusSharp.Items;
using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities
{
    public class ItemEntity : Entity, ClientTickable
    {
        private int lifeTime; // the life time of this entity in the level
        private double xa, ya, za; // the x, y, and z accelerations.
        private double xx, yy, zz; // the x, y, and z coordinates; in double precision.
        public Item item; // the item that this entity is based off of.
        private int time = 0; // time it has lasted in the level

        // solely for multiplayer use.
        private bool pickedUp = false;
        private long pickupTimestamp;

        /**
         * Creates an item entity of the item item at position (x,y) with size 2*2.
         * @param item Item to add as item entity
         * @param x position on map
         * @param y position on map
         */
        public ItemEntity(Item item, int x, int y)
            : base(2, 2)
        {
            this.item = item;
            this.x = x;
            this.y = y;
            xx = x;
            yy = y;

            zz = 2;
            // random direction for each acceleration
            xa = random.NextGaussian() * 0.3;
            ya = random.NextGaussian() * 0.2;
            za = random.NextFloat() * 0.7 + 1;

            lifeTime = 60 * 10 + random.NextInt(70); // sets the lifetime of the item. min = 600 ticks, max = 669 ticks.
        }

        /**
         * Creates an item entity of the item item at position (x,y) with size 2*2.
         * @param item Item to add as item entity.
         * @param x position on map
         * @param y position on map
         * @param zz z position?
         * @param lifetime lifetime (in ticks) of the entity.
         * @param time starting time (in ticks) of the entity.
         * @param xa x velocity
         * @param ya y velocity 
         * @param za z velocity?
         */
        public ItemEntity(Item item, int x, int y, double zz, int lifetime, int time, double xa, double ya, double za)
            : this(item, x, y)
        {
            this.lifeTime = lifetime;
            this.time = time;
            this.zz = zz;
            this.xa = xa;
            this.ya = ya;
            this.za = za;
        }

        /**
         * Returns a string representation of the itementity
         * @return string representation of this entity
         */
        public string GetData()
        {
            return string.Join(":", (new string[] { item.GetData(), zz + "", lifeTime + "", time + "", xa + "", ya + "", za + "" }));
        }

        public override void Tick()
        {
            time++;

            if (time >= lifeTime)
            { // if the time is larger or equal to lifeTime then...
                Remove(); // remove from the world

                return; // skip the rest of the code
            }

            // moves each coordinate by the its acceleration
            xx += xa;
            yy += ya;
            zz += za;

            if (zz < 0)
            { // if z pos is smaller than 0 (which probably marks hitting the ground)
                zz = 0; // set it to zero
                        // multiply the accelerations by an amount:
                za *= -0.5;
                xa *= 0.6;
                ya *= 0.6;
            }
            za -= 0.15; // decrease z acceleration by 0.15

            // storage of x and y positions before move
            int ox = x;
            int oy = y;

            // integer conversion of the double x and y postions (which have already been updated):
            int nx = (int)xx;
            int ny = (int)yy;

            // the difference between the double->int new positions, and the inherited x and y positions:
            int expectedx = nx - x; // expected movement distance
            int expectedy = ny - y;

            /// THIS is where x and y are changed.
            Move(expectedx, expectedy); // move the ItemEntity.

            // finds the difference between the inherited before and after positions
            int gotx = x - ox;
            int goty = y - oy;
            // Basically, this accounts for any error in the whole double-to-int position conversion thing:
            xx += gotx - expectedx;
            yy += goty - expectedy;
        }

        public override bool IsSolid()
        {
            return false; // mobs cannot block this
        }

        public override void Render(Screen screen)
        {
            /* this first part is for the blinking effect */
            if (time >= lifeTime - 6 * 20)
            {
                if (time / 6 % 2 == 0) return;
            }

            item.sprite.Render(screen, x - 4, y - 4 - (int)(zz));
        }

        protected override void TouchedBy(Entity entity)
        {
            if (entity is not  Player player) return; // for the time being, we only care when a player touches an item.

            if (time > 30)
            { // conditional prevents this from being collected immediately.
                if (Game.IsConnectedClient() && player == Game.player)
                {// only register if the main player picks it up, on a client.
                    if (pickedUp && (JavaSystem.NanoTime() - pickupTimestamp) / 1E8 > 15L)
                    { // should be converted to tenths of a second.
                      // the item has already been picked up,
                      // but since more than 1.5 seconds has past, the item will be remarked as not picked up.
                        pickedUp = false;
                    }

                    if (!pickedUp)
                    {
                        Game.client.pickupItem(this);

                        pickedUp = true;
                        pickupTimestamp = JavaSystem.NanoTime();
                    }
                }
                else if (!pickedUp && !Game.ISONLINE)
                {// don't register if we are online and a player touches it; the client will register that.
                    pickedUp = true;

                    player.PickupItem(this);
                    
                    pickedUp = IsRemoved();
                }
            }
        }

        protected override List<string> GetDataPrints()
        {
            List<string> prints = base.GetDataPrints();

            prints.Insert(0, item.ToString());

            return prints;
        }
    }
}
