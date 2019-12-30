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

            _eventAggregator.GetEvent<RefreshDesignerEvent>().Subscribe(OnRefreshDesigner);
        }

        private object _element;
        public object Element
        {
            get { return _element; }
            set { SetProperty(ref _element, value); }
        }

        private void OnRefreshDesigner(string content)
        {
            _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(ProcessStatus.Compile);

            try
            {
                Element = XamlReader.Parse(content) as FrameworkElement;
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
                _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(ProcessStatus.FinishCompile);
            }
        }
    }
}
