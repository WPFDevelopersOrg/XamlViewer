using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Utils.IO;
using XamlService.Commands;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Models;
using XamlViewer.Utils;
using SWF = System.Windows.Forms;

namespace XamlViewer.ViewModels
{
    public class TabViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;
        private IDialogService _dialogService = null;

        private readonly string _guid = null;
        private bool _closeAfterSaving = false;
        private Action<TabViewModel, bool> _closeAction = null;

        public DelegateCommand<bool?> CloseCommand { get; private set; }
        public DelegateCommand CloseAllCommand { get; private set; }
        public DelegateCommand CloseAllButThisCommand { get; private set; }

        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand<bool?> CopyOrOpenPathCommand { get; private set; }

        private TextChangedEvent _textChangedEvent = null;
        private RequestTextEvent _requestTextEvent = null;
        private SaveTextEvent _saveTextEvent = null;
        private CacheTextEvent _cacheTextEvent = null;

        public TabViewModel(string fileName, TabStatus status, Action<TabViewModel, bool> closeAction)
        {
            FileName = fileName;
            _closeAction = closeAction;
            _guid = Guid.NewGuid().ToString();

            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _appCommands = ServiceLocator.Current.GetInstance<IApplicationCommands>();
            _dialogService = ServiceLocator.Current.GetInstance<IDialogService>();

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

            _textChangedEvent = _eventAggregator.GetEvent<TextChangedEvent>();
            _textChangedEvent.Subscribe(OnTextChanged);

            _requestTextEvent = _eventAggregator.GetEvent<RequestTextEvent>();
            _requestTextEvent.Subscribe(OnRequestText);

            _saveTextEvent = _eventAggregator.GetEvent<SaveTextEvent>();
            _saveTextEvent.Subscribe(OnSaveText);

            _cacheTextEvent = _eventAggregator.GetEvent<CacheTextEvent>();
            _cacheTextEvent.Subscribe(OnCacheText);
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

        public string MD5Code { get; set; }
        public string FileContent { get; set; }

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

                UpdateTextToEditor();
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

        public void UpdateStatusToEditor()
        {
            if (!IsSelected || _eventAggregator == null)
                return;

            _eventAggregator.GetEvent<UpdateTabStatusEvent>().Publish(new TabFlag { IsReadOnly = ((Status & TabStatus.Locked) == TabStatus.Locked) });
        }

        public void UpdateTextToEditor()
        {
            if (!IsSelected || _eventAggregator == null)
                return;

            _eventAggregator.GetEvent<LoadTextEvent>().Publish(new TabInfo { Guid = _guid, FileContent = FileContent, IsReadOnly = ((Status & TabStatus.Locked) == TabStatus.Locked) });
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

        public void UpdateFileName(string fileName)
        {
            Title = Path.GetFileName(fileName);
            FileName = fileName;

            CopyOrOpenPathCommand.RaiseCanExecuteChanged();
        }

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

            if (IsSelected && (Status & TabStatus.NoSave) == TabStatus.NoSave)
            {
                var isContinue = true;
                _dialogService.ShowMessage(string.Format("Save file \"{0}\"?", FileName), MessageButton.YesNo, MessageType.Question, r =>
                {
                    if (r.Result != ButtonResult.Yes)
                    {
                        if (_closeAction != null)
                            _closeAction(this, true);

                        UnsubscribeEvents();
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
                _appCommands.SaveCommand.Execute(null);
            }
            else
            {
                if (_closeAction != null)
                    _closeAction(this, false);

                UnsubscribeEvents();
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
            _appCommands.SaveCommand.Execute(null);
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

        private void OnTextChanged(EditorInfo info)
        {
            if (!IsSelected)
                return; ;

            if (info.IsModified || !File.Exists(FileName))
                Status |= TabStatus.NoSave;
            else
                Status &= ~(TabStatus.NoSave);
        }

        private void OnRequestText(TabInfo tabInfo)
        {
            if (tabInfo == null)
                return;

            if (!string.IsNullOrEmpty(tabInfo.Guid) && tabInfo.Guid == _guid || IsSelected)
                _eventAggregator.GetEvent<LoadTextEvent>().Publish(new TabInfo { Guid = _guid, FileContent = FileContent, IsReadOnly = ((Status & TabStatus.Locked) == TabStatus.Locked) });
        }

        private void OnSaveText(TabInfo tabInfo)
        {
            if (tabInfo.Guid != _guid || !CanSave())
                return; ;

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

                UnsubscribeEvents();
            }
        }

        private void OnCacheText(TabInfo tabInfo)
        {
            if (tabInfo == null || tabInfo.Guid != _guid)
                return;

            FileContent = tabInfo.FileContent;
        }

        #endregion

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
        }

        private void UnsubscribeEvents()
        {
            _textChangedEvent.Unsubscribe(OnTextChanged);
            _requestTextEvent.Unsubscribe(OnRequestText);
            _saveTextEvent.Unsubscribe(OnSaveText);
            _cacheTextEvent.Unsubscribe(OnCacheText);

            _appCommands.CloseAllCommand.UnregisterCommand(CloseCommand);
        }
    }
}
