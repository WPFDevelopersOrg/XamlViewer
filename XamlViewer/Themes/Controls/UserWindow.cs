using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

namespace XamlViewer.Themes.Controls
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

        public static readonly DependencyProperty TitleBarBackgroundProperty = DependencyProperty.Register("TitleBarBackground", typeof(Brush), _typeofSelf);
        public Brush TitleBarBackground
        {
            get { return (Brush)GetValue(TitleBarBackgroundProperty); }
            set { SetValue(TitleBarBackgroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarForegroundProperty = DependencyProperty.Register("TitleBarForeground", typeof(Brush), _typeofSelf);
        public Brush TitleBarForeground
        {
            get { return (Brush)GetValue(TitleBarForegroundProperty); }
            set { SetValue(TitleBarForegroundProperty, value); }
        }

        public static readonly DependencyProperty TitleBarContentProperty = DependencyProperty.Register("TitleBarContent", typeof(object), _typeofSelf);
        public object TitleBarContent
        {
            get { return (object)GetValue(TitleBarContentProperty); }
            set { SetValue(TitleBarContentProperty, value); }
        }

        #endregion

        #region Override

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if(this.WindowState == WindowState.Normal)
                this.Top = Math.Max(0, this.Top);
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

        #endregion
    }
}
