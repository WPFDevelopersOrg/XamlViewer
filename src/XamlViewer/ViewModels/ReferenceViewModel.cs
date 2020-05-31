using Prism.Mvvm;
using System.IO;

namespace XamlViewer.ViewModels
{
    public class ReferenceViewModel : BindableBase
    {
        public ReferenceViewModel(string fileName)
        { 
            FileName = Path.GetFileName(fileName);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set { SetProperty(ref _fileName, value); }
        } 
    }
}
