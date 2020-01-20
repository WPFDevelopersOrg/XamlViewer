using System;
using System.Windows;
using System.Windows.Media;
using XamlTheme.Utils;
using XamlUtil.Common;

namespace XamlTheme.Controls
{
    public class DrawingIcon : FrameworkElement
    {
        private Point? _dpi;
        
        public static readonly DependencyProperty NormalDrawingProperty = DependencyProperty.Register("NormalDrawing", typeof(Drawing), typeof(DrawingIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public Drawing NormalDrawing
        {
            get { return (Drawing)GetValue(NormalDrawingProperty); }
            set { SetValue(NormalDrawingProperty, value); }
        }

        public static readonly DependencyProperty HDPIDrawingProperty = DependencyProperty.Register("HDPIDrawing", typeof(Drawing), typeof(DrawingIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public Drawing HDPIDrawing
        {
            get { return (Drawing)GetValue(HDPIDrawingProperty); }
            set { SetValue(HDPIDrawingProperty, value); }
        }

        protected virtual Size MeasureOverride(Size availableSize)
        {
            if(!_dpi.HasValue)
                _dpi = DpiUtil.GetDpi(this);
            
            var drawing = DoubleUtil.LessThanOrClose(_dpi.Value.X, 96) ? NormalDrawing : HDPIDrawing;
            if (drawing != null && !drawing.Bounds.IsEmpty)
            {
                var bounds = drawing.Bounds;
                return new Size(Math.Min(availableSize.Width, bounds.Width), Math.Min(availableSize.Height, bounds.Height));
            }
            
            return base.MeasureOverride(availableSize);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if(!_dpi.HasValue)
                _dpi = DpiUtil.GetDpi(this);
            
            if (NormalDrawing == null || HDPIDrawing == null)
            {
                drawingContext.DrawDrawing(null);
                return;
            } 
            
            if (DoubleUtil.LessThanOrClose(_dpi.Value.X, 96))
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
