using System;
using System.Windows.Media;
using XamlUtil.Common;

namespace XamlTheme.Utils
{
    public static class ColorUtil
    { 
        public static Color InvertColor(Color color)
        {
            return Color.FromRgb((byte)~color.R, (byte)~color.G, (byte)~color.B);
        } 

        public static void HsbFromColor(Color C, ref double H, ref double S, ref double B)
        {
            double r = C.R / 255d;
            double g = C.G / 255d;
            double b = C.B / 255d;

            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            var delta = max - min;

            var hue = 0d;
            var saturation = DoubleUtil.GreaterThan(max, 0) ? (delta / max) : 0.0;
            var brightness = max;

            if (!DoubleUtil.IsZero(delta))
            {
                if (DoubleUtil.AreClose(r, max))
                    hue = (g - b) / delta;
                else if (DoubleUtil.AreClose(g, max))
                    hue = 2 + (b - r) / delta;
                else if (DoubleUtil.AreClose(b, max))
                    hue = 4 + (r - g) / delta;

                hue = hue * 60;
                if (DoubleUtil.LessThan(hue, 0d))
                    hue += 360;
            }

            H = hue / 360d;
            S = saturation;
            B = brightness;
        }

        public static Color ColorFromAhsb(double A, double H, double S, double B)
        {
            var r = ColorFromHsb(H, S, B);
            r.A = (byte)Math.Round(A * 255d);

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="H">0---1</param>
        /// <param name="S">0---1</param>
        /// <param name="B">0---1</param>
        /// <returns></returns>  
        public static Color ColorFromHsb(double H, double S, double B)
        {
            double red = 0.0, green = 0.0, blue = 0.0;

            if (DoubleUtil.IsZero(S))
                red = green = blue = B;
            else
            {
                var h = DoubleUtil.IsOne(H) ? 0d : (H * 6.0);
                int i = (int)Math.Floor(h);

                var f = h - i;
                var r = B * (1.0 - S);
                var s = B * (1.0 - S * f);
                var t = B * (1.0 - S * (1.0 - f));

                switch (i)
                {
                    case 0: red = B; green = t; blue = r; break;
                    case 1: red = s; green = B; blue = r; break;
                    case 2: red = r; green = B; blue = t; break;
                    case 3: red = r; green = s; blue = B; break;
                    case 4: red = t; green = r; blue = B; break;
                    case 5: red = B; green = r; blue = s; break;
                }
            }

            return Color.FromRgb((byte)Math.Round(red * 255.0), (byte)Math.Round(green * 255.0), (byte)Math.Round(blue * 255.0));
        }
    }
}
