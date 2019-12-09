using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace XamlViewer.Utils
{
    public class CursorUtil
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.RegisterAttached("Source", typeof(string), typeof(CursorUtil), new PropertyMetadata(OnSourcePropertyChanged));
        public static string GetSource(DependencyObject obj)
        {
            return (string)obj.GetValue(SourceProperty);
        }
        public static void SetSource(DependencyObject obj, string value)
        {
            obj.SetValue(SourceProperty, value);
        }
        static void OnSourcePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = target as FrameworkElement;
            var uri = (string)e.NewValue;

            if (ctrl == null || string.IsNullOrWhiteSpace(uri))
                return;

            var streamInfo = Application.GetResourceStream(new Uri(uri, UriKind.Relative));

            if (streamInfo != null)
                ctrl.Cursor = new Cursor(streamInfo.Stream);
        }
    }
}
