using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

namespace XamlTheme.Controls
{
    public class UserWindow : Window
    {
        private static readonly Type _typeofSelf = typeof(UserWindow);

        static UserWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UserWindow), new FrameworkPropertyMetadata(typeof(UserWindow)));
        }

        public UserWindow()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
        }

        #region Command

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty IsAltAndF4EnabledProperty = DependencyProperty.Register("IsAltAndF4Enabled", typeof(bool), _typeofSelf, new PropertyMetadata(true));
        public bool IsAltAndF4Enabled
        {
            get { return (bool)GetValue(IsAltAndF4EnabledProperty); }
            set { SetValue(IsAltAndF4EnabledProperty, value); }
        }

        public static readonly DependencyProperty IsCloseCanceledProperty = DependencyProperty.Register("IsCloseCanceled", typeof(bool), _typeofSelf, new PropertyMetadata(false));
        public bool IsCloseCanceled
        {
            get { return (bool)GetValue(IsCloseCanceledProperty); }
            set { SetValue(IsCloseCanceledProperty, value); }
        }

        public static readonly DependencyProperty IsCloseEnabledProperty = DependencyProperty.Register("IsCloseEnabled", typeof(bool), _typeofSelf, new PropertyMetadata(true));
        public bool IsCloseEnabled
        {
            get { return (bool)GetValue(IsCloseEnabledProperty); }
            set { SetValue(IsCloseEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsCloseVisibleProperty = DependencyProperty.Register("IsCloseVisible", typeof(bool), _typeofSelf, new PropertyMetadata(true));
        public bool IsCloseVisible
        {
            get { return (bool)GetValue(IsCloseVisibleProperty); }
            set { SetValue(IsCloseVisibleProperty, value); }
        }

        public static readonly DependencyProperty HideOriginalTitleProperty = DependencyProperty.Register("HideOriginalTitle", typeof(bool), _typeofSelf, new PropertyMetadata(false));
        public bool HideOriginalTitle
        {
            get { return (bool)GetValue(HideOriginalTitleProperty); }
            set { SetValue(HideOriginalTitleProperty, value); }
        }

        public static readonly DependencyProperty CaptionFontSizeProperty = DependencyProperty.Register("CaptionFontSize", typeof(double), _typeofSelf, new PropertyMetadata(14d));
        public double CaptionFontSize
        {
            get { return (double)GetValue(CaptionFontSizeProperty); }
            set { SetValue(CaptionFontSizeProperty, value); }
        }
		
		public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register("CaptionHeight", typeof(double), _typeofSelf, new PropertyMetadata(30d));
        public double CaptionHeight
        {
            get { return (double)GetValue(CaptionHeightProperty); }
            set { SetValue(CaptionHeightProperty, value); }
        }

        public static readonly DependencyProperty CaptionBackgroundProperty = DependencyProperty.Register("CaptionBackground", typeof(Brush), _typeofSelf);
        public Brush CaptionBackground
        {
            get { return (Brush)GetValue(CaptionBackgroundProperty); }
            set { SetValue(CaptionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CaptionForegroundProperty = DependencyProperty.Register("CaptionForeground", typeof(Brush), _typeofSelf);
        public Brush CaptionForeground
        {
            get { return (Brush)GetValue(CaptionForegroundProperty); }
            set { SetValue(CaptionForegroundProperty, value); }
        }

        public static readonly DependencyProperty CaptionContentProperty = DependencyProperty.Register("CaptionContent", typeof(object), _typeofSelf);
        public object CaptionContent
        {
            get { return (object)GetValue(CaptionContentProperty); }
            set { SetValue(CaptionContentProperty, value); }
        }

        #endregion

        #region Override

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Normal)
                Top = Math.Max(0, Top);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (IsCloseEnabled == false && DialogResult == null)
                e.Cancel = true;
            else
                base.OnClosing(e);
        } 

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (!IsAltAndF4Enabled && e.KeyStates == Keyboard.GetKeyStates(Key.F4) && Keyboard.Modifiers == ModifierKeys.Alt)
                e.Handled = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (SizeToContent != SizeToContent.Manual && WindowState == WindowState.Normal)
                InvalidateMeasure();
        }

        #endregion
    }
}
