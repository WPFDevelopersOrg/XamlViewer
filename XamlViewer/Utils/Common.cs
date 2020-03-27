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

        public static ValueWithGuid<EditorSetting> GetCurrentSettings(XamlConfig config, string guid = null)
        {
            return new ValueWithGuid<EditorSetting>
            {
                Guid = guid,
                Value = new EditorSetting
                {
                    FontFamily = config.FontFamily,
                    FontSize = config.FontSize,

                    WordWrap = config.WordWrap,
                    ShowLineNumber = config.ShowLineNumber,
                    CodeCompletion = config.CodeCompletion,

                    AutoCompile = config.AutoCompile,
                    AutoCompileDelay = config.AutoCompileDelay
                }
            };
        }
    }
}
