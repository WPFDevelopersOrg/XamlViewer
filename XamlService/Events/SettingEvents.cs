using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class RequestSettingEvent : PubSubEvent<string> { }
    public class SettingChangedEvent : PubSubEvent<ValueWithGuid<EditorSetting>> {}
}
