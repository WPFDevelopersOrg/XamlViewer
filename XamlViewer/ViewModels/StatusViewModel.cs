using Prism.Events;
using Prism.Mvvm;
using System;
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
        private readonly object _statusLock = new object();

        public StatusViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _processStatuses = new HashSet<ProcessStatus>();

            _eventAggregator.GetEvent<ProcessStatusEvent>().Subscribe(OnProcessStatus, ThreadOption.BackgroundThread);
            _eventAggregator.GetEvent<CaretPositionEvent>().Subscribe(OnCaretPosition, ThreadOption.BackgroundThread);
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

        private void OnProcessStatus(ProcessInfo info)
        {
            switch (info.status)
            {
                case ProcessStatus.FinishCompile:
                    LockAction(() => _processStatuses.Remove(ProcessStatus.Compile));
                    break;

                case ProcessStatus.FinishSave:
                    LockAction(() => _processStatuses.Remove(ProcessStatus.Save));
                    break;

                case ProcessStatus.FinishLoadFonts:
                    LockAction(() => _processStatuses.Remove(ProcessStatus.LoadFonts));
                    break;

                default:
                    LockAction(() => _processStatuses.Add(info.status));
                    break;
            }

            LockAction(() =>
            {
                if (_processStatuses.Count == 0)
                    CurrentStatus = "Ready";
                else
                    CurrentStatus = ResourcesMap.ProcessStatusDic[_processStatuses.Min()];
            });
        }

        private void OnCaretPosition(CaretPosition pos)
        {
            CaretLine = pos.Line;
            CaretColumn = pos.Column;
        }

        private void LockAction(Action action)
        {
            if (action == null)
                return;

            lock(_statusLock)
            {
                action();
            }
        }
    }
}
