using MinicraftPlusSharp.SaveLoad;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MinicraftPlusSharp.Core
{
    public class VersionInfo
    {
        public readonly Version version;
	    public readonly string releaseUrl;
	    public readonly string releaseName;
	
	    public VersionInfo(JObject releaseInfo)
        {
            string versionTag = releaseInfo.Value<string>("tag_name").Substring(1); // cut off the "v" at the beginning

            version = new Version(versionTag);
            releaseUrl = releaseInfo.Value<string>("html_url");
            releaseName = releaseInfo.Value<string>("name");
        }

        public VersionInfo(Version version, string releaseUrl, string releaseName)
        {
            this.version = version;
            this.releaseUrl = releaseUrl;
            this.releaseName = releaseName;
        }
    }
}
