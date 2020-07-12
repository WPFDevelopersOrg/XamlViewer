using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using XamlTheme.Adorners;
using XamlUtil.Common; 

namespace XamlTheme.Controls
{
    [TemplatePart(Name = HorizontalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = VerticalRulerTemplateName, Type = typeof(Ruler))]
    [TemplatePart(Name = ScrollContentPresenterTemplateName, Type = typeof(ScrollContentPresenter))]
    public class ZoomBox : ScrollViewer
    {
        private static readonly Type _typeofSelf = typeof(ZoomBox);

        private const string HorizontalRulerTemplateName = "PART_HorizontalRuler";
        private const string VerticalRulerTemplateName = "PART_VerticalRuler";
        private const string ScrollContentPresenterTemplateName = "PART_ScrollContentPresenter";

        private const double MiniScale = 0.01d;
        private const double MaxiScale = 48d;
        private const double ScaleRatio = 1.1;

        private Ruler _partHorizontalRuler;
        private Ruler _partVerticalRuler;
        private ScrollContentPresenter _partScrollContentPresenter;

        private ScaleTransform _partScaleTransform;
        private FrameworkElement _elementContent;
        private bool _isStringContent = false;

        private ViewPoint? _viewPoint = null;
        private struct ViewPoint
        {
            public Point PointToScrollContent { get; set; }
            public Point PointToViewport { get; set; }
        }

        private AdornerLayer _adornerLayer = null;
        private RulerIndicatorAdorner _rulerIndicatorAdorner = null;

        static ZoomBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        } 

        #region readonly Properties

        private static readonly DependencyPropertyKey HorizontalOriginShiftPropertyKey =
           DependencyProperty.RegisterReadOnly("HorizontalOriginShift", typeof(double), _typeofSelf, new PropertyMetadata(0d));
        public static readonly DependencyProperty HorizontalOriginShiftProperty = HorizontalOriginShiftPropertyKey.DependencyProperty;
        public double HorizontalOriginShift
        {
            get { return (double)GetValue(HorizontalOriginShiftProperty); }
        }

        private static readonly DependencyPropertyKey VerticalOriginShiftPropertyKey =
           DependencyProperty.RegisterReadOnly("VerticalOriginShift", typeof(double), _typeofSelf, new PropertyMetadata(0d));
        public static readonly DependencyProperty VerticalOriginShiftProperty = VerticalOriginShiftPropertyKey.DependencyProperty;
        public double VerticalOriginShift
        {
            get { return (double)GetValue(VerticalOriginShiftProperty); }
        }

        #endregion

        #region Properties

        [Bindable(true)]
        public new object Content
        {
            get
            {
                var textBlock = _elementContent as TextBlock;
                if (textBlock != null && _isStringContent)
                    return textBlock.Text;

                return GetValue(ContentProperty);
            }
            set { SetValue(ContentProperty, value); }
        } 

        public static readonly DependencyProperty IsShowRulerProperty =
            DependencyProperty.Register("IsShowRuler", typeof(bool), _typeofSelf, new PropertyMetadata(true));
        public bool IsShowRuler
        {
            get { return (bool)GetValue(IsShowRulerProperty); }
            set { SetValue(IsShowRulerProperty, value); }
        } 

        private static void OnIsShowRulerPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var zoomBox = sender as ZoomBox;
            var isShow = (bool)e.NewValue;

