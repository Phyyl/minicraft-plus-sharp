using MinicraftPlusSharp.Entities.Mobs;
using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities
{
    public class Arrow : Entity, ClientTickable
    {
        private Direction dir;
        private int damage;
        public Mob owner;
        private int speed;

        public Arrow(Mob owner, Direction dir, int dmg)
            : this(owner, owner.x, owner.y, dir, dmg)
        {
        }

        public Arrow(Mob owner, int x, int y, Direction dir, int dmg)
            : base(Math.Abs(dir.GetX()) + 1, Math.Abs(dir.GetY()) + 1)
        {
            this.owner = owner;
            this.x = x;
            this.y = y;
            this.dir = dir;

            damage = dmg;
            col = Color.Get(-1, 111, 222, 430);

            if (damage > 3)
            {
                speed = 8;
            }
            else
            {
                speed = damage >= 0 ? 7 : 6;
            }
        }

        /**
         * Generates information about the arrow.
         * @return string representation of owner, xdir, ydir and damage.
         */
        public string GetData()
        {
            return owner.eid + ":" + dir.Ordinal + ":" + damage;
        }

        public override void Tick()
        {
            if (x < 0 || x >> 4 > level.w || y < 0 || y >> 4 > level.h)
            {
                Remove(); // Remove when out of bounds
                return;
            }

            x += dir.GetX() * speed;
            y += dir.GetY() * speed;

            // TODO I think I can just use the xr yr vars, and the normal system with touchedBy(entity) to detect collisions instead.

            List<Entity> entitylist = level.GetEntitiesInRect(new Rectangle(x, y, 0, 0, Rectangle.CENTER_DIMS));
            bool criticalHit = random.NextInt(11) < 9;
            foreach (Entity hit in entitylist)
            {
                if (hit is Mob mob && hit != owner)
                {
                    int extradamage = (hit is Player ? 0 : 3) + (criticalHit ? 0 : 1);
                    mob.Hurt(owner, damage + extradamage, dir);
                }

                if (!level.GetTile(x / 16, y / 16).MayPass(level, x / 16, y / 16, this)
                        && !level.GetTile(x / 16, y / 16).connectsToFluid
                        && level.GetTile(x / 16, y / 16).id != 16)
                {
                    this.Remove();
                }
            }
        }

        public override bool IsSolid()
        {
            return false;
        }

        public override void Render(Screen screen)
        {
            int xt = 0;
            int yt = 2;

            if (dir == Direction.LEFT) xt = 1;
            if (dir == Direction.UP) xt = 2;
            if (dir == Direction.DOWN) xt = 3;

            screen.Render(x - 4, y - 4, xt + yt * 32, 0);
        }
    }
}
