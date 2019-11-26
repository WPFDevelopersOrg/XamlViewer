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
using XamlViewer.Models;

namespace XamlViewer.ViewModels
{
    public class SettingViewModel : BindableBase
    {
        private XamlConfig _xamlConfig = null; 
        private IEventAggregator _eventAggregator = null;

        public SettingViewModel(IContainerExtension container, IEventAggregator eventAggregator)
        {
            _xamlConfig = container.Resolve<XamlConfig>();
            _eventAggregator = eventAggregator;

            LoadFonts();
            ApplyEditorConfig();
        }

        private List<string> _fontFamilies = null;
        public List<string> FontFamilies
        {
            get { return _fontFamilies ?? new List<string> { _xamlConfig.FontFamily }; }
            set { SetProperty(ref _fontFamilies, value); }
        }

        public string FontFamily
        {
            get { return _xamlConfig.FontFamily; }
            set
            {
                _xamlConfig.FontFamily = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public double FontSize
        {
            get { return _xamlConfig.FontSize; }
            set
            {
                _xamlConfig.FontSize = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public bool WordWrap
        {
            get { return _xamlConfig.WordWrap; }
            set
            {
                _xamlConfig.WordWrap = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public bool ShowLineNumber
        {
            get { return _xamlConfig.ShowLineNumber; }
            set
            {
                _xamlConfig.ShowLineNumber = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public bool AutoCompile
        {
            get { return _xamlConfig.AutoCompile; }
            set
            {
                _xamlConfig.AutoCompile = value;

                RaisePropertyChanged();
                ApplyEditorConfig(); 
            }
        }

        public double AutoCompileDelay
        {
            get { return _xamlConfig.AutoCompileDelay; }
            set
            {
                _xamlConfig.AutoCompileDelay = value;

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
            _eventAggregator.GetEvent<EditorConfigEvent>().Publish(new EditorConfig
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
