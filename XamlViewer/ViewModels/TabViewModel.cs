using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Utils.IO;
using XamlService;
using XamlService.Commands;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Models;
using XamlViewer.Utils;
using XamlViewer.Views;

namespace XamlViewer.ViewModels
{
    public class TabViewModel : BindableBase
    {
        private IContainerExtension _container = null;
        private AppData _appData = null;
        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;
        private IDialogService _dialogService = null;
        private IRegionManager _regionManager = null;

        private readonly string _guid = null;
        private bool _closeAfterSaving = false;
        private Action<TabViewModel, bool> _closeAction = null;
        private WorkControl _workControl = null; 

        public DelegateCommand<bool?> CloseCommand { get; private set; }
        public DelegateCommand CloseAllCommand { get; private set; }
        public DelegateCommand CloseAllButThisCommand { get; private set; }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand<bool?> CopyOrOpenPathCommand { get; private set; }

        private RequestSettingEvent _requestSettingEvent = null;
        private TextChangedEvent _textChangedEvent = null;
        private RequestTextEvent _requestTextEvent = null;
        private SaveTextEvent _saveTextEvent = null;

        public TabViewModel(string fileName, TabStatus status, Action<TabViewModel, bool> closeAction)
        {
            FileName = fileName;
            _closeAction = closeAction;
            _guid = Guid.NewGuid().ToString();

            _container = ServiceLocator.Current.GetInstance<IContainerExtension>();
            if (_container != null)
            {
                _appData = _container.Resolve<AppData>();
                _eventAggregator = _container.Resolve<IEventAggregator>();
                _appCommands = _container.Resolve<IApplicationCommands>();
                _dialogService = _container.Resolve<IDialogService>();
                _regionManager = _container.Resolve<IRegionManager>();
            }

            InitEvent();
            InitCommand();

            InitInfo(status);
        }

        public TabViewModel(string fileName, Action<TabViewModel, bool> closeAction)
            : this(fileName, TabStatus.None, closeAction)
        {
        }

        #region Init

        private void InitEvent()
        {
            if (_eventAggregator == null)
                return;

            _requestSettingEvent = _eventAggregator.GetEvent<RequestSettingEvent>();
            _requestSettingEvent.Subscribe(OnRequestSetting);

            _textChangedEvent = _eventAggregator.GetEvent<TextChangedEvent>();
            _textChangedEvent.Subscribe(OnTextChanged, ThreadOption.UIThread);

            _requestTextEvent = _eventAggregator.GetEvent<RequestTextEvent>();
            _requestTextEvent.Subscribe(OnRequestText);

            _saveTextEvent = _eventAggregator.GetEvent<SaveTextEvent>();
            _saveTextEvent.Subscribe(OnSaveText, ThreadOption.UIThread);
        }

        private void InitCommand()
        { 
            CloseCommand = new DelegateCommand<bool?>(Close);
            _appCommands.CloseAllCommand.RegisterCommand(CloseCommand);

            CloseAllCommand = new DelegateCommand(CloseAll);
            CloseAllButThisCommand = new DelegateCommand(CloseAllButThis);

            SaveCommand = new DelegateCommand(Save, CanSave);
            CopyOrOpenPathCommand = new DelegateCommand<bool?>(CopyOrOpenPath, CanCopyOrOpenPath);
        }

        #endregion

        #region Property

        public string MD5Code { get; set; }
        public string FileContent { get; set; }

        public string FileGuid { get { return _guid; } }
        public bool IsReadOnly { get { return (Status & TabStatus.Locked) == TabStatus.Locked; } }

        private string _fileName = null;
        public string FileName
        {
            get { return _fileName; }
            private set { SetProperty(ref _fileName, value); }
        }

        private string _title = null;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);

