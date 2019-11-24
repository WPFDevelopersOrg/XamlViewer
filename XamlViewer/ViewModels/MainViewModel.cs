using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Utils.IO;
using XamlViewer.Models;

namespace XamlViewer.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private XamlConfig _xamlConfig = null;

        public DelegateCommand ExpandOrCollapseCommand { get; private set; }
        public DelegateCommand<CancelEventArgs> ClosingCommand { get; private set; }

        public MainViewModel(IContainerExtension container)
        {
            _xamlConfig = container.Resolve<XamlConfig>();

            ExpandOrCollapseCommand = new DelegateCommand(ExpandOrCollapse);
            ClosingCommand = new DelegateCommand<CancelEventArgs>(Closing);
        }

        private string _title = "Xaml Viewer";
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

        private GridLength _settingRowHeight=new GridLength(0);
        public GridLength SettingRowHeight
        {
            get { return _settingRowHeight; }
            set { SetProperty(ref _settingRowHeight, value); }
        }

        private void ExpandOrCollapse()
        {
            ExpandOrCollapse(_isExpandSetting);
        }

        private void ExpandOrCollapse(bool isExpand)
        {
            SettingRowHeight = isExpand ? GridLength.Auto : new GridLength(0);
        }

        private void Closing(CancelEventArgs e)
        {  
            FileHelper.SaveToJsonFile(ResourcesMap.LocationDic[Location.GlobalConfigFile], _xamlConfig);
        }
    }
}
