using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Utils.IO;
using XamlService.Commands;
using XamlService.Events;
using XamlViewer.Dialogs;
using XamlViewer.Models;
using XamlViewer.Regions;
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
            ViewModelLocationProvider.Register<ToolbarControl, ToolbarViewModel>();
            ViewModelLocationProvider.Register<SettingControl, SettingViewModel>();
            ViewModelLocationProvider.Register<TabsControl, TabsViewModel>();
            ViewModelLocationProvider.Register<WorkControl, WorkViewModel>();
            ViewModelLocationProvider.Register<StatusControl, StatusViewModel>();

            ViewModelLocationProvider.Register<MessageDialog, MessageDialogViewModel>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
        {
            base.ConfigureRegionAdapterMappings(regionAdapterMappings);
            regionAdapterMappings.RegisterMapping(typeof(Grid), Container.Resolve<GridRegionAdapter>());
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {  
            //Dialog
            containerRegistry.RegisterDialogWindow<DialogWindow>();
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>();

            //Command
            containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();

            //Work Area
            containerRegistry.Register<WorkControl>();

            //Config
            var localConfig = FileHelper.LoadFromJsonFile<XamlConfig>(ResourcesMap.LocationDic[Location.GlobalConfigFile]);
            if (localConfig != null)
            {
                //check history file
                if (localConfig.Files == null)
                    localConfig.Files = new List<string>();
                else
                    localConfig.Files.RemoveAll(f => !File.Exists(f) || Path.GetExtension(f).ToLower() != ".xaml");

                //check reference file
                if (localConfig.References == null)
                    localConfig.References = new List<string>();
                else
                    localConfig.References.RemoveAll(f => !File.Exists(AppDomain.CurrentDomain.BaseDirectory + f) || Path.GetExtension(f).ToLower() != ".dll");
            }

            containerRegistry.RegisterInstance(new AppData { Config = localConfig ?? new XamlConfig() });
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = @".\Modules" };
        }

        protected override void OnInitialized()
        { 
            base.OnInitialized();

            var eventAggregator = Container.Resolve<IEventAggregator>();
            if (eventAggregator != null)
                eventAggregator.GetEvent<InitWorkAreaEvent>().Publish();
        }
    }
}
