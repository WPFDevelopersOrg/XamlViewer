using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class CloseTabEvent : PubSubEvent<string> { }
    public class UpdateTabStatusEvent : PubSubEvent<TabFlag> { }
    public class InitWorkAreaEvent : PubSubEvent { }
    public class SelectTabEvent : PubSubEvent<TabSelectInfo> { }
    public class OpenFilesEvent : PubSubEvent<string[]> { }
}
