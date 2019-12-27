using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlViewer.Models
{
    public enum Location
    {
        GlobalConfigFile
    }

    public enum TabStatus
    {
        None = 0,
        NoSave = 1,
        Locked = 2,
        Inner = 6, //contains Locked
    }

    public enum MessageType
    { 
        None, 
        Error, 
        Warning,
        Information, 
        Question
    }

    public enum MessageButton
    {
        OK, 
        OKCancel,
        YesNoCancel,
        YesNo
    }
}
