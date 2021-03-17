using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Java
{
    public static class JavaExtensions
    {
        public static void PrintStackTrace(this Exception ex)
        {
            Console.Error.WriteLine(ex.StackTrace);
        }

        public static string ReplaceFirst(this string str, string from, string to)
        {
            int pos = str.IndexOf(from);

            if (pos < 0)
            {
                return str;
            }

            return str[0..pos] + to + str[(pos+from.Length)..];
        }
    }
}
