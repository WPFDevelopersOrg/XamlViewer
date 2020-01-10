
namespace XamlService.Payloads
{
    public class SearchFilter
    {
        public bool IsMatchCase { get; set; }
        public bool IsWholeWords { get; set; }
        public bool UseRegex { get; set; }
    }

    public class EditorSetting : SearchFilter
    { 
        public string FontFamily { get; set; }
        public double FontSize { get; set; }

        public bool WordWrap { get; set; }
        public bool ShowLineNumber { get; set; }

        public bool AutoCompile { get; set; }
        public double AutoCompileDelay { get; set; } 
    }
}
