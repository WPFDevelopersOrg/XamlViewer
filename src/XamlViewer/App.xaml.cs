using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using XamlUtil.IO;
using XamlService.Commands;
using XamlService.Events;
using XamlService.Utils;
using XamlUtil.Common;
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
        private string[] _xamlFiles = null;

        public App()
            : base()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        #region Exception

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var msg = "[ UnhandledException ] UI Dispatcher, " + Common.GetExceptionStringFormat(e.Exception);
            var time = " [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff") + "]";

            MessageBox.Show(msg, "Exception" + time, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var msg = "[ UnhandledException ] Task, " + e.Exception.Message;
            var time = " [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff") + "]";

            MessageBox.Show(msg, "Exception" + time, MessageBoxButton.OK, MessageBoxImage.Error);

            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var msg = "[ UnhandledException ] Current Domain, " + Common.GetExceptionStringFormat(e.ExceptionObject as Exception);
            var time = " [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff") + "]";

            MessageBox.Show(msg, "Exception" + time, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        { 
            if(e.Args.Length > 0)
            {
                _xamlFiles = e.Args.Where(f => Path.GetExtension(f).ToLower() == ".xaml").ToArray();
                if (_xamlFiles == null || _xamlFiles.Length == 0)
                {
                    Environment.Exit(0);
                    return;
                }
            } 

            base.OnStartup(e);
        }

        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.Register<MainWindow, MainViewModel>();
            ViewModelLocationProvider.Register<ToolbarControl, ToolbarViewModel>();
            ViewModelLocationProvider.Register<SettingControl, SettingViewModel>();
            ViewModelLocationProvider.Register<DataControl, DataViewModel>();
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

            //Code Completion
            containerRegistry.RegisterInstance(new XsdParser());

            //Config
            var localConfig = FileHelper.LoadFromJsonFile<XamlConfig>(ResourcesMap.LocationDic[Location.GlobalConfigFile]);
            if (localConfig != null)
            {
                //check history file
                if (localConfig.Files == null)
                    localConfig.Files = new List<string>();
                else
                    localConfig.Files.RemoveAll(f => !FileHelper.Exists(f) || Path.GetExtension(f).ToLower() != ".xaml");

                //check reference file
                if (localConfig.References == null)
                    localConfig.References = new List<string>();
                else
                    localConfig.References.RemoveAll(f => !FileHelper.Exists(AppDomain.CurrentDomain.BaseDirectory + f) || Path.GetExtension(f).ToLower() != ".dll");
            }

            //version
            var version = ResourceAssembly.GetName().Version;
            var appData = new AppData { Config = localConfig ?? new XamlConfig(), Version = string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build) };

            if (_xamlFiles != null && _xamlFiles.Length > 0)
            {
                appData.Config.Files.RemoveAll(f => _xamlFiles.Any(xf => Path.GetFullPath(xf).ToLower() == Path.GetFullPath(f).ToLower()));
                appData.Config.Files.InsertRange(0, _xamlFiles);
            }

            containerRegistry.RegisterInstance(appData);
        }

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = ResourcesMap.LocationDic[Location.ModulePath] };
        }

        protected override void OnInitialized()
        { 
            base.OnInitialized();
           
            var eventAggregator = Container.Resolve<IEventAggregator>();
			var appData = Container.Resolve<AppData>();
			
            eventAggregator?.GetEvent<InitWorkAreaEvent>().Publish();
			
			if(appData.Config.IsSyncDataSource)
			    eventAggregator?.GetEvent<SyncDataSourceEvent>().Publish(appData.Config.DataSourceJsonString?.Trim());
        }
    }
}
