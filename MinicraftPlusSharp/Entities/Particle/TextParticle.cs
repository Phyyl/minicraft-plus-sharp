using MinicraftPlusSharp.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities.Particle
{
    public class TextParticle : Particle
    {
        private string msg; // Message of the text particle
        private double xa, ya, za; // x,y,z acceleration
        private double xx, yy, zz; // x,y,z coordinates

        private FontStyle style;

        /**
         * Creates a text particle which shows a message on the screen.
         * 
         * @param msg Message to display
         * @param x X map position
         * @param y Y map position
         * @param col Text color
         */
        public TextParticle(string msg, int x, int y, int col)
            : base(x, y, msg.Length, 60, null)
        {
            style = new FontStyle(col).SetShadowType(Color.BLACK, false);

            this.msg = msg;

            xx = x; //assigns x pos
            yy = y; //assigns y pos
            zz = 2; //assigns z pos to be 2

            //assigns x,y,z acceleration:
            xa = random.NextGaussian() * 0.3;
            ya = random.NextGaussian() * 0.2;
            za = random.NextFloat() * 0.7 + 2;
        }

        public override void Tick()
        {
            base.Tick();

            //move the particle according to the acceleration
            xx += xa;
            yy += ya;
            zz += za;

            if (zz < 0)
            {
                //if z pos if less than 0, alter accelerations...
                zz = 0;
                za *= -0.5;
                xa *= 0.6;
                ya *= 0.6;
            }

            za -= 0.15;  // za decreases by 0.15 every tick.
                         //truncate x and y coordinates to integers:
            x = (int)xx;
            y = (int)yy;
        }

        public override void Render(Screen screen)
        {
            style.SetXPos(x - msg.Length * 4).SetYPos(y - (int)zz).Draw(msg, screen);
        }

        /**
         * Returns the message and color divied by the character :.
         * @return string representation of the particle
         */
        public string GetData()
        {
            return msg + ":" + style.getColor();
        }
    }

}
