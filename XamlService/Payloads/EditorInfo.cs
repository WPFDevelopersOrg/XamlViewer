
namespace XamlService.Payloads
{
    public class EditorInfo
    {
        public string Guid { get; set; }
        public string Text { get; set; }
        public bool IsModified { get; set; }
        public bool CanRedo { get; set; }
        public bool CanUndo { get; set; }
    }
}
