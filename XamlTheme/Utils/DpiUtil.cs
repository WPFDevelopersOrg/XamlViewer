using System;
using System.Windows;
using System.Windows.Media;
using XamlTheme.Controls;
using XamlUtil.Common;

namespace XamlTheme.Utils
{
    /// <summary>
    /// DIP (Device Independent Pixel)
    /// DIU (Device Independent Unit)
    /// </summary>
    public static class DpiUtil
    {
        private const double DIP = 96.0; 
        private const double DIU = 1 / 96.0;

        public static Point GetDpiFactor(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");

            var source = PresentationSource.FromVisual(visual);
            var matrix = source.CompositionTarget.TransformToDevice;
            return new Point(matrix.M11, matrix.M22);
        }

        public static double PtToPixel(double pt)
        {
            return (pt * 1 / 72.0 * DIP);
        }

        public static Point GetDpi(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");

            Point sysDpiFactor = GetDpiFactor(visual);
            return new Point(
                 sysDpiFactor.X * DIP,
                 sysDpiFactor.Y * DIP);
        }

        public static double GetDevicePixelUnit(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");
                
            return DIU * GetDpi(visual).X;
        }

        public static double GetPixelPerUnit(RulerUnit unit)
        {
            double result = 1;
            switch (unit)
            {
                case RulerUnit.Pixel:
                    result = 1;
                    break;
                case RulerUnit.Inch:
                    result = 96;
                    break;
                case RulerUnit.Foot:
                    result = 1152;
                    break;
                case RulerUnit.Millimeter:
                    result = 3.7795;
                    break;
                case RulerUnit.Centimeter:
                    result = 37.795;
                    break;
            }

            return result;
        }
        
        /// <summary>
        /// Calculates the value to be used for layout rounding at high DPI.
        /// </summary>
        /// <param name="value">Input value to be rounded.</param>
        /// <param name="dpiScale">Ratio of screen's DPI to layout DPI</param>
        /// <returns>Adjusted value that will produce layout rounding on screen at high dpi.</returns>
        /// <remarks>This is a layout helper method. It takes DPI into account and also does not return
        /// the rounded value if it is unacceptable for layout, e.g. Infinity or NaN. It's a helper associated with
        /// UseLayoutRounding  property and should not be used as a general rounding utility.</remarks>
        public static double RoundLayoutValue(double value, double dpiScale)
        {
            double newValue;

            // If DPI == 1, don't use DPI-aware rounding. 
            if (!DoubleUtil.AreClose(dpiScale, 1.0))
            {
                newValue = Math.Round(value * dpiScale) / dpiScale;
                // If rounding produces a value unacceptable to layout (NaN, Infinity or MaxValue), use the original value.
                if (DoubleUtil.IsNaN(newValue) ||
                    Double.IsInfinity(newValue) ||
                    DoubleUtil.AreClose(newValue, Double.MaxValue))
                {
                    newValue = value;
                }
            }
            else
                newValue = Math.Round(value);

            return newValue;
        } 
    }
}
