using System;
using System.Collections.Generic;
using XamlService.Payloads;

namespace XamlViewer.Models
{
    public static class ResourcesMap
    {
        public static Dictionary<Location, string> LocationDic = new Dictionary<Location, string>
        {
            [Location.GlobalConfigFile] = AppDomain.CurrentDomain.BaseDirectory + "Config\\GlobalConfig.json",
            [Location.ModulePath] = AppDomain.CurrentDomain.BaseDirectory + "Modules",
        };

        public static Dictionary<ProcessStatus, string> ProcessStatusDic = new Dictionary<ProcessStatus, string>
        {
            [ProcessStatus.Compile] = "Compiling...",
            [ProcessStatus.Save] = "Saving...",
            [ProcessStatus.LoadFonts] = "Loading system fonts...",
        };

        public static Dictionary<int, string> ExampleFileNameDic = new Dictionary<int, string>
        {
            [0] = InternalConstStrings.StyleExampleFileName,
            [1] = InternalConstStrings.WindowExampleFileName,
            [2] = InternalConstStrings.AnimationExampleFileName,
            [3] = InternalConstStrings.DataSourceExampleFileName,
            [4] = InternalConstStrings.CustomControlExampleFileName,
        };

        public static Dictionary<string, string> NameToContentKeyDic = new Dictionary<string, string>
        {
            [InternalConstStrings.HelpFileName] = InternalConstStrings.HelpFileContentKey,
            [InternalConstStrings.StyleExampleFileName] = InternalConstStrings.StyleExampleFileContentKey,
            [InternalConstStrings.WindowExampleFileName] = InternalConstStrings.WindowExampleFileContentKey,
            [InternalConstStrings.AnimationExampleFileName] = InternalConstStrings.AnimationExampleFileContentKey,
            [InternalConstStrings.DataSourceExampleFileName] = InternalConstStrings.DataSourceExampleFileContentKey,
            [InternalConstStrings.CustomControlExampleFileName] = InternalConstStrings.CustomControlExampleFileContentKey,
        };
    }
}
