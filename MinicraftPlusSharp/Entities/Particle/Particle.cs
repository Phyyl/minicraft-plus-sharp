using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Particle
{
    public class Particle : Entity, ClientTickable
    {
        private int time; // lifetime elapsed.
        private int lifetime;

        protected Sprite sprite;

        /**
         * Creates an particle entity at the given position. The particle has a x and y radius = 1.
         * @param x X map coordinate
         * @param y Y map coorindate
         * @param xr x radius of the particle   
         * @param lifetime How many game ticks the particle lives before its removed
         * @param sprite The particle's sprite
         */
        public Particle(int x, int y, int xr, int lifetime, Sprite sprite)
            : base(xr, 1) // make a particle at the given coordinates
        {
            this.x = x;
            this.y = y;
            this.lifetime = lifetime;
            this.sprite = sprite;
            time = 0;
        }

        public Particle(int x, int y, int lifetime, Sprite sprite)
            : this(x, y, 1, lifetime, sprite)
        {
        }

        public override void Tick()
        {
            time++;

            if (time > lifetime)
            {
                Remove();
            }
        }

        public override void Render(Screen screen)
        {
            sprite.Render(screen, x, y);
        }

        public override bool IsSolid()
        {
            return false;
        }
    }
}
