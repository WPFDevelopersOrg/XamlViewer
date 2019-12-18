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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XamlService;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Models;
using XamlViewer.Utils;
using SWF = System.Windows.Forms;
using System.Windows.Controls.Primitives;

namespace XamlViewer.ViewModels
{
    public class SettingViewModel : BindableBase
    {
        private AppData _appData = null; 
        private IEventAggregator _eventAggregator = null;

        public DelegateCommand AddRefCommand { get; private set; }
        public DelegateCommand RemoveRefCommand { get; private set; }
        public DelegateCommand RefSelectionChangedCommand { get; private set; }
		
		public DelegateCommand<ScrollViewer> ScrollToLeftCommand { get; private set; }
		public DelegateCommand<ScrollViewer> ScrollToRightCommand { get; private set; }
		public DelegateCommand<MouseWheelEventArgs> MouseWheelCommand { get; private set; }

        public SettingViewModel(IContainerExtension container, IEventAggregator eventAggregator)
        {
            _appData = container.Resolve<AppData>();
            _eventAggregator = eventAggregator;

            AddRefCommand = new DelegateCommand(AddReference);
            RemoveRefCommand = new DelegateCommand(RemoveReference, CanRemoveReference);
            RefSelectionChangedCommand = new DelegateCommand(RefSelectionChanged);
			
			ScrollToLeftCommand = new DelegateCommand<ScrollViewer>(ScrollToLeft);
			ScrollToRightCommand = new DelegateCommand<ScrollViewer>(ScrollToRight);
			MouseWheelCommand = new  DelegateCommand<MouseWheelEventArgs>(MouseWheel);

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
                foreach(var selectedName in ofd.FileNames)
                {
					var fileName = Path.GetFileName(selectedName);
					
                    var reference = References.FirstOrDefault(r => string.Equals(r.FileName, fileName, StringComparison.OrdinalIgnoreCase));
                    if (reference != null)
                    {
                        var r = MessageBox.Show("Same File, Replace?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r == MessageBoxResult.No)
                            return;

                        References.Remove(reference);
                    } 

                    File.Copy(ofd.FileName, AppDomain.CurrentDomain.BaseDirectory + fileName);

                    References.Add(new ReferenceViewModel(fileName));
                    _appData.Config.References.Add(fileName);
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
                    _appData.Config.References.RemoveAll(rf => rf == r.FileName);

                    var filePath = AppDomain.CurrentDomain.BaseDirectory + r.FileName;
                    if (File.Exists(filePath))
                        File.Delete(filePath);

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

        #region Scroll
		
		private void ScrollToLeft(ScrollViewer sv)
		{
            sv.ScrollToHorizontalOffset(Math.Max(0, sv.HorizontalOffset - 15));
        }
		
		private void ScrollToRight(ScrollViewer sv)
		{
            sv.ScrollToHorizontalOffset(Math.Min(sv.ScrollableWidth, sv.HorizontalOffset + 15));
		}
		
		private void MouseWheel(MouseWheelEventArgs e)
		{
            if (e.Source is Selector)
                return;

			var sv = e.Source as ScrollViewer;
            if (sv == null)
            {
                sv = Common.FindLogicParent<ScrollViewer>(e.Source as DependencyObject);
                if(sv == null)
                    return;
            }
			
			if (e.Delta > 0)
                sv.ScrollToHorizontalOffset(Math.Max(0, sv.HorizontalOffset - e.Delta));
            else
                sv.ScrollToHorizontalOffset(Math.Min(sv.ScrollableWidth, sv.HorizontalOffset - e.Delta));

            e.Handled = true;
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
