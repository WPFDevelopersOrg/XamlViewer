using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System.ComponentModel;
using System.Windows;
using Utils.IO;
using XamlViewer.Models;
using XamlService.Commands;
using System.Linq;
using System.IO;

namespace XamlViewer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private AppData _appData = null;

        public DelegateCommand ExpandOrCollapseCommand { get; private set; }
        public DelegateCommand ActivatedCommand { get; private set; }
        public DelegateCommand<DragEventArgs> DropCommand { get; private set; }
        public DelegateCommand<CancelEventArgs> ClosingCommand { get; private set; } 

        public MainViewModel(IContainerExtension container, IApplicationCommands appCommands)
        {
            _appData = container.Resolve<AppData>();
            AppCommands = appCommands;

            InitCommand();
        }

        #region Init

        private void InitCommand()
        {
            ExpandOrCollapseCommand = new DelegateCommand(ExpandOrCollapse);
            ActivatedCommand = new DelegateCommand(Activated);
            DropCommand = new DelegateCommand<DragEventArgs>(Drop);
            ClosingCommand = new DelegateCommand<CancelEventArgs>(Closing); 
        }

        #endregion

        #region Command

        private void ExpandOrCollapse()
        {
            ExpandOrCollapse(_isExpandSetting);
        }

        private void Activated()
        {
            if (_appCommands == null)
                return;

            _appCommands.RefreshCommand.Execute(null);
        }

        private void Drop(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            var xamlFiles = files.Where(f => Path.GetExtension(f).ToLower() == ".xaml").ToArray();

            if (xamlFiles != null && xamlFiles.Length > 0)
                _appCommands.DropCommand.Execute(xamlFiles);
        }

        private async void Closing(CancelEventArgs e)
        {
            if (_appData.DealExistedFileAction != null)
                await _appData.DealExistedFileAction();

            FileHelper.SaveToJsonFile(ResourcesMap.LocationDic[Location.GlobalConfigFile], _appData.Config);
        } 

        #endregion

        private IApplicationCommands _appCommands;
        public IApplicationCommands AppCommands
        {
            get { return _appCommands; }
            set { SetProperty(ref _appCommands, value); }
        }

        private string _title = "XAML Viewer";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private bool _isExpandSetting;
        public bool IsExpandSetting
        {
            get { return _isExpandSetting; }
            set { SetProperty(ref _isExpandSetting, value); }
        }

        private GridLength _settingRowHeight = new GridLength(0);
        public GridLength SettingRowHeight
        {
            get { return _settingRowHeight; }
            set { SetProperty(ref _settingRowHeight, value); }
        } 

        #region Func

        private void ExpandOrCollapse(bool isExpand)
        {
            SettingRowHeight = isExpand ? GridLength.Auto : new GridLength(0);
        }

        #endregion
    }
}
