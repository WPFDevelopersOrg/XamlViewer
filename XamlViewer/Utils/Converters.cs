using System;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using XamlUtil.Common;

namespace XamlViewer.Utils
{ 
    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new GridLength((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 
}
