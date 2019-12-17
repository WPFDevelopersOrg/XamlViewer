using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlService.Payloads
{
    public class TabFlag
    {
        public bool IsReadOnly { get; set; }
    }
    
    public class TabInfo : TabFlag
    {
        public string FileName { get; set; }
        public string FileContent { get; set; }
    }
}
