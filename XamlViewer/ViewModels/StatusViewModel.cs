using Prism.Events;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
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
            _eventAggregator.GetEvent<CaretPositionEvent>().Subscribe(OnCaretPositionEvent);
        }

        private string _currentStatus = "Ready";
        public string CurrentStatus
        {
            get { return _currentStatus; }
            set { SetProperty(ref _currentStatus, value); }
        }

        private int _caretLine = 0;
        public int CaretLine
        {
            get { return _caretLine; }
            set { SetProperty(ref _caretLine, value); }
        }

        private int _caretColumn = 0;
        public int CaretColumn
        {
            get { return _caretColumn; }
            set { SetProperty(ref _caretColumn, value); }
        }

        private void OnProcessStatusEvent(ProcessInfo info)
        {
            switch (info.status)
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
                    _processStatuses.Add(info.status);
                    break;
            }

            if (_processStatuses.Count == 0)
                CurrentStatus = "Ready";
            else
                CurrentStatus = ResourcesMap.ProcessStatusDic[_processStatuses.Min()];
        }

        private void OnCaretPositionEvent(CaretPosition pos)
        {
            CaretLine = pos.Line;
            CaretColumn = pos.Column;
        }
    }
}
