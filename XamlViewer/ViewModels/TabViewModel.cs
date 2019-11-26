using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using XamlService.Events;
using XamlViewer.Models;

namespace XamlViewer.ViewModels
{
    public class TabViewModel : BindableBase
    {
        private IEventAggregator _eventAggregator = null;

        public DelegateCommand CloseCommand { get; private set; }

        public TabViewModel(string fileName)
        {
            CloseCommand = new DelegateCommand(Close);

            _eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

            FileName = fileName;
            Title = Path.IsPathRooted(FileName) ? Path.GetFileName(FileName) : FileName;
        }

        private string _fileName = null;
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
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
            set { SetProperty(ref _isSelected, value); }
        }

        private TabStatus _status = TabStatus.None;
        public TabStatus Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }

        private void Close()
        {
            if (!File.Exists(FileName))
            {
                var r = MessageBox.Show("Save to file?","",MessageBoxButton.YesNo,MessageBoxImage.Question);
                if(r == MessageBoxResult.Yes)
                {
                    //Create file...
                }
            }
            else
            {
                if ((Status & TabStatus.NoSave) == TabStatus.NoSave)
                {
                    var r = MessageBox.Show("Save?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.Yes)
                    {
                        //Save file...
                    }
                }
            }

            _eventAggregator.GetEvent<CloseTabEvent>().Publish(FileName);
        }
    }
}
