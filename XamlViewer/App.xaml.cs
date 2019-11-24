using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Utils.IO;
using XamlViewer.Models;
using XamlViewer.ViewModels;
using XamlViewer.Views;

namespace XamlViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.Register<MainWindow, MainViewModel>();
            ViewModelLocationProvider.Register<SettingControl, SettingViewModel>();
            ViewModelLocationProvider.Register<ToolbarControl, ToolbarViewModel>();
            ViewModelLocationProvider.Register<StatusControl, StatusViewModel>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(Prism.Ioc.IContainerRegistry containerRegistry)
        {
            var localConfig = FileHelper.LoadFromJsonFile<XamlConfig>(ResourcesMap.LocationDic[Location.GlobalConfigFile]);

            if (localConfig != null)
                containerRegistry.RegisterInstance(localConfig);
            else
                containerRegistry.RegisterSingleton<XamlConfig>();
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = @".\Modules" };
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var config = Container.Resolve<XamlConfig>();
            if (config == null)
                return;


             
        }
    }
}
