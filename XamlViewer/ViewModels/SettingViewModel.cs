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
        }

        private List<FontFamily> _fontFamilies = new List<FontFamily> { new FontFamily("Calibri") };
        public List<FontFamily> FontFamilies
        {
            get { return _fontFamilies; }
            set { SetProperty(ref _fontFamilies, value); }
        }

        public FontFamily FontFamily
        {
            get { return new FontFamily(_xamlConfig.FontFamily); }
            set
            {
                _xamlConfig.FontFamily = value.Source;
                RaisePropertyChanged();
            }
        }

        public double FontSize
        {
            get { return _xamlConfig.FontSize; }
            set
            {
                _xamlConfig.FontSize = value;
                RaisePropertyChanged();
            }
        }

        public bool IsCodeCollapsing
        {
            get { return _xamlConfig.IsCodeCollapsing; }
            set
            {
                _xamlConfig.IsCodeCollapsing = value;
                RaisePropertyChanged();
            }
        }

        public bool ShowLineNumber
        {
            get { return _xamlConfig.ShowLineNumber; }
            set
            {
                _xamlConfig.ShowLineNumber = value;
                RaisePropertyChanged();
            }
        }

        public bool AutoCompile
        {
            get { return _xamlConfig.AutoCompile; }
            set
            {
                _xamlConfig.AutoCompile = value;
                RaisePropertyChanged();
            }
        }

        public double AutoCompileDelay
        {
            get { return _xamlConfig.AutoCompileDelay; }
            set
            {
                _xamlConfig.AutoCompileDelay = value;
                RaisePropertyChanged();
            }
        }

        private void LoadFonts()
        {
            _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(ProcessStatus.StartFonts);

            Task.Run(()=> 
            {
                var fontFamilies = new List<FontFamily>();
                foreach (FontFamily font in Fonts.SystemFontFamilies.OrderBy(f => f.Source))
                {
                    fontFamilies.Add(font);
                }

                FontFamilies = fontFamilies;

                _eventAggregator.GetEvent<ProcessStatusEvent>().Publish(ProcessStatus.EndFonts);
            });
            
        }
    }
}
