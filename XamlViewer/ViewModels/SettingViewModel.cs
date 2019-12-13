using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using XamlService;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Models;

namespace XamlViewer.ViewModels
{
    public class SettingViewModel : BindableBase
    {
        private AppData _appData = null; 
        private IEventAggregator _eventAggregator = null;

        public SettingViewModel(IContainerExtension container, IEventAggregator eventAggregator)
        {
            _appData = container.Resolve<AppData>();
            _eventAggregator = eventAggregator;

            LoadFonts();
            ApplyEditorConfig();
        }

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

        private Color _selectedColor = Colors.Black;
        public Color SelectedColor
        {
            get { return _selectedColor; }
            set { SetProperty(ref _selectedColor, value); }
        }

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
