using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class ProcessStatusEvent : PubSubEvent<ProcessInfo> { }
}
