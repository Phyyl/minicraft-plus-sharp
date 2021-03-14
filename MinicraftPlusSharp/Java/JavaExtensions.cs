using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Java
{
    public static class JavaExtensions
    {
        public static void PrintStackTrace(this Exception ex)
        {
            Console.Error.WriteLine(ex.StackTrace);
        }
    }
}
