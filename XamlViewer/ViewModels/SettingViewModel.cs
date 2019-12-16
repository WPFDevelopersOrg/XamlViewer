using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using XamlService;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Models;
using SWF = System.Windows.Forms;

namespace XamlViewer.ViewModels
{
    public class SettingViewModel : BindableBase
    {
        private AppData _appData = null; 
        private IEventAggregator _eventAggregator = null;

        public DelegateCommand AddRefCommand { get; private set; }
        public DelegateCommand RemoveRefCommand { get; private set; }
        public DelegateCommand RefSelectionChangedCommand { get; private set; }

        public SettingViewModel(IContainerExtension container, IEventAggregator eventAggregator)
        {
            _appData = container.Resolve<AppData>();
            _eventAggregator = eventAggregator;

            AddRefCommand = new DelegateCommand(AddReference);
            RemoveRefCommand = new DelegateCommand(RemoveReference, CanRemoveReference);
            RefSelectionChangedCommand = new DelegateCommand(RefSelectionChanged);

            References = new ObservableCollection<ReferenceViewModel>(_appData.Config.References.Select(r => new ReferenceViewModel(r)));

            LoadFonts();
            ApplyEditorConfig();
        }

        #region Editor & Designer

        private List<string> _fontFamilies = null;
        public List<string> FontFamilies
        {
            get { return _fontFamilies ?? new List<string> { _appData.Config.FontFamily }; }
            set { SetProperty(ref _fontFamilies, value); }
        }

        public string FontFamily
        {
            get { return _appData.Config.FontFamily; }
            set
            {
                _appData.Config.FontFamily = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public double FontSize
        {
            get { return _appData.Config.FontSize; }
            set
            {
                _appData.Config.FontSize = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public bool WordWrap
        {
            get { return _appData.Config.WordWrap; }
            set
            {
                _appData.Config.WordWrap = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public bool ShowLineNumber
        {
            get { return _appData.Config.ShowLineNumber; }
            set
            {
                _appData.Config.ShowLineNumber = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public bool AutoCompile
        {
            get { return _appData.Config.AutoCompile; }
            set
            {
                _appData.Config.AutoCompile = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public double AutoCompileDelay
        {
            get { return _appData.Config.AutoCompileDelay; }
            set
            {
                _appData.Config.AutoCompileDelay = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        #endregion

        #region Color

        private Color _selectedColor = Colors.Black;
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { SetProperty(ref _selectedColor, value); }
        }

        #endregion

        #region Reference

        public ObservableCollection<ReferenceViewModel> References { get; private set; }
         
        private void AddReference()
        {
            var ofd = new SWF.OpenFileDialog { Filter = "DLL|*.dll", Multiselect = true };
            if (ofd.ShowDialog() == SWF.DialogResult.OK)
            {
                foreach(var fileName in ofd.FileNames)
                {
                    var reference = References.FirstOrDefault(r => string.Equals(r.Name, Path.GetFileNameWithoutExtension(fileName), StringComparison.OrdinalIgnoreCase));
                    if (reference != null)
                    {
                        var r = MessageBox.Show("Same File, Replace?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r == MessageBoxResult.No)
                            return;

                        References.Remove(reference);
                    }

                    //Check Runtime Version and ...
                    //.......

                    var newFileName = AppDomain.CurrentDomain.BaseDirectory + Path.GetFileName(fileName);

                    //File.Copy(ofd.FileName, newFileName);
                    var refVm = new ReferenceViewModel(newFileName);

                    References.Add(refVm);
                    _appData.Config.References.Add(refVm.Name);
                }
                
                RemoveRefCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanRemoveReference()
        {
            return References != null && References.Count > 0 && References.Any(r => r.IsSelected);
        }

        private void RemoveReference()
        {
            for(int i = 0; i < References.Count; i++)
            {
                var r = References[i];
                if (r.IsSelected)
                {
                    References.RemoveAt(i);
                    _appData.Config.References.RemoveAll(rf => rf == r.Name);

                    if (File.Exists(r.FileName))
                        File.Delete(r.FileName);

                    i--;
                }
            }

            RemoveRefCommand.RaiseCanExecuteChanged();
        }

        private void RefSelectionChanged()
        {
            RemoveRefCommand.RaiseCanExecuteChanged();
        }

        #endregion

        private void LoadFonts()
        {
            _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(ProcessStatus.LoadFonts);

            Task.Run(() =>
            {
                FontFamilies = Fonts.SystemFontFamilies.Select(f => f.Source).OrderBy(f => f).ToList();
                _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(ProcessStatus.FinishLoadFonts);
            });
        }

        private void ApplyEditorConfig()
        {
            _eventAggregator.GetEvent<ConfigEvents>().Publish(new EditorConfig
            {
                FontFamily = FontFamily,
                FontSize = FontSize,

                WordWrap = WordWrap,
                ShowLineNumber = ShowLineNumber,

                AutoCompile = AutoCompile,
                AutoCompileDelay = AutoCompileDelay
            });
        }
    }
}
