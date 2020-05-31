using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using XamlService.Events;
using XamlService.Payloads;
using XamlUtil.Common;
using XamlUtil.Net;
using XamlViewer.Models;

namespace XamlViewer.ViewModels
{
    public class StatusViewModel : BindableBase
    {
        private AppData _appData = null;
        private IEventAggregator _eventAggregator = null;

        private HashSet<ProcessStatus> _processStatuses = null;
        private readonly object _statusLock = new object();

        public DelegateCommand DownloadCommand { get; private set; }

        public StatusViewModel(IContainerExtension container, IEventAggregator eventAggregator)
        {
            _appData = container.Resolve<AppData>();
            _eventAggregator = eventAggregator;

            DownloadCommand = new DelegateCommand(Download);

            _processStatuses = new HashSet<ProcessStatus>();

            _eventAggregator.GetEvent<ProcessStatusEvent>().Subscribe(OnProcessStatus, ThreadOption.BackgroundThread);
            _eventAggregator.GetEvent<CaretPositionEvent>().Subscribe(OnCaretPosition, ThreadOption.BackgroundThread);


            Task.Run(() =>
            {
                try
                {
                    var urlString = @"https://api.github.com/repos/huangjia2107/xamlviewer/releases/latest";
#if NETFRAMEWORK
                    var reponseStr = HttpUtil.GetString(urlString).Result;
#else
                    var reponseStr = HttpUtil.Instance.GetString(urlString).Result;
#endif
                    if (!string.IsNullOrEmpty(reponseStr))
                        ReleaseVersion = JsonUtil.DeserializeObject<ReleaseInfo>(reponseStr);
                }
                catch(Exception ex) 
                {
                    Trace.TraceError(Common.GetExceptionStringFormat(ex));
                }
            });
        }

        private ReleaseInfo _releaseVersion;
        public ReleaseInfo ReleaseVersion
        {
            get { return _releaseVersion; }
            set { SetProperty(ref _releaseVersion, value); }
        }

        public string CurrentVersion
        {
            get { return _appData.Version; } 
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

        private void Download()
        {
            Process.Start(new ProcessStartInfo(@"https://github.com/huangjia2107/XamlViewer/releases"));
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
