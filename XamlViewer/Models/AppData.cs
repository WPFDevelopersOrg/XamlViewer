using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlViewer.Models
{
    public class AppData
    {
        public XamlConfig Config { get; set; }
        public Action CollectExistedFileAction { get; set; }
    }
}