            if (isShow)
                zoomBox.AddAdorner();
            else
                zoomBox.RemoveAdorner();
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), _typeofSelf, new PropertyMetadata(1d, OnScalePropertyChanged, CoerceScale));
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        private static object CoerceScale(DependencyObject d, object value)
        {
            var val = (double)value;

            if (DoubleUtil.LessThan(val, MiniScale))
                return MiniScale;

            if (DoubleUtil.GreaterThan(val, MaxiScale))
                return MaxiScale;

            return value;
        }

        private static void OnScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var zoomBox = sender as ZoomBox;
            zoomBox.UpdateScaleTransform();
        }

        public static readonly DependencyProperty UnitProperty =
            DependencyProperty.Register("Unit", typeof(RulerUnit), _typeofSelf, new PropertyMetadata(RulerUnit.Pixel, OnUnitPropertyChanged));
        public RulerUnit Unit
        {
            get { return (RulerUnit)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        private static void OnUnitPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var zoomBox = sender as ZoomBox;
            zoomBox.UpdateRulerParams();
        }

        public static readonly DependencyProperty RibbonProperty =
            DependencyProperty.Register("Ribbon", typeof(object), _typeofSelf, new PropertyMetadata(null, OnRibbonChanged));
        public object Ribbon
        {
            get { return (object)GetValue(RibbonProperty); }
            set { SetValue(RibbonProperty, value); }
        }

        private static void OnRibbonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var zoomBox = (ZoomBox)d;
            zoomBox.OnContentChanged(e.OldValue, e.NewValue);
        }

        public static readonly DependencyProperty ScrollBarBackgroundProperty =
            DependencyProperty.Register("ScrollBarBackground", typeof(Brush), typeof(ZoomBox), new PropertyMetadata(Brushes.Transparent));
        public Brush ScrollBarBackground
        {
            get { return (Brush)GetValue(ScrollBarBackgroundProperty); }
            set { SetValue(ScrollBarBackgroundProperty, value); }
        }

        #endregion

        #region Override

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            InitContent();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _partHorizontalRuler = GetTemplateChild(HorizontalRulerTemplateName) as Ruler;
            _partVerticalRuler = GetTemplateChild(VerticalRulerTemplateName) as Ruler;

            _partScrollContentPresenter = GetTemplateChild(ScrollContentPresenterTemplateName) as ScrollContentPresenter;

            if (_partHorizontalRuler == null || _partVerticalRuler == null || _partScrollContentPresenter == null)
            {
                throw new NullReferenceException(string.Format("You have missed to specify {0}, {1} or {2} in your template",
                    HorizontalRulerTemplateName, VerticalRulerTemplateName, ScrollContentPresenterTemplateName));
            }

            _partScrollContentPresenter.PreviewMouseMove -= OnPreviewMouseMove;
            _partScrollContentPresenter.PreviewMouseMove += OnPreviewMouseMove;

            _partScrollContentPresenter.MouseLeave -= OnMouseLeave;
            _partScrollContentPresenter.MouseLeave += OnMouseLeave;

            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            _rulerIndicatorAdorner = new RulerIndicatorAdorner(this, 20);

            InitContent();
            UpdateScaleTransform();
            UpdateRulerParams();
        }

        #endregion

        #region Event

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            RemoveAdorner();
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            AddAdorner();
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e)
        {
            base.OnScrollChanged(e);

            if (!IsLoaded)
                return;

            UpdateRulerParams();
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (_partScrollContentPresenter.CanHorizontallyScroll || _partScrollContentPresenter.CanVerticallyScroll)
                    _viewPoint = ResetViewPoint();

                if (e.Delta > 0)
                {
                    ZoomIn();
                }
                if (e.Delta < 0)
                {
                    ZoomOut();
                }

                e.Handled = true;
            }
        }

        #endregion

        #region Func

        private void AddAdorner()
        {
            if (!IsShowRuler || _adornerLayer == null || _rulerIndicatorAdorner == null)
                return;

            var adorners = _adornerLayer.GetAdorners(this);

            if (adorners == null || adorners.Length == 0)
                _adornerLayer.Add(_rulerIndicatorAdorner);

            _adornerLayer.Update();
        }

        private void RemoveAdorner()
        {
            if (_adornerLayer != null && _rulerIndicatorAdorner != null)
                _adornerLayer.Remove(_rulerIndicatorAdorner);

            _adornerLayer.Update();
        }

        private void InitContent()
        {
            if (_partScrollContentPresenter == null)
                return;

            if (Content != null)
            {
                var element = Content as FrameworkElement;
                if (element == null)
                {
                    _elementContent = new TextBlock { Text = Content.ToString() };
                    _isStringContent = true;
                }
                else
                {
                    _elementContent = element;
                    _isStringContent = false;
                }
            }
            else
            {
                _elementContent = null;
            }

            if (_elementContent != null)
            {
                _partScaleTransform = new ScaleTransform(Scale, Scale);

                _elementContent.RenderTransformOrigin = new Point(0.5, 0.5);
                _elementContent.LayoutTransform = _partScaleTransform;
            }

            _partScrollContentPresenter.Content = _elementContent;
        }

        private void UpdateRulerParams()
        {
            if (_partScrollContentPresenter == null || _elementContent == null || _partHorizontalRuler == null || _partVerticalRuler == null || _partScaleTransform == null)
                return;

            using (_partHorizontalRuler.DeferRefresh())
            {
                KeepingHorizontalViewPoint(_partHorizontalRuler.Scale, _partScrollContentPresenter.CanHorizontallyScroll);

                _partHorizontalRuler.Scale = Scale;
                _partHorizontalRuler.Unit = Unit;

                var offset = _elementContent.TranslatePoint(new Point(), _partScrollContentPresenter);
                SetValue(HorizontalOriginShiftPropertyKey, offset.X);
            }

            using (_partVerticalRuler.DeferRefresh())
            {
                KeepingVerticalViewPoint(_partVerticalRuler.Scale, _partScrollContentPresenter.CanVerticallyScroll);

                _partVerticalRuler.Scale = Scale;
                _partVerticalRuler.Unit = Unit;

                var offset = _elementContent.TranslatePoint(new Point(), _partScrollContentPresenter);
                SetValue(VerticalOriginShiftPropertyKey, offset.Y);
            }

            _viewPoint = null;
        }

        private void UpdateScaleTransform()
        {
            if (_partScaleTransform == null)
                return;

            _partScaleTransform.ScaleX = Scale;
            _partScaleTransform.ScaleY = Scale;
        }

        private void KeepingHorizontalViewPoint(double lastScale, bool canHorizontallyScroll)
        {
            if (_viewPoint.HasValue && canHorizontallyScroll)
                ScrollToHorizontalOffset((_viewPoint.Value.PointToScrollContent.X - _elementContent.Margin.Left) * Scale / lastScale + _elementContent.Margin.Left - _viewPoint.Value.PointToViewport.X);
        }

        private void KeepingVerticalViewPoint(double lastScale, bool canVerticallyScroll)
        {
            if (_viewPoint.HasValue && canVerticallyScroll)
                ScrollToVerticalOffset((_viewPoint.Value.PointToScrollContent.Y - _elementContent.Margin.Top) * Scale / lastScale + _elementContent.Margin.Top - _viewPoint.Value.PointToViewport.Y);
        }

        private ViewPoint ResetViewPoint()
        {
            var viewPoint = new ViewPoint
            {
                PointToViewport = Mouse.GetPosition(_partScrollContentPresenter)
            };

            viewPoint.PointToScrollContent = new Point(viewPoint.PointToViewport.X + HorizontalOffset, viewPoint.PointToViewport.Y + VerticalOffset);

            return viewPoint;
        }

        public void ZoomIn()
        {
            Scale = Scale * ScaleRatio;
        }

        public void ZoomOut()
        {
            Scale = Scale / ScaleRatio;
        }

        #endregion
    }
}
