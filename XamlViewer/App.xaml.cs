using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using CommonServiceLocator;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Utils.IO;
using XamlService.Commands;
using XamlService.Events;
using XamlService.Payloads;
using XamlViewer.Dialogs;
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

            ViewModelLocationProvider.Register<MessageDialog, MessageDialogViewModel>();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Dialog
            containerRegistry.RegisterDialogWindow<DialogWindow>();
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>();

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
                    AutoCompileDelay = appData.Config.AutoCompileDelay,
                    
                    IsMatchCase = appData.Config.IsMatchCase,
                    IsWholeWords = appData.Config.IsWholeWords,
                    UseRegex = appData.Config.UseRegex
                });
            }
        }
    }
}