                if (_workControl != null)
                {
                    _workControl.Visibility = _isSelected ? Visibility.Visible : Visibility.Hidden;
                    UpdateSelectInfo();
                }
                else
                {
                    if (_isSelected)
                        InitWorkArea();
                }
            }
        } 

        private TabStatus _status = TabStatus.None;
        public TabStatus Status
        {
            get { return _status; }
            set
            {
                SetProperty(ref _status, value);

                UpdateStatusToEditor();
            }
        }

        #endregion

        #region Command 

        private void CloseAll()
        {
            _appCommands.CloseAllCommand.Execute(null);
        }

        private void CloseAllButThis()
        {
            _appCommands.CloseAllCommand.Execute(true);
        }

        private void Close(bool? ignoreSelected)
        {
            if (ignoreSelected.HasValue && ignoreSelected.Value && IsSelected)
                return;

            if ((Status & TabStatus.NoSave) == TabStatus.NoSave)
            {
                var isContinue = true;
                _dialogService.ShowMessage(string.Format("Save file \"{0}\"?", FileName), MessageButton.YesNo, MessageType.Question, r =>
                {
                    if (r.Result != ButtonResult.Yes)
                    {
                        if (_closeAction != null)
                            _closeAction(this, true);

                        Dispose();
                        isContinue = false;
                    }
                });

                if (!isContinue)
                    return;

                if (!File.Exists(FileName))
                {
                    var fileName = Common.ShowSaveFileDialog(FileName);
                    if (string.IsNullOrEmpty(fileName))
                        return;

                    UpdateFileName(fileName);
                }

                //this--->Editor(text)--->this(Save)
                _closeAfterSaving = true;
                _appCommands.SaveCommand.Execute(_guid);
            }
            else
            {
                if (_closeAction != null)
                    _closeAction(this, false);

                Dispose();
            }
        }

        private bool CanSave()
        {
            return (Status & TabStatus.NoSave) == TabStatus.NoSave && (Status & TabStatus.Inner) != TabStatus.Inner;
        }

        public void Save()
        {
            if (!File.Exists(FileName))
            {
                var fileName = Common.ShowSaveFileDialog(FileName);
                if (string.IsNullOrEmpty(fileName))
                    return;

                UpdateFileName(fileName);
            }

            //this--->Editor(text)--->this(Save)
            _appCommands.SaveCommand.Execute(_guid);
        }

        private bool CanCopyOrOpenPath(bool? isOpen)
        {
            return File.Exists(FileName);
        }

        private void CopyOrOpenPath(bool? isOpen)
        {
            if (isOpen.HasValue && isOpen.Value)
            {
                Process.Start(new ProcessStartInfo("Explorer.exe")
                {
                    Arguments = "/e,/select," + "\"" + FileName + "\""
                });

                return;
            }

            Clipboard.SetText(FileName);
        }

        #endregion

        #region Event

        private void OnRequestSetting(string guid)
        {
            if (_guid != guid)
                return;

            _eventAggregator.GetEvent<SettingChangedEvent>().Publish(Common.GetCurrentSettings(_appData.Config, _guid));
        }

        private void OnTextChanged(EditorInfo info)
        {
            if (info.Guid != _guid)
                return;

            if (info.IsModified || !File.Exists(FileName))
                Status |= TabStatus.NoSave;
            else
                Status &= ~(TabStatus.NoSave);
                
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void OnRequestText(TabInfo tabInfo)
        {
            if (tabInfo.Guid != _guid)
                return;

            _eventAggregator.GetEvent<LoadTextEvent>().Publish(new TabInfo { Guid = _guid, FileContent = FileContent, IsReadOnly = IsReadOnly });
        }

        private void OnSaveText(TabInfo tabInfo)
        {
            if (tabInfo.Guid != _guid || !CanSave())
                return; ;

            //Note:Do not call File.Exists(FileName), It is possible that FileName has not been created.
            if (!string.Equals(Path.GetFullPath(FileName), FileName, StringComparison.OrdinalIgnoreCase))
            {
                var fileName = Common.ShowSaveFileDialog(FileName);
                if (string.IsNullOrEmpty(fileName))
                    return;

                UpdateFileName(fileName);
            }

            FileContent = tabInfo.FileContent;
            SaveToFile();

            if (_closeAfterSaving)
            {
                _closeAfterSaving = false;

                if (_closeAction != null)
                    _closeAction(this, true);

                Dispose();
            }
        }

        #endregion

        #region Func

        public void InitWorkArea()
        {
            if (_workControl != null)
                return;

            if (!_regionManager.Regions.ContainsRegionWithName(RegionNames.WorkName))
                return;

            _workControl = _container.Resolve<WorkControl>(new ValueTuple<Type, object>(typeof(string), _guid), new ValueTuple<Type, object>(typeof(bool), IsSelected), new ValueTuple<Type, object>(typeof(bool), IsReadOnly));
            _workControl.Visibility = IsSelected ? Visibility.Visible : Visibility.Hidden;

            _regionManager.Regions[RegionNames.WorkName].Add(_workControl, null, true);
        }

        public void SaveToFile()
        {
            using (var fs = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(FileContent);
                }
            }

            MD5Code = FileHelper.ComputeMD5(FileName);
            Status &= ~(TabStatus.NoSave);
            
            SaveCommand.RaiseCanExecuteChanged();
        }

        private void UpdateStatusToEditor()
        {
            if (_eventAggregator == null)
                return;

            _eventAggregator.GetEvent<UpdateTabStatusEvent>().Publish(new TabFlag { IsReadOnly = IsReadOnly });
        }

        public void UpdateTextToEditor()
        {
            if (_eventAggregator == null)
                return;

            _eventAggregator.GetEvent<LoadTextEvent>().Publish(new TabInfo { Guid = _guid, FileContent = FileContent, IsReadOnly = IsReadOnly });
        }

        public void UpdateSelectInfo()
        {
            if (_eventAggregator == null)
                return;

            _eventAggregator.GetEvent<SelectTabEvent>().Publish(new TabSelectInfo { Guid = _guid, IsSelected = IsSelected });
        }

        public void UpdateFileName(string fileName)
        {
            Title = Path.GetFileName(fileName);
            FileName = fileName;

            CopyOrOpenPathCommand.RaiseCanExecuteChanged();
        }

        private void InitInfo(TabStatus status)
        {
            Status = status;

            if (File.Exists(FileName))
            {
                Title = Path.GetFileName(FileName);

                using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    MD5Code = FileHelper.ComputeMD5(fs);

                    fs.Position = 0;
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        FileContent = sr.ReadToEnd();
                    }
                }
            }
            else
            {
                Title = FileName;
                FileContent = (status & TabStatus.Inner) == TabStatus.Inner ? Application.Current.Resources["HelpContentTemplate"] as string : Application.Current.Resources["FileContentTemplate"] as string;
            } 

            CopyOrOpenPathCommand.RaiseCanExecuteChanged(); 
        }

        #endregion

        #region IDisposable

        private void UnsubscribeEventAndCommands()
        {
            _requestSettingEvent.Unsubscribe(OnRequestSetting);
            _textChangedEvent.Unsubscribe(OnTextChanged);
            _requestTextEvent.Unsubscribe(OnRequestText);
            _saveTextEvent.Unsubscribe(OnSaveText);

            _appCommands.CloseAllCommand.UnregisterCommand(CloseCommand);
        }

        private void Dispose()
        {
            UnsubscribeEventAndCommands();

            if(_workControl!=null)
            {
                _regionManager.Regions[RegionNames.WorkName].Remove(_workControl);
                _workControl = null;
            }
        }

        #endregion
    }
}
