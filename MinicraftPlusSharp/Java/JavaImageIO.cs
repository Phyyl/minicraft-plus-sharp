using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MinicraftPlusSharp.Java
{
    public static class JavaImageIO
    {
        public static BitmapImage Read(string filename)
        {
            return new BitmapImage(new Uri(filename));
        }
    }
}
