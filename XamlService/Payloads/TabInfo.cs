
namespace XamlService.Payloads
{
    public class TabFlag
    {
        public bool IsReadOnly { get; set; }
    }

    public class TabInfo : TabFlag
    {
        public string Guid { get; set; }
        public bool AlreadySelectPath { get; set; } 
        public string FileContent { get; set; }
    }
}
