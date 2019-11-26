using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace XamlViewer.ViewModels
{
    public class TabViewModel : BindableBase
    {
        public TabViewModel(string fileName)
        {
            FileName = fileName;

            Title = Path.IsPathRooted(FileName) ? Path.GetFileName(FileName) : FileName;
        }

        public string FileName { get; set; }

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
    }
}
