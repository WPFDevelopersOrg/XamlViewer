using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class SettingChangedEvents : PubSubEvent<EditorSetting> {}
    public class SearchFilterChangedEvents : PubSubEvent<SearchFilter> {}
}
