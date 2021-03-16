using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Entities
{
    public interface ClientTickable : Tickable
    {
        void ClientTick()
        {
            Tick();
        }
    }
}
