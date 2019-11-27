using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlService;
using XamlService.Payloads;
using XamlViewer.Models;

namespace XamlViewer.Models
{
    public static class ResourcesMap
    {
        public static Dictionary<Location, string> LocationDic = new Dictionary<Location, string>
        {
            {Location.GlobalConfigFile,    AppDomain.CurrentDomain.BaseDirectory + "Config\\GlobalConfig.json" }
        };

        public static Dictionary<ProcessStatus, string> ProcessStatusDic = new Dictionary<ProcessStatus, string>
        {
            {ProcessStatus.Compile,       "Compiling..."},
            {ProcessStatus.Save,          "Saving..."},
            {ProcessStatus.LoadFonts,     "Loading system fonts..."},
        };
    }
}
