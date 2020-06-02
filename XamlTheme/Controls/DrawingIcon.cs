using System;
using System.Windows;
using System.Windows.Media;
using XamlTheme.Utils;
using XamlUtil.Common;

namespace XamlTheme.Controls
{
    public class DrawingIcon : UIElement
    {
        private Point? _dpi;
        
        static DrawingIcon()
        {
            IsHitTestVisibleProperty.OverrideMetadata(typeof(DrawingIcon), new FrameworkPropertyMetadata(false));
        }
        
        public static readonly DependencyProperty DrawingProperty = DependencyProperty.Register("Drawing", typeof(Drawing), typeof(DrawingIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public Drawing Drawing
        {
            get { return (Drawing)GetValue(DrawingProperty); }
            set { SetValue(DrawingProperty, value); }
        }

        public static readonly DependencyProperty HDPIDrawingProperty = DependencyProperty.Register("HDPIDrawing", typeof(Drawing), typeof(DrawingIcon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));
        public Drawing HDPIDrawing
        {
            get { return (Drawing)GetValue(HDPIDrawingProperty); }
            set { SetValue(HDPIDrawingProperty, value); }
        }

        protected override Size MeasureCore(Size availableSize)
        {
            if(!_dpi.HasValue)
                _dpi = DpiUtil.GetDpi(this);
            
            var drawing = (DoubleUtil.LessThanOrClose(_dpi.Value.X, 96) || HDPIDrawing == null) ? Drawing : HDPIDrawing;
            if (drawing != null && !drawing.Bounds.IsEmpty)
            {
                var bounds = drawing.Bounds;
                return new Size(Math.Min(availableSize.Width, bounds.Width), Math.Min(availableSize.Height, bounds.Height));
            }
            
            return base.MeasureCore(availableSize);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if(!_dpi.HasValue)
                _dpi = DpiUtil.GetDpi(this);
            
            if (Drawing == null || HDPIDrawing == null)
            {
                drawingContext.DrawDrawing(null);
                return;
            } 
            
            if (DoubleUtil.LessThanOrClose(_dpi.Value.X, 96) || HDPIDrawing == null)
            {
                RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
                drawingContext.DrawDrawing(Drawing);
            }
            else
            {
                RenderOptions.SetEdgeMode(this, EdgeMode.Unspecified);
                drawingContext.DrawDrawing(HDPIDrawing);
            }
        }
    }
}
