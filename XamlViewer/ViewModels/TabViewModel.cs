using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Utils.IO;
using XamlService.Commands;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Models;
using SWF = System.Windows.Forms;

namespace XamlViewer.ViewModels
{
    public class TabViewModel : BindableBase
    { 
        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;

        private bool _closeAfterSaving = false;
        private Action<TabViewModel, bool> _closeAction = null;

        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        private TextChangedEvent _textChangedEvent = null;
        private RequestTextEvent _requestTextEvent = null;
        private SaveTextEvent _saveTextEvent = null;
        private CacheTextEvent _cacheTextEvent = null;

        public TabViewModel(string fileName, Action<TabViewModel, bool> closeAction)
        {
            FileName = fileName;
            _closeAction = closeAction;
             
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _appCommands = ServiceLocator.Current.GetInstance<IApplicationCommands>();

            InitEvent();
            InitCommand();

            InitInfo();
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
            CloseCommand = new DelegateCommand(Close);
            SaveCommand = new DelegateCommand(Save, CanSave);
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

            _eventAggregator.GetEvent<LoadTextEvent>().Publish(new TabInfo { FileName = FileName, FileContent = FileContent, IsReadOnly = ((Status & TabStatus.Locked) == TabStatus.Locked) });
        }

        private void InitInfo()
        {
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
                FileContent = Application.Current.Resources["FileContentTemplate"] as string;
            }
        }

        public void UpdateFileName(string fileName)
        {
            Title = Path.GetFileName(fileName);
            FileName = fileName;
        }

        #region Command 

        private void Close()
        {
            if (IsSelected && (Status & TabStatus.NoSave) == TabStatus.NoSave)
            {
                if (!File.Exists(FileName))
                {
                    var r = MessageBox.Show("Save to file?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.No)
                    {
                        if (_closeAction != null)
                            _closeAction(this, true);

                        return;
                    }

                    var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml", FileName = Path.GetFileNameWithoutExtension(FileName) };
                    if (sfd.ShowDialog() != SWF.DialogResult.OK)
                        return;

                    UpdateFileName(sfd.FileName);

                    ////this--->Editor(text)--->this(Save)
                    _closeAfterSaving = true;
                    _appCommands.SaveCommand.Execute(null);
                }
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
            return (Status & TabStatus.NoSave) == TabStatus.NoSave;
        }

        public void Save()
        {
            if (!File.Exists(FileName))
            {
                var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml", FileName = Path.GetFileNameWithoutExtension(FileName) };
                if (sfd.ShowDialog() != SWF.DialogResult.OK)
                    return;

                UpdateFileName(sfd.FileName);
            }

            //this--->Editor(text)--->this(Save)
            _appCommands.SaveCommand.Execute(null);
        }

        #endregion

        #region Event

        private void OnTextChanged(EditorInfo info)
        {
            if (!IsSelected)
                return; ;

            if (info.IsModified)
                Status |= TabStatus.NoSave;
            else
                Status &= ~(TabStatus.NoSave);
        }

        private void OnRequestText(TabInfo tabInfo)
        {
            if (tabInfo == null)
                return;

            if (!string.IsNullOrEmpty(tabInfo.FileName) && tabInfo.FileName == FileName || IsSelected)
                _eventAggregator.GetEvent<LoadTextEvent>().Publish(new TabInfo { FileName = FileName, FileContent = FileContent, IsReadOnly = ((Status & TabStatus.Locked) == TabStatus.Locked) });
        }

        private void OnSaveText(TabInfo tabInfo)
        {
            if (tabInfo.FileName != FileName)
                return; ;

            if (!File.Exists(FileName))
            {
                var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml", FileName = Path.GetFileNameWithoutExtension(FileName) };
                if (sfd.ShowDialog() != SWF.DialogResult.OK)
                    return;

                UpdateFileName(sfd.FileName);
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
            if (tabInfo == null || tabInfo.FileName != FileName)
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
        }
    }
}
