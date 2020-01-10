using Prism.Mvvm;
using System;
using System.Windows.Controls;
using System.Windows.Markup;
using Prism.Events;
using XamlService.Events;
using System.Windows;
using XamlService.Payloads;

namespace XamlDesigner.ViewModels
{
    public class DesignerControlViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator = null;

        public DesignerControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<RefreshDesignerEvent>().Subscribe(OnRefreshDesigner, ThreadOption.PublisherThread, false, tab => tab.Guid == FileGuid);
        }

        private string _fileGuid = null;
        public string FileGuid
        {
            get { return _fileGuid; }
            set { SetProperty(ref _fileGuid, value); }
        }

        private object _element;
        public object Element
        {
            get { return _element; }
            set { SetProperty(ref _element, value); }
        }

        private void OnRefreshDesigner(TabInfo tabInfo)
        {
            if (tabInfo.Guid != FileGuid)
                return;

            _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(new ProcessInfo { status = ProcessStatus.Compile, Guid = FileGuid });

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
                _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(new ProcessInfo { status = ProcessStatus.FinishCompile, Guid = FileGuid });
            }
        }
    }
}
