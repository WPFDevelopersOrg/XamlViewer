using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XamlService;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Models;

namespace XamlViewer.ViewModels
{
    public class StatusViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator = null;
        private HashSet<ProcessStatus> _processStatuses = null;

        public StatusViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _processStatuses = new HashSet<ProcessStatus>();

            _eventAggregator.GetEvent<ProcessStatusEvent>().Subscribe(OnProcessStatusEvent);
        }

        private string _currentStatus = "Ready";
        public string CurrentStatus
        {
            get { return _currentStatus; }
            set { SetProperty(ref _currentStatus, value); }
        }

        private void OnProcessStatusEvent(ProcessStatus status)
        {
            switch (status)
            {
                case ProcessStatus.FinishCompile:
                    _processStatuses.Remove(ProcessStatus.Compile);
                    break;

                case ProcessStatus.FinishSave:
                    _processStatuses.Remove(ProcessStatus.Save);
                    break;

                case ProcessStatus.FinishLoadFonts:
                    _processStatuses.Remove(ProcessStatus.LoadFonts);
                    break;

                default:
                    _processStatuses.Add(status);
                    break;
            }

            if (_processStatuses.Count == 0)
                CurrentStatus = "Ready";
            else
                CurrentStatus = ResourcesMap.ProcessStatusDic[_processStatuses.Min()];
        }
    }
}
