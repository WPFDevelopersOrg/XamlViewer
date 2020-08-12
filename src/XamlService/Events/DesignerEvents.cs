using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class RefreshDesignerEvent : PubSubEvent<TabInfo> { }
    public class SyncDataSourceEvent : PubSubEvent<string> { }
}
