using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class TextChangedEvent : PubSubEvent<EditorInfo> {}  

    public class SaveTextEvent : PubSubEvent<TabInfo> {}

    public class RequestTextEvent : PubSubEvent<TabInfo> {}

    public class LoadTextEvent : PubSubEvent<TabInfo> {}

    public class CaretPositionEvent : PubSubEvent<CaretPosition> { }

    public class InitComplatedEvent : PubSubEvent<string> { }
}
