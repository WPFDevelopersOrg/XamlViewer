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

        public DelegateCommand CloseCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        public TabViewModel(string fileName)
        {
            _xamlConfig = ServiceLocator.Current.GetInstance<XamlConfig>();
            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            _appCommands = ServiceLocator.Current.GetInstance<IApplicationCommands>();

            _eventAggregator.GetEvent<TextChangedEvent>().Subscribe(OnTextChanged);
            _eventAggregator.GetEvent<SaveTextEvent>().Subscribe(OnSaveText);

            CloseCommand = new DelegateCommand(Close, CanClose);
            SaveCommand = new DelegateCommand(Save, CanSave);

            FileName = fileName;
        }
         
        public string FileContent { get; set; }
        
        private string _fileName = null;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;

                if (File.Exists(_fileName))
                {
                    Title = Path.GetFileName(_fileName);

                    using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (var sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            FileContent = sr.ReadToEnd();
                            ReloadToEditor();
                        }
                    }
                }
                else
                    Title = _fileName;

                RaisePropertyChanged();
            }
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

            _eventAggregator.GetEvent<ReloadTextEvent>().Publish(FileContent);
        }

        private void UpdateFileName(string fileName)
        {
            Title = File.Exists(fileName) ? Path.GetFileName(fileName) : fileName;

            _fileName = fileName;
            RaisePropertyChanged(nameof(FileName));
        }  

        #region Command

        private bool CanClose()
        {
            return (Status & TabStatus.Locked) != TabStatus.Locked;
        }

        private void Close()
        {
            if (!File.Exists(FileName))
            {
                var r = MessageBox.Show("Save to file?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                {
                    var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml" };
                    if (sfd.ShowDialog() != SWF.DialogResult.OK)
                        return;

                    UpdateFileName(sfd.FileName); 

                    //this--->Editor(text)--->this(Save)
                    _appCommands.SaveCommand.Execute(null);
                }
            }
            else
            {
                if ((Status & TabStatus.NoSave) == TabStatus.NoSave)
                {
                    var r = MessageBox.Show("Save?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.Yes)
                    {
                        //this--->Editor(text)--->this(Save)
                        _appCommands.SaveCommand.Execute(null);
                    }
                }
            }

            //TBD???????????????????????????????????
            //1.Close after saving...
            //2.Close the unselected tab...
            _eventAggregator.GetEvent<CloseTabEvent>().Publish(FileName);
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

            Status |= TabStatus.NoSave;
        }

        private void OnSaveText(string text)
        {
            if (!IsSelected)
                return; ;

            if (!File.Exists(FileName))
            {
                var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml" };
                if (sfd.ShowDialog() != SWF.DialogResult.OK)
                    return;

                UpdateFileName(sfd.FileName);
            }

            using (var fs = new FileStream(FileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.Write(text);
                }
            }

            FileContent = text;
            Status &= ~(TabStatus.NoSave);
        }

        #endregion
    }
}
