using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlService.Payloads
{
    public class EditorConfig
    {
        public string FontFamily { get; set; }
        public double FontSize { get; set; }

        public bool WordWrap { get; set; }
        public bool ShowLineNumber { get; set; }

        public bool AutoCompile { get; set; }
        public double AutoCompileDelay { get; set; }
    }
}
