using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;

namespace XamlTheme.Controls
{
    [TemplatePart(Name = TextEditorTemplateName, Type = typeof(TextEditor))]
    public class TextEditorEx : Control
    {
        private static readonly Type _typeofSelf = typeof(TextEditorEx);

        private const string TextEditorTemplateName = "PART_TextEditor";

        private TextEditor _partTextEditor = null;
        private FoldingManager _foldingManager = null;
        private XmlFoldingStrategy _foldingStrategy = null;

        private DispatcherTimer _timer = null;
        private bool _disabledTimer = false;

        public string Text
        {
            get
            {
                if (_partTextEditor != null)
                    return _partTextEditor.Text;

                return string.Empty;
            }
            set
            {
                if (_partTextEditor != null)
                {
                    _disabledTimer = true;
                    _partTextEditor.Text = value;
                }
            }
        } 

        static TextEditorEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public TextEditorEx()
        {
            _foldingStrategy = new XmlFoldingStrategy() { ShowAttributesWhenFolded = true };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Math.Max(1, Delay)) };
            _timer.Tick += _timer_Tick;
        } 

        #region RouteEvent

        public static readonly RoutedEvent DelayArrivedEvent = EventManager.RegisterRoutedEvent("DelayArrived", RoutingStrategy.Bubble, typeof(RoutedEventArgs), _typeofSelf);
        public event RoutedEventHandler DelayArrived
        {
            add { AddHandler(DelayArrivedEvent, value); }
            remove { RemoveHandler(DelayArrivedEvent, value); }
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty IsModifiedProperty = TextEditor.IsModifiedProperty.AddOwner(_typeofSelf);
        public bool IsModified
        {
            get { return (bool)GetValue(IsModifiedProperty); }
            set { SetValue(IsModifiedProperty, value); }
        }

        public static readonly DependencyProperty WordWrapProperty = TextEditor.WordWrapProperty.AddOwner(_typeofSelf);
        public bool WordWrap
        {
            get { return (bool)GetValue(WordWrapProperty); }
            set { SetValue(WordWrapProperty, value); }
        }

        public static readonly DependencyProperty ShowLineNumbersProperty = TextEditor.ShowLineNumbersProperty.AddOwner(_typeofSelf);
        public bool ShowLineNumbers
        {
            get { return (bool)GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(double), _typeofSelf, new PropertyMetadata(1d, OnDelayPropertyChanged));
        public double Delay
        {
            get { return (double)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        private static void OnDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var delay = (double)e.NewValue;

            if (ctrl._timer != null)
                ctrl._timer.Interval = TimeSpan.FromSeconds(Math.Max(1, delay));
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_partTextEditor != null)
                _partTextEditor.TextChanged -= _partTextEditor_TextChanged;

            _partTextEditor = GetTemplateChild(TextEditorTemplateName) as TextEditor;

            if (_partTextEditor != null)
            {
                _partTextEditor.TextChanged += _partTextEditor_TextChanged;

                _partTextEditor.TextArea.SelectionCornerRadius = 0;
                _partTextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
                _partTextEditor.TextArea.SelectionBorder = null;
                _partTextEditor.TextArea.SelectionForeground = null;
            }
        } 

        #endregion

        #region Event

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            RaiseEvent(new RoutedEventArgs(DelayArrivedEvent));
        }

        private void _partTextEditor_TextChanged(object sender, EventArgs e)
        {  
            RefreshFoldings();

            if (_disabledTimer)
            {
                _disabledTimer = false;
                return;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        #endregion

        #region Func

        private void RefreshFoldings()
        {
            if (_partTextEditor == null)
                return;

            if (_foldingManager == null)
                _foldingManager = FoldingManager.Install(_partTextEditor.TextArea);

            _foldingStrategy.UpdateFoldings(_foldingManager, _partTextEditor.Document);
        }

        #endregion
    }
}
