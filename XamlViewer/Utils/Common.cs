using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace XamlViewer.Utils
{
    public static class Common
    {
        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            if (!(obj is Visual) && !(obj is Visual3D))
                return null;

            while (obj != null)
            {
                var o = obj as T;
                if (o != null)
                    return o;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        public static T FindLogicParent<T>(DependencyObject obj) where T : DependencyObject
        {
            if (!(obj is Visual) && !(obj is Visual3D))
                return null;

            while (obj != null)
            {
                var o = obj as T;
                if (o != null)
                    return o;

                obj = LogicalTreeHelper.GetParent(obj);
            }

            return null;
        }

        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
                return null;

            var childCount = VisualTreeHelper.GetChildrenCount(obj);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);

                var tChild = child as T;
                if (tChild != null)
                    return tChild;

                child = FindVisualChild<T>(child);
                if (child != null)
                    return (T)child;
            }

            return null;
        }
    }
}
