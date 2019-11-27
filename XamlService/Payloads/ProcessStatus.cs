using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlService.Payloads
{
    public enum ProcessStatus
    {
        Compile,
        FinishCompile,

        Save,
        FinishSave,

        LoadFonts,
        FinishLoadFonts,
    }
}
