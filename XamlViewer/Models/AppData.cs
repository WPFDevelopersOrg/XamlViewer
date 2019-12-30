using System;

namespace XamlViewer.Models
{
    public class AppData
    {
        public XamlConfig Config { get; set; }
        public Action CollectExistedFileAction { get; set; }
    }
}
