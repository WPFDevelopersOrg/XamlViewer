using System;
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
using Utils.IO;
using XamlService.Commands;
using XamlService.Events;
using XamlUtil.Common;
using XamlViewer.Models;
using SWF = System.Windows.Forms;

namespace XamlViewer.ViewModels
{
    public class TabsViewModel : BindableBase
    {
        private AppData _appData = null;
        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;

        public DelegateCommand NewCommand { get; private set; }
        public DelegateCommand OpenCommand { get; private set; }
        public DelegateCommand SaveAllCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }

        public ObservableCollection<TabViewModel> XamlTabs { get; private set; }

        public TabsViewModel(IContainerExtension container, IEventAggregator eventAggregator, IApplicationCommands appCommands)
        {
            _appData = container.Resolve<AppData>();
            _eventAggregator = eventAggregator;
            _appCommands = appCommands;

            InitEvent();
            InitCommand();

            InitData();
        }

        #region Init

        private void InitEvent()
        {

        }

        private void InitCommand()
        {
            NewCommand = new DelegateCommand(New);
            _appCommands.NewCommand.RegisterCommand(NewCommand);

            OpenCommand = new DelegateCommand(Open);
            _appCommands.OpenCommand.RegisterCommand(OpenCommand);

            SaveAllCommand = new DelegateCommand(SaveAll);
            _appCommands.SaveAllCommand.RegisterCommand(SaveAllCommand);

            RefreshCommand = new DelegateCommand(Refresh);
            _appCommands.RefreshCommand.RegisterCommand(RefreshCommand);
        }

        private void InitData()
        {
            _appData.CollectExistedFileAction = CollectExistedFile;

            XamlTabs = new ObservableCollection<TabViewModel>(_appData.Config.Files.Select(f => new TabViewModel(f, CloseXamlTab)));
            if (XamlTabs.Count == 0)
                XamlTabs.Add(new TabViewModel("NewFile.xaml", CloseXamlTab) { Status = TabStatus.NoSave });

            XamlTabs[0].IsSelected = true;
        }

        #endregion

        #region Command

        private void New()
        {
            var newTab =
                new TabViewModel(
                    Common.GetCopyName("NewFile", " ",
                        n => XamlTabs.Any(tab => Path.GetFileNameWithoutExtension(tab.Title).ToLower() == n.ToLower())) +
                    ".xaml", CloseXamlTab)
                {
                    Status = TabStatus.NoSave,
                };

            XamlTabs.Add(newTab);
            newTab.IsSelected = true;
        }

        private void Open()
        {
            var ofd = new SWF.OpenFileDialog { Filter = "XAML|*.xaml" };
            if (ofd.ShowDialog() == SWF.DialogResult.OK)
            {
                var tab = XamlTabs.FirstOrDefault(t => t.FileName == ofd.FileName);
                if (tab != null)
                    tab.IsSelected = true;
                else
                {
                    var newTab = new TabViewModel(ofd.FileName, CloseXamlTab);
                    XamlTabs.Add(newTab);
                    newTab.IsSelected = true;
                }
            }
        }

        private void SaveAll()
        {
            foreach (var curTab in XamlTabs)
            {
                if ((curTab.Status & TabStatus.NoSave) != TabStatus.NoSave)
                    continue;

                if (!File.Exists(curTab.FileName))
                {
                    var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml", FileName = Path.GetFileNameWithoutExtension(curTab.FileName) };
                    if (sfd.ShowDialog() != SWF.DialogResult.OK)
                        continue;
                }

                if (!curTab.IsSelected)
                {
                    curTab.SaveToFile();
                    continue;
                }

                curTab.Save();
            }
        }

        private void Refresh()
        {
            for (int i = 0; i < XamlTabs.Count; i++)
            {
                var curTab = XamlTabs[i];
                if (File.Exists(curTab.FileName))
                {
                    var fileContent = string.Empty;
                    var md5Code = string.Empty;
                    using (var fs = new FileStream(curTab.FileName, FileMode.Open, FileAccess.Read))
                    {
                        md5Code = FileHelper.ComputeMD5(fs);

                        fs.Position = 0;
                        using (var sr = new StreamReader(fs, Encoding.UTF8))
                        {
                            fileContent = sr.ReadToEnd();
                        }
                    }

                    if (curTab.MD5Code != md5Code)
                    {
                        //reset
                        curTab.MD5Code = md5Code;

                        var msg = string.Format("{0}\n\nThis file has been modified outside of the source editor.\nDo you want to reload it?", curTab.FileName);
                        var r = MessageBox.Show(msg, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r == MessageBoxResult.Yes)
                        {
                            curTab.FileContent = fileContent;
                            curTab.Status &= ~(TabStatus.NoSave);

                            curTab.UpdateTextToEditor();
                        }
                        else
                        {
                            curTab.Status |= TabStatus.NoSave;
                        }
                    }
                }
                else
                {
                    if (Path.IsPathRooted(curTab.FileName) && !string.IsNullOrEmpty(curTab.MD5Code))
                    {
                        curTab.MD5Code = null;

                        var msg = string.Format("{0}\n\nThis file has been deleted.\nDo you want to remove it?", curTab.FileName);
                        var r = MessageBox.Show(msg, "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r == MessageBoxResult.Yes)
                        {
                            Remove(curTab);
                            i--;
                        }
                        else
                        {
                            curTab.UpdateFileName(Path.GetFileName(curTab.FileName));
                            curTab.Status |= TabStatus.NoSave;
                        }
                    }
                }
            }
        }

        #endregion

        #region Event


        #endregion

        #region Func

        private void CollectExistedFile()
        {
            _appData.Config.Files = XamlTabs.Where(tab => File.Exists(tab.FileName)).Select(tab => tab.FileName).ToList();
        }

        private void CloseXamlTab(TabViewModel tab, bool ignoreSaving = false)
        {
            if (!ignoreSaving && (tab.Status & TabStatus.Inner) != TabStatus.Inner)
            {
                if (!File.Exists(tab.FileName))
                {
                    var r = MessageBox.Show("Save to file?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r == MessageBoxResult.Yes)
                    {
                        var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml", FileName = Path.GetFileNameWithoutExtension(tab.FileName) };
                        if (sfd.ShowDialog() != SWF.DialogResult.OK)
                            return;

                        tab.UpdateFileName(sfd.FileName);
                        tab.SaveToFile();
                    }
                }
                else
                {
                    if ((tab.Status & TabStatus.NoSave) == TabStatus.NoSave)
                    {
                        var r = MessageBox.Show("Save?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (r == MessageBoxResult.Yes)
                        {
                            tab.SaveToFile();
                        }
                    }
                }
            }

            Remove(tab);
        }

        private void Remove(TabViewModel tab)
        {
            if (tab == null)
                return;

            var index = XamlTabs.IndexOf(tab);
            if (index > -1)
            {
                XamlTabs.RemoveAt(index);
            }

            if (XamlTabs.Count == 0)
                New();
            else
            {
                if (tab.IsSelected)
                {
                    XamlTabs[Math.Max(0, index - 1)].IsSelected = true;
                }
            }

            _appData.Config.Files.RemoveAll(f => f == tab.FileName);
        }

        #endregion
    }
}
