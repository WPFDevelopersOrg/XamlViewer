using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlViewer.Models
{
    public class XamlConfig
    {
        public string FontFamily { get; set; } = "Calibri";
        public double FontSize { get; set; } = 10d;

        public bool IsCodeCollapsing { get; set; } = true;
        public bool ShowLineNumber { get; set; } = true;
        public bool ShowRuler { get; set; }

        public bool AutoCompile { get; set; } = true;
        public double AutoCompileDelay { get; set; } = 2d;
    }
}
