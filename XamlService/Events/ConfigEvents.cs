using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class ConfigEvents : PubSubEvent<EditorConfig> {}
}
