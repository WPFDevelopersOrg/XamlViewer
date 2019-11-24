using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlService;
using XamlService.Events;

namespace XamlViewer.ViewModels
{
    public class StatusViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator = null;

        public StatusViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<ProcessStatusEvent>().Subscribe(OnProcessStatusEvent);
        }

        private void OnProcessStatusEvent(ProcessStatus status)
        {

        }
    }
}
