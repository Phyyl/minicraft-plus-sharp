using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Java
{
    public abstract class JavaThread
    {
        private readonly Thread thread;

        protected JavaThread(string name)
        {
            thread = new Thread(ThreadMain)
            {
                Name = name,
                IsBackground = true
            };
        }

        protected abstract void ThreadMain();
    }
}
