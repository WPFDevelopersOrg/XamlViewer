using System;
using System.IO;
using System.Text;
using System.Windows;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using XamlService.Commands;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Models;
using SWF = System.Windows.Forms;

namespace XamlViewer.ViewModels
{
    public class TabViewModel : BindableBase
    {
        private XamlConfig _xamlConfig = null;
        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;

        private bool _closeAfterSaving = false;

        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        public Action<TabViewModel,bool> CloseAction { get; set; }

        public TabViewModel(string fileName)
        {
            _xamlConfig = ServiceLocator.Current.GetInstance<XamlConfig>();
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _appCommands = ServiceLocator.Current.GetInstance<IApplicationCommands>();

            _eventAggregator.GetEvent<TextChangedEvent>().Subscribe(OnTextChanged);
            _eventAggregator.GetEvent<SaveTextEvent>().Subscribe(OnSaveText);
            _eventAggregator.GetEvent<CacheTextEvent>().Subscribe(OnCacheText);

            CloseCommand = new DelegateCommand(Close, CanClose);
            SaveCommand = new DelegateCommand(Save, CanSave);

            FileName = fileName;
            InitInfo();
        }
         
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

                //Update Designer and Editor

                ReloadToEditor();
            }
        }

        private TabStatus _status = TabStatus.None;
        public TabStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        private void ReloadToEditor()
        {
            if (!IsSelected || _eventAggregator == null)
                return;

            _eventAggregator.GetEvent<LoadTextEvent>().Publish(new TabInfo { FileName = FileName, FileContent = FileContent});
        }

        private void InitInfo()
        {
            if (File.Exists(FileName))
            {
                Title = Path.GetFileName(FileName);

                using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        FileContent = sr.ReadToEnd();
                    }
                }
            }
            else
                Title = FileName;
        }

        public void UpdateFileName(string fileName)
        {
            Title = File.Exists(fileName) ? Path.GetFileName(fileName) : fileName;
            SetProperty(ref _fileName, fileName, "FileName");
        }  

        #region Command

        private bool CanClose()
        {
            return (Status & TabStatus.Locked) != TabStatus.Locked;
        }

        private void Close()
        {
            if(IsSelected && (Status & TabStatus.NoSave) == TabStatus.NoSave)
            {
                if (!File.Exists(FileName))
                {
                    var r = MessageBox.Show("Save to file?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.No)
                    {
                        if (CloseAction != null)
                            CloseAction(this, true);

                        return;
                    }
                       
                    var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml" };
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
                if (CloseAction != null)
                    CloseAction(this, false);
            } 
        }

        private bool CanSave()
        {
            return (Status & TabStatus.NoSave) == TabStatus.NoSave;
        }

        private void Save()
        {
            if (!File.Exists(FileName))
            {
                var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml" };
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

            if(info.IsModified)
                Status |= TabStatus.NoSave;
            else
                Status &= ~(TabStatus.NoSave);
        }

        private void OnSaveText(TabInfo tabInfo)
        {
            if (tabInfo.FileName != FileName)
                return; ;

            if (!File.Exists(FileName))
            {
                var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml" };
                if (sfd.ShowDialog() != SWF.DialogResult.OK)
                    return;

                UpdateFileName(sfd.FileName);
            }

            FileContent = tabInfo.FileContent;
            SaveToFile();

            if(_closeAfterSaving)
            {
                _closeAfterSaving = false;

                if (CloseAction != null)
                    CloseAction(this, true);
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

            Status &= ~(TabStatus.NoSave);
        }
    }
}
