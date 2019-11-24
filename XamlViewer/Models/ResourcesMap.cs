using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlViewer.Models;

namespace XamlViewer.Models
{
    public static class ResourcesMap
    {
        public static Dictionary<Location, string> LocationDic = new Dictionary<Location, string>
        {
            {Location.GlobalConfigFile,    AppDomain.CurrentDomain.BaseDirectory + "Config\\GlobalConfig.json" }
        };
    }
}
