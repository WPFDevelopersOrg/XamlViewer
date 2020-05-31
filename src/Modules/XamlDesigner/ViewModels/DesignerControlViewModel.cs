using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.Versioning;
using XamlDesigner.Views;
using XamlService.Events;
using XamlService.Payloads;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using SWF = System.Windows.Forms;

namespace XamlDesigner.ViewModels
{
    public class DesignerControlViewModel : BindableBase, IDisposable
    {
        private string _fileGuid = null;
        private bool _canSnapshot = false;

        private IEventAggregator _eventAggregator = null;
        private RefreshDesignerEvent _refreshDesignerEvent = null;

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }
        public DelegateCommand SnapshotCommand { get; private set; }

        public DesignerControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _refreshDesignerEvent = _eventAggregator.GetEvent<RefreshDesignerEvent>();
            _refreshDesignerEvent.Subscribe(OnRefreshDesigner, ThreadOption.UIThread, false, tab => tab.Guid == _fileGuid);

            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
            SnapshotCommand = new DelegateCommand(OnSnapshot, CanSnapshot);
        }

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetProperty(ref _isReadOnly, value); }
        }

        private string _version = "1.0.0";
        public string Version
        {
            get { return _version; }
            set { SetProperty(ref _version, value); }
        }

#if NETFRAMEWORK
        private string _targetFramework = ".Net Framework 4.5";
#else
        private string _targetFramework = ".Net Core 3.1";
#endif
        public string TargetFramework
        {
            get { return _targetFramework; }
            set { SetProperty(ref _targetFramework, value); }
        }

#if NETFRAMEWORK
        private string _minReferenceDescription = ".NETFramework 4.0 or above";
#else
        private string _minReferenceDescription = ".NETCoreApp 3.0 or above";
#endif
        public string MinReferenceDescription
        {
            get { return _minReferenceDescription; }
            set { SetProperty(ref _minReferenceDescription, value); }
        }

        private FrameworkElement _element;
        public FrameworkElement Element
        {
            get { return _element; }
            set { SetProperty(ref _element, value); }
        }

        private void OnLoaded(RoutedEventArgs e)
        {
            var designerControl = e.OriginalSource as DesignerControl;

            var selectInfo = (TabSelectInfo)(RegionContext.GetObservableContext(designerControl).Value);
            if (selectInfo != null)
                _fileGuid = selectInfo.Guid;

            var targetFw = Application.ResourceAssembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), false).FirstOrDefault() as TargetFrameworkAttribute;
            if (targetFw != null)
                TargetFramework = targetFw.FrameworkName.Replace(",Version=v", " ");

            var version = Application.ResourceAssembly.GetName().Version;
            Version = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build);
        }

        private bool CanSnapshot()
        {
            return _canSnapshot;
        }

        private void OnSnapshot()
        {
            var sfd = new SWF.SaveFileDialog { Filter = "PNG|*.png", FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") };
            if (sfd.ShowDialog() != SWF.DialogResult.OK)
                return;

            using (var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.ReadWrite))
            {
                var drawingVisual = new DrawingVisual();
                var width = Element.ActualWidth;
                var height = Element.ActualHeight;

                using (var context = drawingVisual.RenderOpen())
                {
                    var contentBounds = VisualTreeHelper.GetDescendantBounds(Element);
                    context.DrawRectangle(new VisualBrush(Element) { Stretch = Stretch.Fill, Viewbox = new Rect(0, 0, width / contentBounds.Width, height / contentBounds.Height) }, null, new Rect(0, 0, width, height));
                }

                var rtb = new RenderTargetBitmap((int)width, (int)height, 96, 96, PixelFormats.Default);
                rtb.Render(drawingVisual);

                var encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(rtb));
                encoder.Save(fs);
            }
        }

        private void OnRefreshDesigner(TabInfo tabInfo)
        {
            if (tabInfo.Guid != _fileGuid)
                return;

            IsReadOnly = tabInfo.IsReadOnly;
            _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(new ProcessInfo { status = ProcessStatus.Compile, Guid = _fileGuid });

            try
            {
                RefreshSnapshotStatus(false);

                var obj = XamlReader.Parse(tabInfo.FileContent);
                var window = obj as Window;
                if (window != null)
                {
                    ShowLocalText("Window", 15);

                    window.Owner = Application.Current.MainWindow;
                    window.Show();
                }
                else
                {
                    Element = obj as FrameworkElement;
                    RefreshSnapshotStatus(Element != null);
                }
            }
            catch (Exception ex)
            {
                ShowLocalText("Error: " + ex.Message);
            }
            finally
            {
                _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(new ProcessInfo { status = ProcessStatus.FinishCompile, Guid = _fileGuid });
            }
        }

        private void ShowLocalText(string text, double fontSize = 14d)
        {
            Element = new TextBlock
            {
                Text = text,
                Margin = new Thickness(5),
                FontSize = fontSize,
                Foreground = Brushes.DarkSlateGray,
                FontWeight = FontWeights.Medium,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private void RefreshSnapshotStatus(bool canSnapshot)
        {
            _canSnapshot = canSnapshot;
            SnapshotCommand.RaiseCanExecuteChanged();
        }

        public void Dispose()
        {
            _refreshDesignerEvent.Unsubscribe(OnRefreshDesigner);
        }
    }
}
