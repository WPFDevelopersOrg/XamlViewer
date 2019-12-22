using System;
using System.Diagnostics; 
using System.Windows;
using System.Windows.Controls; 

namespace XamlTheme.Controls
{
    /// <summary>
    /// HyperLink.xaml 的交互逻辑
    /// </summary>
    public partial class Hyperlink : UserControl
    {
        public Hyperlink()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty NavigateUriProperty =
            DependencyProperty.Register("NavigateUri", typeof(Uri), typeof(Hyperlink), new PropertyMetadata(null, OnNavigateUriPropertyChanged));
        public Uri NavigateUri
        {
            get { return (Uri)GetValue(NavigateUriProperty); }
            set { SetValue(NavigateUriProperty, value); }
        }

        static void OnNavigateUriPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Hyperlink;
            ctrl.LocalHyperlink.NavigateUri = (Uri)(e.NewValue);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Hyperlink), new PropertyMetadata(null, OnTextPropertyChanged));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as Hyperlink;
            ctrl.LinkText.Text = (string)(e.NewValue);
        } 

        private void LocalHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(LocalHyperlink.NavigateUri.AbsoluteUri));
        }
    }
}
