
namespace XamlService.Payloads
{
    public class ValueWithGuid<T>
    {
        public string Guid { get; set; }
        public T Value { get; set; }
    }

    public class EditorSetting
    { 
        public string FontFamily { get; set; }
        public double FontSize { get; set; }

        public bool WordWrap { get; set; }
        public bool ShowLineNumber { get; set; }

        public bool AutoCompile { get; set; }
        public double AutoCompileDelay { get; set; } 
    }
}
