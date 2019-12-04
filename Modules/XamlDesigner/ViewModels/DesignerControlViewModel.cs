using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Element = XamlReader.Parse(content) as FrameworkElement;
        }
    }
}
