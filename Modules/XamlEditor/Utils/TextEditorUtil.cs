using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.AvalonEdit;

namespace XamlEditor.Utils
{
    public class TextEditorUtil
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(TextEditorUtil), new PropertyMetadata(OnTextPropertyChanged));
        public static string GetText(DependencyObject obj)
        {
            return (string)obj.GetValue(TextProperty);
        }
        public static void SetText(DependencyObject obj, string value)
        {
            obj.SetValue(TextProperty, value);
        }
        static void OnTextPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = target as TextEditor;
            var text = (string)e.NewValue;

            ctrl.Text = text;
        } 
    }
}
