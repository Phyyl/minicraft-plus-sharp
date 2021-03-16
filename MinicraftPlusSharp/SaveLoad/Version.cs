using MinicraftPlusSharp.Java;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.SaveLoad
{
    public class Version : IComparable<Version>
    {
        private int make, major, minor, dev;

        private bool valid = true;

        public Version(string version)
            : this(version, true)
        {
        }

        private Version(string version, bool printError)
        {
            string[] nums = version.Split("\\.");

            try
            {
                make = nums.Length > 0 ? int.Parse(nums[0]) : 0;
                major = nums.Length > 1 ? int.Parse(nums[1]) : 0;

                string min = nums.Length > 2 ? nums[2] : "";

                if (min.Contains("-"))
                {
                    string[] mindev = min.Split("-");
                    minor = int.Parse(mindev[0]);
                    dev = int.Parse(mindev[1].Replace("pre", "").Replace("dev", ""));
                }
                else
                {
                    if (!min.Equals("")) minor = int.Parse(min);
                    else minor = 0;
                    dev = 0;
                }
            }
            catch (FormatException ex)
            {
                if (printError)
                {
                    Console.Error.WriteLine("INVALID version number: \"" + version + "\"");
                }

                valid = false;
            }
            catch (Exception ex)
            {
                if (printError)
                {
                    ex.PrintStackTrace();
                }

                valid = false;
            }
        }

        public bool IsValid()
        {
            return valid;
        }

        public static bool IsValid(string version)
        {
            return new Version(version, false).IsValid();
        }

        // the returned value of this method (-1, 0, or 1) is determined by whether this object is less than, equal to, or greater than the specified object.
        public int CompareTo(Version ov)
        {
            if (make != ov.make)
            {
                return make.CompareTo(ov.make);
            }

            if (major != ov.major)
            {
                return major.CompareTo(ov.major);
            }

            if (minor != ov.minor)
            {
                return minor.CompareTo(ov.minor);
            }

            if (dev != ov.dev)
            {
                if (dev == 0)
                {
                    return 1; //0 is the last "dev" version, as it is not a dev.
                }

                return ov.dev == 0 ? -1 : dev.CompareTo(ov.dev);
            }

            return 0; // the versions are equal.
        }

        public override string ToString()
        {
            return make + "." + major + "." + minor + (dev == 0 ? "" : "-dev" + dev);
        }
    }

}
