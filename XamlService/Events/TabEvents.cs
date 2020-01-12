using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class CloseTabEvent : PubSubEvent<string> { }
    public class UpdateTabStatusEvent : PubSubEvent<TabFlag> { }
    public class InitWorkAreaEvent : PubSubEvent { }
    public class CompileTabEvent : PubSubEvent<string> { }
    public class ActiveTabEvent : PubSubEvent<TabActiveInfo> { }
}
