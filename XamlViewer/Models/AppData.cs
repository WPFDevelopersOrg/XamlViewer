using System;
using System.Threading.Tasks;

namespace XamlViewer.Models
{
    public class AppData
    {
        public XamlConfig Config { get; set; }
        public Func<Task> CollectExistedFileAction { get; set; }
    }
}
