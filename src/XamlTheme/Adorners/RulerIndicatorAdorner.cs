using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XamlTheme.Utils;
using XamlUtil.Common;

namespace XamlTheme.Adorners
{
    public class RulerIndicatorAdorner : Adorner
    {
        private double _length = 0;
        private Pen _pen = null;

        public RulerIndicatorAdorner(UIElement adornedElement, double length)
            : base(adornedElement)
        {
            _length = length;

            _pen = new Pen(Brushes.Red, 1);
            _pen.Freeze();

            IsHitTestVisible = false;
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement == null || _pen == null || DoubleUtil.IsZero(_length))
                return;

            var screenPos = new Win32.POINT();
            if (Win32.GetCursorPos(ref screenPos))
            {
                var pos = AdornedElement.PointFromScreen(new Point(screenPos.X, screenPos.Y));

                drawingContext.DrawLine(_pen, new Point(pos.X, 0), new Point(pos.X, _length));
                drawingContext.DrawLine(_pen, new Point(0, pos.Y), new Point(_length, pos.Y));
            }
        }
    }
}

