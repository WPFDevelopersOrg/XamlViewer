using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using CommonServiceLocator;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Utils.IO;
using XamlService;
using XamlService.Commands;
using XamlService.Events;
using XamlService.Payloads;
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
            ViewModelLocationProvider.Register<ToolbarControl, ToolbarViewModel>();
            ViewModelLocationProvider.Register<SettingControl, SettingViewModel>();
            ViewModelLocationProvider.Register<TabsControl, TabsViewModel>();
            ViewModelLocationProvider.Register<StatusControl, StatusViewModel>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Command
            containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();

            //Config
            var localConfig = FileHelper.LoadFromJsonFile<XamlConfig>(ResourcesMap.LocationDic[Location.GlobalConfigFile]);

            containerRegistry.RegisterInstance(new AppData { Config = localConfig ?? new XamlConfig() });
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = @".\Modules" };
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var appData = Container.Resolve<AppData>();
            if (appData == null)
                return;

            //check history file
            if (appData.Config.Files == null)
                appData.Config.Files = new List<string>();
            else
                appData.Config.Files.RemoveAll(f => !File.Exists(f) || Path.GetExtension(f).ToLower() != ".xaml");

            //check reference file
            if (appData.Config.References == null)
                appData.Config.References = new List<string>();
            else
                appData.Config.References.RemoveAll(f => !File.Exists(AppDomain.CurrentDomain.BaseDirectory + f) || Path.GetExtension(f).ToLower() != ".dll");

            //apply config
            var ea = ServiceLocator.Current.GetInstance<IEventAggregator>();
            if (ea != null)
            {
                ea.GetEvent<ConfigEvents>().Publish(new EditorConfig
                {
                    FontFamily = appData.Config.FontFamily,
                    FontSize = appData.Config.FontSize,

                    WordWrap = appData.Config.WordWrap,
                    ShowLineNumber = appData.Config.ShowLineNumber,

                    AutoCompile = appData.Config.AutoCompile,
                    AutoCompileDelay = appData.Config.AutoCompileDelay
                });
            }
        }
    }
}
