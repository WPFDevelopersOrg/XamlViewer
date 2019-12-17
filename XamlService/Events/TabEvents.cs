using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class CloseTabEvent : PubSubEvent<string> {}
    
    public class UpdateTabStatusEvent : PubSubEvent<TabFlag> {}

}
