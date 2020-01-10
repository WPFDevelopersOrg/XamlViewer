using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using XamlService.Payloads;
using XamlViewer.Models;
using SWF = System.Windows.Forms;

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

        public static BitmapSource ToBitmapSource(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return null;

            return new BitmapImage(new Uri(uri, UriKind.RelativeOrAbsolute));
        }

        public static string ShowSaveFileDialog(string fileName)
        {
            var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml", FileName = Path.GetFileNameWithoutExtension(fileName) };
            if (sfd.ShowDialog() == SWF.DialogResult.OK)
                return sfd.FileName;

            return null;
        }

        public static EditorSetting GetCurrentSettings(XamlConfig config)
        {
            return new EditorSetting
            {
                FontFamily = config.FontFamily,
                FontSize = config.FontSize,

                WordWrap = config.WordWrap,
                ShowLineNumber = config.ShowLineNumber,

                AutoCompile = config.AutoCompile,
                AutoCompileDelay = config.AutoCompileDelay,

                IsMatchCase = config.IsMatchCase,
                IsWholeWords = config.IsWholeWords,
                UseRegex = config.UseRegex
            };
        }
    }
}
