using System.Windows;
using System.Windows.Controls;

namespace XamlTheme.Utils
{
    public class GridSplitterUtil
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.RegisterAttached("Orientation", typeof(Orientation), typeof(GridSplitterUtil), new PropertyMetadata(Orientation.Horizontal));
        public static Orientation GetOrientation(DependencyObject obj)
        {
            return (Orientation)obj.GetValue(OrientationProperty);
        }
        public static void SetOrientation(DependencyObject obj, Orientation value)
        {
            obj.SetValue(OrientationProperty, value);
        }
    }
}
