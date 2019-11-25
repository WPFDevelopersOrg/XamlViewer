namespace XamlViewer.Models
{
    public class XamlConfig
    {
        public XamlConfig()
        {
            FontFamily = "Calibri";
            FontSize = 10d;

            IsCodeCollapsing = true;
            ShowLineNumber = true;

            AutoCompile = true;
            AutoCompileDelay = 2d;
        }

        public string FontFamily { get; set; }
        public double FontSize { get; set; }

        public bool IsCodeCollapsing { get; set; }
        public bool ShowLineNumber { get; set; }
        public bool ShowRuler { get; set; }

        public bool AutoCompile { get; set; }
        public double AutoCompileDelay { get; set; }
    }
}
