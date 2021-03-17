using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Java
{
    public abstract class JavaThread
    {
        private readonly Thread thread;

        protected JavaThread(string name = default)
        {
            thread = new Thread(ThreadMain)
            {
                Name = name ?? "JavaThread",
                IsBackground = true
            };
        }

        protected abstract void ThreadMain();

        public static void DumpStack()
        {
            Console.WriteLine(new StackTrace().ToString());
        }
    }
}
