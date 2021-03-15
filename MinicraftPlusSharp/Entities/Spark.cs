using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities
{
    public class Spark : Entity
    {
        private int lifeTime; // how much time until the spark disappears
        private double xa, ya; // the x and y acceleration
        private double xx, yy; // the x and y positions
        private int time; // the amount of time that has passed
        private AirWizard owner; // the AirWizard that created this spark

        /**
         * Creates a new spark. Owner is the AirWizard which is spawning this spark.
         * @param owner The AirWizard spawning the spark.
         * @param xa X velocity.
         * @param ya Y velocity.
         */
        public Spark(AirWizard owner, double xa, double ya)
            : base(0, 0)
        {

            this.owner = owner;
            xx = owner.x;
            yy = owner.y;
            this.xa = xa;
            this.ya = ya;

            // Max time = 389 ticks. Min time = 360 ticks.
            lifeTime = 60 * 6 + random.NextInt(30);
        }

        public override void Tick()
        {
            time++;
            if (time >= lifeTime)
            {
                Remove(); // remove this from the world
                return;
            }
            // move the spark:
            xx += xa;
            yy += ya;
            x = (int)xx;
            y = (int)yy;

            // if the entity is a mob, but not a Air Wizard, then hurt the mob with 1 damage.
            List<Entity> toHit = level.GetEntitiesInRect(entity => entity is Mob && entity is not AirWizard, new Rectangle(x, y, 0, 0, Rectangle.CENTER_DIMS)); // gets the entities in the current position to hit.
            toHit.ForEach(entity => (entity as Mob)?.Hurt(owner, 1));
        }

        /** Can this entity block you? Nope. */
        public override bool IsSolid()
        {
            return false;
        }

        public override void Render(Screen screen)
        {
            /* this first part is for the blinking effect */
            if (time >= lifeTime - 6 * 20)
            {
                if (time / 6 % 2 == 0) return; // if time is divisible by 12, then skip the rest of the code.
            }

            int randmirror = random.NextInt(4);

            screen.Render(x - 4, y - 4 - 2, 8 + 24 * 32, randmirror, 2); // renders the spark
        }

        /**
         * Returns the owners id as a string.
         * @return the owners id as a string.
         */
        public string GetData()
        {
            return owner.eid + "";
        }
    }

}
