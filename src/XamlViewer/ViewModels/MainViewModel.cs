using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System.ComponentModel;
using System.Windows;
using XamlUtil.IO;
using XamlViewer.Models;
using XamlService.Commands;
using System.Linq;
using System.IO;
using Prism.Events;
using XamlService.Events;

namespace XamlViewer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private AppData _appData = null;
        private IEventAggregator _eventAggregator = null;

        private GridLength _lastDataSourceColumnWidth = new GridLength(1, GridUnitType.Star);

        public DelegateCommand ExpandOrCollapseCommand { get; private set; }
        public DelegateCommand ActivatedCommand { get; private set; }
        public DelegateCommand<DragEventArgs> DropCommand { get; private set; }
        public DelegateCommand<CancelEventArgs> ClosingCommand { get; private set; }

        public MainViewModel(IContainerExtension container, IApplicationCommands appCommands)
        {
            _appData = container.Resolve<AppData>();
            _eventAggregator = container.Resolve<IEventAggregator>();

            AppCommands = appCommands;

            InitEvent();
            InitCommand();
            InitStatus();
        }

        #region Init

        private void InitEvent()
        {
            _eventAggregator.GetEvent<OpenDataSourceEvent>().Subscribe(OnOpenDataSource);
        }

        private void InitCommand()
        {
            ExpandOrCollapseCommand = new DelegateCommand(ExpandOrCollapse);
            ActivatedCommand = new DelegateCommand(Activated);
            DropCommand = new DelegateCommand<DragEventArgs>(Drop);
            ClosingCommand = new DelegateCommand<CancelEventArgs>(Closing);
        }

        private void InitStatus()
        {
            if(_appData.Config.IsOpenDataSource)
                OnOpenDataSource(true);
        }

        #endregion

        #region Event

        private void OnOpenDataSource(bool isOpen)
        {
            if (isOpen)
            {
                DataSourceMinColumnWidth = 100d;
                DataSourceColumnWidth = _lastDataSourceColumnWidth;

                GridSplitterColumnWidth = GridLength.Auto;
            }
            else
            {
                //backup
                _lastDataSourceColumnWidth = DataSourceColumnWidth;

                DataSourceMinColumnWidth = 0d;
                DataSourceColumnWidth = new GridLength(0);

                GridSplitterColumnWidth = new GridLength(0);
            }
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
            await _appData.DealExistedFileAction?.Invoke();

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

        private GridLength _gridSplitterColumnWidth = new GridLength(0);
        public GridLength GridSplitterColumnWidth
        {
            get { return _gridSplitterColumnWidth; }
            set { SetProperty(ref _gridSplitterColumnWidth, value); }
        }

        private GridLength _dataSourceColumnWidth = new GridLength(0);
        public GridLength DataSourceColumnWidth
        {
            get { return _dataSourceColumnWidth; }
            set { SetProperty(ref _dataSourceColumnWidth, value); }
        }

        private double _dataSourceMinColumnWidth;
        public double DataSourceMinColumnWidth
        {
            get { return _dataSourceMinColumnWidth; }
            set { SetProperty(ref _dataSourceMinColumnWidth, value); }
        }

        #region Func

        private void ExpandOrCollapse(bool isExpand)
        {
            SettingRowHeight = isExpand ? GridLength.Auto : new GridLength(0);
        }

        #endregion
    }
}
