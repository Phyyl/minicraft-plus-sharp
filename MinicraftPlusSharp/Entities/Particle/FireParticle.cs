using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Particle
{
    public class FireParticle : Particle
    {
        /// This is used for Spawners, when they spawn an entity.

        /**
		 * Creates a new particle at the given position. It has a lifetime of 30 ticks
		 * and a fire looking sprite.
		 * 
		 * @param x X map position
		 * @param y Y map position
		 */
        public FireParticle(int x, int y)
            : base(x, y, 30, new Sprite(4, 4, 3))
        {
        }
    }
}
