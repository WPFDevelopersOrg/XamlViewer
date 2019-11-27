using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class TextChangedEvent : PubSubEvent<EditorInfo>
    {
    }  

    public class SaveTextEvent : PubSubEvent<string>
    {}

    public class ReloadTextEvent : PubSubEvent<string>
    { }
}
