using System.Collections.Generic;
using XamlService.Payloads;

namespace XamlViewer.Models
{
    public class XamlConfig : EditorConfig
    {
        public XamlConfig()
        {
            FontFamily = "Calibri";
            FontSize = 13d;

            WordWrap = false;
            ShowLineNumber = true;

            AutoCompile = true;
            AutoCompileDelay = 2d;

            Files = new List<string>();
            References = new List<string>();
        }

        public List<string> Files { get; set; } 
        public List<string> References { get; set; }
    }
}
