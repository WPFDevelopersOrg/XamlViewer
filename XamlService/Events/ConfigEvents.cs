using Prism.Events;
using XamlService.Payloads;

namespace XamlService.Events
{
    public class ConfigEvents : PubSubEvent<EditorConfig> {}
    public class SearchConfigEvents : PubSubEvent<SearchConfig> {}
}
