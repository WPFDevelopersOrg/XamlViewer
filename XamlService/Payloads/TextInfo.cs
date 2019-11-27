using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlService.Payloads
{
    public class EditorInfo
    {
        public string Text { get; set; }
        public bool CanRedo { get; set; }
        public bool CanUndo { get; set; }
    }
}
