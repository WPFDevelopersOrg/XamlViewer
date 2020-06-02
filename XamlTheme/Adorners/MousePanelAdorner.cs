using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using XamlTheme.Utils;

namespace XamlTheme.Adorners
{
    public class MousePanelAdorner : Adorner
    {
        private Point _posToMouse;
        private Point _posToPanel;
        private Size _adornerElementSize;
        private ImageBrush _imageBrush = null;
        private bool _disabledYPosition = false;

        public MousePanelAdorner(UIElement adornedElement, FrameworkElement adornerElement, Point posToMouse, Point posToPanel = new Point(), bool disabledYPosition = false)
            : base(adornedElement)
        {
            IsHitTestVisible = false;

            _posToMouse = posToMouse;
            _posToPanel = posToPanel;
            _disabledYPosition = disabledYPosition;
            _adornerElementSize = new Size(adornerElement.ActualWidth, adornerElement.ActualHeight);
            _imageBrush = ConstructImageBrush(adornerElement);
        }

        public void Update()
        {
            InvalidateVisual();
        }

        public RenderTargetBitmap RenderVisualToBitmap(Visual vsual, int width, int height)
        {
            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(new VisualBrush(vsual), null, new Rect(0, 0, width, height));
                context.Close();
            }

            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
            rtb.Render(drawingVisual);

            return rtb;
        }

        private ImageBrush ConstructImageBrush(FrameworkElement frameworkElement)
        {
            return new ImageBrush(RenderVisualToBitmap(frameworkElement, (int)frameworkElement.ActualWidth, (int)frameworkElement.ActualHeight));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement != null)
            {
                var screenPos = new Win32.POINT();
                if (Win32.GetCursorPos(ref screenPos))
                {
                    var pos = AdornedElement.PointFromScreen(new Point(screenPos.X, screenPos.Y));
                    var rect = new Rect(new Point(pos.X - _posToMouse.X, _disabledYPosition ? _posToPanel.Y : (pos.Y - _posToMouse.Y)), _adornerElementSize);

                    //System.Diagnostics.Trace.TraceInformation("Adorner Pos = {0},{1},{2},{3}", rect.X, rect.Y, rect.Width, rect.Height);

                    drawingContext.DrawRectangle(_imageBrush, null, rect);
                }
            }
        }
    }
}

