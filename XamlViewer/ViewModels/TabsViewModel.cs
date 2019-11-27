using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using XamlService.Commands;
using XamlService.Events;
using XamlUtil.Common;
using XamlViewer.Models;
using SWF = System.Windows.Forms;

namespace XamlViewer.ViewModels
{
    public class TabsViewModel : BindableBase
    {
        private XamlConfig _xamlConfig = null;
        private IEventAggregator _eventAggregator = null;

        public DelegateCommand NewCommand { get; private set; }
        public DelegateCommand OpenCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }

        public ObservableCollection<TabViewModel> XamlTabs { get; set; }

        public TabsViewModel(IContainerExtension container, IEventAggregator eventAggregator, IApplicationCommands appCommands)
        {
            _xamlConfig = container.Resolve<XamlConfig>();

            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<CloseTabEvent>().Subscribe(OnCloseTab);

            NewCommand = new DelegateCommand(New);
            appCommands.NewCommand.RegisterCommand(NewCommand);

            OpenCommand = new DelegateCommand(Open);
            appCommands.OpenCommand.RegisterCommand(OpenCommand);

            RefreshCommand = new DelegateCommand(Refresh);
            appCommands.RefreshCommand.RegisterCommand(RefreshCommand);

            XamlTabs = new ObservableCollection<TabViewModel>(_xamlConfig.Files.Select(f => new TabViewModel(f)));
            if (XamlTabs.Count == 0)
                XamlTabs.Add(new TabViewModel("NewFile.xaml") { Status = TabStatus.NoSave });

            XamlTabs[0].IsSelected = true;
        }

        #region Command

        private void New()
        {
            XamlTabs.Add(new TabViewModel(Common.GetCopyName("NewFile", " ", n => XamlTabs.Any(tab => Path.GetFileNameWithoutExtension(tab.Title).ToLower() == n.ToLower())) + ".xaml")
            {
                IsSelected = true,
                Status = TabStatus.NoSave
            });
        }

        private void Open()
        { 
            var ofd = new SWF.OpenFileDialog { Filter = "XAML|*.xaml" };
            if (ofd.ShowDialog() != SWF.DialogResult.OK)
            {
                XamlTabs.Add(new TabViewModel(ofd.FileName)
                {
                    IsSelected = true
                });
            }
        }

        private void Refresh()
        { 
            foreach (var curTab in XamlTabs)
            {
                if (File.Exists(curTab.FileName))
                {
                    var fileContent = string.Empty;
                    using (var fs = new FileStream(curTab.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            fileContent = sr.ReadToEnd();
                        }
                    }

                    if (curTab.FileContent != fileContent)
                    {
                        var msg = string.Format("{0}\n\nthis file has been modified outside of the source editor.\nDo you want to reload it?", curTab.FileName);
                        var r = MessageBox.Show(msg, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r == MessageBoxResult.Yes)
                        {
                            //Reload file...
                            curTab.FileContent = fileContent;
                            curTab.Status &= ~(TabStatus.NoSave);
                        }
                        else
                        {
                            curTab.Status |= TabStatus.NoSave;
                        }
                    }
                }
                else
                {
                    if (Path.IsPathRooted(curTab.FileName))
                    {
                        var r = MessageBox.Show("this file has been deleted.\nDo you want to remove?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r == MessageBoxResult.Yes)
                        {
                            Remove(curTab);
                        }
                        else
                        {
                            curTab.Status |= TabStatus.NoSave;
                        }
                    }
                }
            } 
        }

        #endregion

        #region Event

        private void OnCloseTab(string fileName)
        {
            Remove(XamlTabs.FirstOrDefault(t => t.FileName == fileName));
        }

        #endregion

        private void Remove(TabViewModel tab)
        {
            if(tab==null)
                return;

            XamlTabs.Remove(tab);

            if (XamlTabs.Count == 0)
                New();

            if (tab.IsSelected)
                XamlTabs[0].IsSelected = true;

            _xamlConfig.Files.RemoveAll(f => f == tab.FileName);
        }
    }
}
