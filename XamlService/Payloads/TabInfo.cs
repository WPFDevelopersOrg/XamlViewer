
namespace XamlService.Payloads
{
    public class TabFlag
    {
        public string Guid { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class TabInfo : TabFlag
    {
        public string FileContent { get; set; }
    }
}
