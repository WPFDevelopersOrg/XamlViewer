using System.ComponentModel; 
using System.Windows;
using System.Windows.Controls.Primitives;

namespace XamlTheme.Controls
{
    public class StatusToggle : ToggleButton
    {
        static StatusToggle()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusToggle), new FrameworkPropertyMetadata(typeof(StatusToggle)));
        } 

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(StatusToggle), new FrameworkPropertyMetadata(new CornerRadius(0, 0, 0, 0)));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (StatusToggle)d; 
            ctrl.OnContentChanged(e.OldValue, e.NewValue);
        } 

        public static readonly DependencyProperty CheckedContentProperty = DependencyProperty.Register("CheckedContent", typeof(object), typeof(StatusToggle),
            new FrameworkPropertyMetadata((object)null,OnContentChanged));
        [Bindable(true)]
        public object CheckedContent
        {
            get { return GetValue(CheckedContentProperty); }
            set { SetValue(CheckedContentProperty, value); }
        }

        public static readonly DependencyProperty UnCheckedContentProperty = DependencyProperty.Register("UnCheckedContent", typeof(object), typeof(StatusToggle),
            new FrameworkPropertyMetadata((object)null,OnContentChanged));
        [Bindable(true)]
        public object UnCheckedContent
        {
            get { return GetValue(UnCheckedContentProperty); }
            set { SetValue(UnCheckedContentProperty, value); }
        }

        public static readonly DependencyProperty CheckedToolTipProperty = DependencyProperty.Register("CheckedToolTip", typeof(string), typeof(StatusToggle));
        public string CheckedToolTip
        {
            get { return (string)GetValue(CheckedToolTipProperty); }
            set { SetValue(CheckedToolTipProperty, value); }
        }

        public static readonly DependencyProperty UnCheckedToolTipProperty = DependencyProperty.Register("UnCheckedToolTip", typeof(string), typeof(StatusToggle));
        public string UnCheckedToolTip
        {
            get { return (string)GetValue(UnCheckedToolTipProperty); }
            set { SetValue(UnCheckedToolTipProperty, value); }
        }
    }
}
