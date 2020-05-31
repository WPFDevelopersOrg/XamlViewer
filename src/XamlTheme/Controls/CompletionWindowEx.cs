using System;
using System.Windows;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace XamlTheme.Controls
{
    public class CompletionWindowEx : CompletionWindow
    {
        public CompletionWindowEx(TextArea textArea)
            : base(textArea)
        {
        } 

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (SizeToContent != SizeToContent.Manual && WindowState == WindowState.Normal)
                InvalidateMeasure();
        }
    }
}
