using Prism.Mvvm;
using System;
using System.Windows.Controls;
using System.Windows.Markup;
using Prism.Events;
using XamlService.Events;
using System.Windows;
using XamlService.Payloads;
using Prism.Commands;
using XamlDesigner.Views;
using Prism.Regions;

namespace XamlDesigner.ViewModels
{
    public class DesignerControlViewModel : BindableBase, IDisposable
    {
        private string _fileGuid = null;
        private IEventAggregator _eventAggregator = null;
        private RefreshDesignerEvent _refreshDesignerEvent = null;

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }

        public DesignerControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _refreshDesignerEvent = _eventAggregator.GetEvent<RefreshDesignerEvent>();
            _refreshDesignerEvent.Subscribe(OnRefreshDesigner, ThreadOption.UIThread, false, tab => tab.Guid == _fileGuid);

            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
        }

        private object _element;
        public object Element
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
        }

        private void OnRefreshDesigner(TabInfo tabInfo)
        {
            if (tabInfo.Guid != _fileGuid)
                return;

            _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(new ProcessInfo { status = ProcessStatus.Compile, Guid = _fileGuid });

            try
            {
                Element = XamlReader.Parse(tabInfo.FileContent) as FrameworkElement;
            }
            catch (Exception ex)
            {
                Element = new TextBlock
                {
                    Text = "Error: " + ex.Message,
                    Margin = new Thickness(5),
                    FontSize = 14,
                    FontWeight = FontWeights.Medium,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
            }
            finally
            {
                _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(new ProcessInfo { status = ProcessStatus.FinishCompile, Guid = _fileGuid });
            }
        }

        public void Dispose()
        {
            _refreshDesignerEvent.Unsubscribe(OnRefreshDesigner);
        }
    }
}
