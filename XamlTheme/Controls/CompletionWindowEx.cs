using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;
using XamlTheme.Utils;

namespace XamlTheme.Controls
{
    public class CompletionWindowEx : CompletionWindow
    {
        public CompletionWindowEx(TextArea textArea)
            : base(textArea)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            WindowUtil.HandleSizeToContent(this, () => UpdatePosition());
        }
    }
}
