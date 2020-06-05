using System;
using System.Windows.Controls;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using XamlTheme.Datas;

namespace XamlTheme.Controls
{
    [DefaultEvent("TextChanged"), DefaultProperty("Value")]
    [TemplatePart(Name = TextBoxTemplateName, Type = typeof(TextBox))]
    public class RestrictTextBox : Control
    {
        private static readonly Type _typeofSelf = typeof(RestrictTextBox);

        private const string TextBoxTemplateName = "PART_TextBox";
        private TextBox _valueTextBox;

        private string _lastValidValue = "";
        private bool _isManual;

        static RestrictTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        #region RouteEvent

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<string>), _typeofSelf);
        public event RoutedPropertyChangedEventHandler<string> TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty TextAlignmentProperty = TextBox.TextAlignmentProperty.AddOwner(_typeofSelf);
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = TextBoxBase.IsReadOnlyProperty.AddOwner(_typeofSelf,
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, IsReadOnlyPropertyChangedCallback));
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        private static void IsReadOnlyPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue || e.NewValue == null)
                return;

            ((RestrictTextBox)d).ToggleReadOnlyMode((bool)e.NewValue);
        }

        public static readonly DependencyProperty PatternProperty =
            DependencyProperty.Register("Pattern", typeof(string), typeof(RestrictTextBox), new PropertyMetadata(null, OnPatternChanged, CoercePattern));
        public string Pattern
        {
            get { return (string)GetValue(PatternProperty); }
            set { SetValue(PatternProperty, value); }
        }

        private static void OnPatternChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (RestrictTextBox)d;
            var pattern = (string)e.NewValue;

            if (!string.IsNullOrEmpty(pattern) && !Regex.IsMatch(ctrl.Text, pattern))
                ctrl.Text = string.Empty;
        }

        private static object CoercePattern(DependencyObject d, object value)
        {
            var pattern = (string)value;

            try
            {
                new Regex(pattern);
                return pattern;
            }
            catch
            {
                return null;
            }
        }

        public static readonly DependencyProperty TextCaseProperty =
            DependencyProperty.Register("TextCase", typeof(TextCase), _typeofSelf, new PropertyMetadata(TextCase.Normal));
        public TextCase TextCase
        {
            get { return (TextCase)GetValue(TextCaseProperty); }
            set { SetValue(TextCaseProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), _typeofSelf,
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged, CoerceText));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (RestrictTextBox)d;
            var text = (string)e.NewValue; 

            ctrl.OnTextChanged((string)e.OldValue, text);
        }

        private static object CoerceText(DependencyObject d, object value)
        {
            var ctrl = (RestrictTextBox)d;
            var text = (string)value;

            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(ctrl.Pattern) || !Regex.IsMatch(text, ctrl.Pattern))
                return ctrl._lastValidValue;

            return value;
        }

        #endregion

        #region Virtual

        protected virtual void OnTextChanged(string oldStr, string newStr)
        {
            InternalSetText(newStr);

            if (!string.Equals(oldStr, newStr))
            {
                Debug.Print("[ NumericBox ] ValueChanged, OldStr = {0}, NewStr = {1}, IsManual = {2}", oldStr, newStr, _isManual);
                RaiseEvent(new TextBoxValueChangedEventArgs<string>(oldStr, newStr, _isManual, false, TextChangedEvent));
            }

            _isManual = false;
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_valueTextBox != null)
            {
                _valueTextBox.LostFocus -= OnTextBoxLostFocus;
                _valueTextBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            }

            _valueTextBox = GetTemplateChild(TextBoxTemplateName) as TextBox;

            ToggleReadOnlyMode(IsReadOnly);
            InternalSetText(Text);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            if (Focusable && !IsReadOnly)
            {
                Focused();
                SelectAll();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            switch (e.Key)
            {
                case Key.Enter:

                    DealInputText(_valueTextBox.Text);
                    SelectAll();

                    e.Handled = true;
                    break;
            }
        }

        #endregion

        #region Event

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Focusable && !IsReadOnly && !_valueTextBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;

                Focused();
                SelectAll();
            }
        }

        private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            DealInputText(tb.Text);
        }

        #endregion

        private void Focused()
        {
            _valueTextBox?.Focus();
        }

        private void SelectAll()
        {
            _valueTextBox?.SelectAll();
        }

        private void ToggleReadOnlyMode(bool isReadOnly)
        {
            if (_valueTextBox == null)
                return;

            if (isReadOnly)
            {
                _valueTextBox.LostFocus -= OnTextBoxLostFocus;
                _valueTextBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            }
            else
            {
                _valueTextBox.LostFocus += OnTextBoxLostFocus;
                _valueTextBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            }
        }

        private string GetFormatedText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            switch (TextCase)
            {
                case TextCase.Upper:
                    return text.ToUpper();

                case TextCase.Lower:
                    return text.ToLower();

                default:
                    return text;
            }
        }

        private StringComparison GetStringComparison()
        {
            return TextCase == TextCase.Normal ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        }

        private void InternalSetText(string newStr)
        {
            if (_valueTextBox != null)
            {
                var text = GetFormatedText(newStr);

                _valueTextBox.Text = text;
                _lastValidValue = text;
            }
        }

        private void DealInputText(string text)
        {
            if (!string.Equals(_lastValidValue, text, GetStringComparison())
                && (string.IsNullOrEmpty(Pattern) || string.IsNullOrEmpty(text) || Regex.IsMatch(text, Pattern)))
            {
                _isManual = true;

                Text = text;
                return;
            }

            _valueTextBox.Text = _lastValidValue;
        }
    }
}
