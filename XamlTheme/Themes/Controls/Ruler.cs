using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media; 
using System.Windows.Controls;
using XamlUtil.Common;
using XamlUtil.Functions;
using XamlViewer.Utils;

namespace XamlTheme.Themes.Controls
{
    public enum RulerUnit
    {
        Pixel,
        Millimeter,
        Centimeter,
        Inch,
        Foot
    }

    public enum MarkDock
    {
        Up,
        Down
    }

    public class Ruler : FrameworkElement
    {
        private static readonly Type _typeofSelf = typeof(Ruler);
        private readonly DrawingGroup _drawingGroup = new DrawingGroup();

        //96dpi:1   120dpi:1.25
        private double _devicePixelUnit = 1;
        private Pen _markPen = null;
        private Pen _baselinePen = null;

        private int _deferLevel = 0;
        private bool _needRefresh = false;

        static Ruler()
        {
            VisibilityProperty.OverrideMetadata(_typeofSelf, new PropertyMetadata(OnVisibilityChanged));
        }

        static void OnVisibilityChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), _typeofSelf, new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged), IsValidOrientation);
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
        }

        static bool IsValidOrientation(object o)
        {
            var value = (Orientation)o;
            return value == Orientation.Horizontal || value == Orientation.Vertical;
        }

        public static readonly DependencyProperty MarkDockProperty =
            DependencyProperty.Register("MarkDock", typeof(MarkDock), _typeofSelf, new PropertyMetadata(MarkDock.Up, OnMarkDockPropertyChanged));
        public MarkDock MarkDock
        {
            get { return (MarkDock)GetValue(MarkDockProperty); }
            set { SetValue(MarkDockProperty, value); }
        }

        static void OnMarkDockPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
        }

        public static readonly DependencyProperty UnitProperty =
           DependencyProperty.Register("Unit", typeof(RulerUnit), _typeofSelf, new PropertyMetadata(RulerUnit.Pixel, OnUnitPropertyChanged));
        public RulerUnit Unit
        {
            get { return (RulerUnit)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        static void OnUnitPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
        }

        public static readonly DependencyProperty ShiftProperty =
            DependencyProperty.Register("Shift", typeof(double), _typeofSelf, new PropertyMetadata(0d, OnShiftPropertyChanged));
        public double Shift
        {
            get { return (double)GetValue(ShiftProperty); }
            set { SetValue(ShiftProperty, value); }
        }

        static void OnShiftPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
        }

        public static readonly DependencyProperty ScaleProperty =
            DependencyProperty.Register("Scale", typeof(double), _typeofSelf, new FrameworkPropertyMetadata(1d, OnScalePropertyChanged));
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }
        static void OnScalePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ruler = sender as Ruler;
            if (ruler._deferLevel == 0)
                ruler.Render();
            else
                ruler._needRefresh = true;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_markPen == null)
            {
                _devicePixelUnit = DpiUtil.GetDevicePixelUnit(this);
                _markPen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7F7F7F")), 1.0 / _devicePixelUnit);
                _markPen.Freeze();
            }

            if (_baselinePen == null)
            {
                _devicePixelUnit = DpiUtil.GetDevicePixelUnit(this);
                _baselinePen = new Pen(Brushes.Black, 1.0 / _devicePixelUnit);
                _baselinePen.Freeze();
            }

            Render();
            drawingContext.DrawDrawing(_drawingGroup);
        }

        #region DeferRefresh

        public IDisposable DeferRefresh()
        {
            ++_deferLevel;

            return new DeferRefresh(
                () =>
                {
                    --_deferLevel;

                    if (_deferLevel == 0 && _needRefresh)
                        Render();
                });
        }

        #endregion

        #region Private Func

        private double BaseLineOffset
        {
            get { return this.MarkDock == MarkDock.Up ? MaxSpan - 1.0 / _devicePixelUnit : 0d; }
        }

        private double MaxLength
        {
            get { return Orientation == Orientation.Horizontal ? ActualWidth : ActualHeight; }
        }

        private double MaxSpan
        {
            get { return Orientation == Orientation.Horizontal ? ActualHeight : ActualWidth; }
        }

        private void Render()
        {
            if (_baselinePen == null || _markPen == null)
                return;

            var guidelineSet = new GuidelineSet();

            using (var dc = _drawingGroup.Open())
            {
                if(Visibility == Visibility.Collapsed)
                {
                    _drawingGroup.GuidelineSet = null;
                    return;
                }

                var mainStep = 0d;
                var miniStep = 0d;
                var miniStepCount = 0;

                InitStepInfo(ref mainStep, ref miniStep, ref miniStepCount);

                DrawOffsetRight(dc, miniStep, miniStepCount, guidelineSet);
                DrawOffsetLeft(dc, miniStep, miniStepCount, guidelineSet);

                if (Orientation == Orientation.Horizontal)
                    dc.DrawLine(_baselinePen, new Point(0, BaseLineOffset), new Point(MaxLength, BaseLineOffset));
                else
                    dc.DrawLine(_baselinePen, new Point(BaseLineOffset, 0), new Point(BaseLineOffset, MaxLength));

                _needRefresh = false;
            }

            _drawingGroup.GuidelineSet = guidelineSet;
        }

        private int GetPrecision()
        {
            var result = 0;
            switch (Unit)
            {
                case RulerUnit.Pixel:
                    result = 0;
                    break;
                case RulerUnit.Inch:
                    result = 2;
                    break;
                case RulerUnit.Foot:
                    result = 3;
                    break;
                case RulerUnit.Millimeter:
                    result = 0;
                    break;
                case RulerUnit.Centimeter:
                    result = 1;
                    break;
            }

            return result;
        }

        private double GetBaseStep()
        {
            var result = 0d;
            switch (Unit)
            {
                case RulerUnit.Pixel:
                    result = 1;
                    break;
                case RulerUnit.Inch:
                    result = 0.96;
                    break;
                case RulerUnit.Foot:
                    result = 1.152;
                    break;
                case RulerUnit.Millimeter:
                    result = 3.7795;
                    break;
                case RulerUnit.Centimeter:
                    result = 3.7795;
                    break;
            }

            return result;
        }

        private void InitStepInfo(ref double mainStep, ref double miniStep, ref int miniStepCount)
        {
            var tempScale = Scale * GetBaseStep();
            var tempStep = tempScale;

            while (true)
            {
                if (DoubleUtil.GreaterThanOrClose(tempStep / 4, 20))
                {
                    mainStep = tempStep;
                    miniStep = tempStep / 20;
                    miniStepCount = 20;

                    break;
                }

                if (DoubleUtil.GreaterThanOrClose(tempStep / 4, 10))
                {
                    mainStep = tempStep;
                    miniStep = tempStep / 10;
                    miniStepCount = 10;

                    break;
                }

                if (DoubleUtil.GreaterThanOrClose(tempStep / 5, 5))
                {
                    mainStep = tempStep;
                    miniStep = tempStep / 5;
                    miniStepCount = 5;

                    break;
                }

                if (DoubleUtil.LessThan(Scale, 0.1))
                {
                    if (DoubleUtil.AreClose(tempStep, tempScale))
                        tempStep = tempScale * 500;
                    else
                        tempStep += tempScale * 500;
                }
                else if (DoubleUtil.LessThan(Scale, 1))
                {
                    if (DoubleUtil.AreClose(tempStep, tempScale))
                        tempStep = tempScale * 50;
                    else
                        tempStep += tempScale * 50;
                }
                else if (DoubleUtil.LessThanOrClose(Scale, 10))
                {
                    if (DoubleUtil.AreClose(tempStep, tempScale))
                        tempStep = tempScale * 5;
                    else
                        tempStep += tempScale * 5;
                }
                else
                {
                    if (DoubleUtil.AreClose(tempStep, tempScale))
                        tempStep = tempScale * 2;
                    else
                        tempStep += tempScale * 2;
                }
            }
        }

        private FormattedText GetFormattedText(string textToFormat)
        {
            var ft = new FormattedText(
                       textToFormat,
                       CultureInfo.CurrentCulture,
                       FlowDirection.LeftToRight,
                       new Typeface("Arial"),
                       DpiUtil.PtToPixel(6),
                       Brushes.Black,
                       null,
                       TextFormattingMode.Display);
            ft.SetFontWeight(FontWeights.Regular);
            ft.TextAlignment = TextAlignment.Left;

            return ft;
        }

        private void DrawStep(DrawingContext dc, int stepIndex, double stepOffset, int miniStepCount, GuidelineSet guidelineSet, bool ignoreFirstMark = false)
        {
            if (stepIndex % miniStepCount == 0)
            {
                var mainstepOffset = stepOffset - Shift * Scale * DpiUtil.GetPixelPerUnit(Unit);
                var mark = Math.Round(mainstepOffset / (Scale * DpiUtil.GetPixelPerUnit(Unit)), GetPrecision());

                if (ignoreFirstMark && DoubleUtil.AreClose(mark, 0))
                    return;

                DrawLine(dc, new Point(stepOffset, 0), new Point(stepOffset, MaxSpan - 1), guidelineSet);

                var ft = GetFormattedText(mark.ToString("#0.###"));
                var ftXOffset = stepOffset + 1;
                var ftYOffset = MarkDock == MarkDock.Up ? 0 : MaxSpan - ft.Height;

                if (Orientation == Orientation.Horizontal)
                    dc.DrawText(ft, new Point(ftXOffset, ftYOffset));
                else
                {
                    dc.PushTransform(new RotateTransform(90, MarkDock == MarkDock.Up ? 4 : MaxSpan - 5.5, ftXOffset + ft.Height / 2));
                    dc.DrawText(ft, new Point(ftYOffset, ftXOffset));
                    dc.Pop();
                }
            }
            else
            {
                if (miniStepCount == 5)
                {
                    DrawLine(dc, new Point(stepOffset, MaxSpan * 0.5), new Point(stepOffset, BaseLineOffset), guidelineSet);
                }

                if (miniStepCount == 10)
                {
                    if (stepIndex % 5 == 0)
                        DrawLine(dc, new Point(stepOffset, MaxSpan * (MarkDock == MarkDock.Up ? 0.2 : 0.8)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else if (stepIndex % 2 == 0)
                        DrawLine(dc, new Point(stepOffset, MaxSpan * 0.5), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else
                        DrawLine(dc, new Point(stepOffset, MaxSpan * (MarkDock == MarkDock.Up ? 0.625 : 0.375)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                }

                if (miniStepCount == 20)
                {
                    if (stepIndex % 10 == 0)
                        DrawLine(dc, new Point(stepOffset, MaxSpan * (MarkDock == MarkDock.Up ? 0.2 : 0.8)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else if (stepIndex % 5 == 0)
                        DrawLine(dc, new Point(stepOffset, MaxSpan * 0.5), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else if (stepIndex % 2 == 0)
                        DrawLine(dc, new Point(stepOffset, MaxSpan * (MarkDock == MarkDock.Up ? 0.625 : 0.375)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                    else
                        DrawLine(dc, new Point(stepOffset, MaxSpan * (MarkDock == MarkDock.Up ? 0.71875 : 0.28125)), new Point(stepOffset, BaseLineOffset), guidelineSet);
                }
            }
        }

        private void DrawLine(DrawingContext dc, Point startPoint, Point endPoint, GuidelineSet guidelineSet)
        {
            var sp = Orientation == Orientation.Horizontal ? startPoint : new Point(startPoint.Y, startPoint.X);
            var ep = Orientation == Orientation.Horizontal ? endPoint : new Point(endPoint.Y, endPoint.X);
            var halfPenWidth = _markPen.Thickness / 2;

            dc.DrawLine(_markPen, sp, ep);

            if (Orientation == Orientation.Horizontal)
            {
                guidelineSet.GuidelinesX.Add(sp.X + halfPenWidth);

                if (!guidelineSet.GuidelinesY.Contains(sp.Y))
                    guidelineSet.GuidelinesY.Add(sp.Y);

                if (!guidelineSet.GuidelinesY.Contains(ep.Y - halfPenWidth))
                    guidelineSet.GuidelinesY.Add(ep.Y - halfPenWidth);
            }
            else
            {
                guidelineSet.GuidelinesY.Add(sp.Y + halfPenWidth);

                if (!guidelineSet.GuidelinesX.Contains(sp.X))
                    guidelineSet.GuidelinesX.Add(sp.X);

                if (!guidelineSet.GuidelinesX.Contains(ep.X - halfPenWidth))
                    guidelineSet.GuidelinesX.Add(ep.X - halfPenWidth);
            }
        }

        private void DrawOffsetRight(DrawingContext dc, double miniStep, int miniStepCount, GuidelineSet guidelineSet)
        {
            var realShift = Shift * Scale * DpiUtil.GetPixelPerUnit(Unit);
            if (DoubleUtil.GreaterThanOrClose(realShift, MaxLength))
                return;

            var stepIndex = 0;
            for (var stepOffset = realShift; stepOffset < MaxLength; stepOffset += miniStep)
            {
                if (DoubleUtil.GreaterThanOrClose(stepOffset, 0))
                    DrawStep(dc, stepIndex, stepOffset, miniStepCount, guidelineSet);

                stepIndex++;
            }
        }

        private void DrawOffsetLeft(DrawingContext dc, double miniStep, int miniStepCount, GuidelineSet guidelineSet)
        {
            if (DoubleUtil.LessThanOrClose(Shift, 0))
                return;

            var stepIndex = 0;
            for (var stepOffset = Shift * Scale * DpiUtil.GetPixelPerUnit(Unit); stepOffset >= 0; stepOffset -= miniStep)
            {
                if (DoubleUtil.LessThanOrClose(stepOffset, MaxLength))
                    DrawStep(dc, stepIndex, stepOffset, miniStepCount, guidelineSet, true);

                stepIndex++;
            }
        }

        #endregion
    }
}
