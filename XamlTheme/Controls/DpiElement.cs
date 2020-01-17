using System.Windows;
using System.Windows.Media;
using XamlTheme.Utils;
using XamlUtil.Common;

namespace XamlTheme.Controls
{
    public class DpiElement:FrameworkElement
    {
        public static readonly DependencyProperty NormalDrawingProperty = DependencyProperty.Register("NormalDrawing", typeof(Drawing), typeof(DpiElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Drawing NormalDrawing
        {
            get { return (Drawing)GetValue(NormalDrawingProperty); }
            set { SetValue(NormalDrawingProperty, value); }
        }

        public static readonly DependencyProperty HDPIDrawingProperty = DependencyProperty.Register("HDPIDrawing", typeof(Drawing), typeof(DpiElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public Drawing HDPIDrawing
        {
            get { return (Drawing)GetValue(HDPIDrawingProperty); }
            set { SetValue(HDPIDrawingProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (NormalDrawing == null || HDPIDrawing == null)
            {
                drawingContext.DrawDrawing(null);
                return;
            }

            var dpi = DpiUtil.GetDpi(this);

            if (DoubleUtil.LessThanOrClose(dpi.X, 96))
            {
                RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
                drawingContext.DrawDrawing(NormalDrawing);
            }
            else
            {
                RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
                drawingContext.DrawDrawing(HDPIDrawing);
            }
        }
    }
}
