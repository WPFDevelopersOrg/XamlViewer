
namespace XamlService.Payloads
{
    public enum ProcessStatus
    {
        Compile,
        FinishCompile,

        Save,
        FinishSave,

        LoadFonts,
        FinishLoadFonts,
    }

    public class ProcessInfo
    {
        public ProcessStatus status { get; set; }
        public string Guid { get; set; }
    }
}
