using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Utils.IO;
using XamlService.Commands;
using XamlUtil.Common;
using XamlViewer.Models;
using XamlViewer.Utils;
using Common = XamlUtil.Common.Common;
using SWF = System.Windows.Forms;

namespace XamlViewer.ViewModels
{
    public class TabsViewModel : BindableBase
    {
        private ItemsControl _itemsControl = null;

        private AppData _appData = null;
        private IApplicationCommands _appCommands = null;
        private IDialogService _dialogService = null;

        public DelegateCommand NewCommand { get; private set; }
        public DelegateCommand OpenCommand { get; private set; }
        public DelegateCommand SaveAllCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand HelpCommand { get; private set; }

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }
        public DelegateCommand<SizeChangedEventArgs> SizeChangedCommand { get; private set; }
        public DelegateCommand<int?> SelectionChangedCommand { get; private set; }

        public ObservableCollection<TabViewModel> XamlTabs { get; private set; }

        public TabsViewModel(IContainerExtension container, IApplicationCommands appCommands, IDialogService dialogService)
        {
            _appData = container.Resolve<AppData>();
            _appCommands = appCommands;
            _dialogService = dialogService;

            InitEvent();
            InitCommand();

            InitData();
        }

        private bool _isOpenActiveFiles;
        public bool IsOpenActiveFiles
        {
            get { return _isOpenActiveFiles; }
            set { SetProperty(ref _isOpenActiveFiles, value); }
        }

        private Action<int, int> _moveTabPosAction = null;
        public Action<int, int> MoveTabPosAction
        {
            get
            {
                if (_moveTabPosAction == null)
                    _moveTabPosAction = (si, ti) =>
                    {
                        if (si == ti)
                            return;

                        XamlTabs.Move(si, ti);
                    };

                return _moveTabPosAction;
            }
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

            HelpCommand = new DelegateCommand(Help);
            _appCommands.HelpCommand.RegisterCommand(HelpCommand);

            SizeChangedCommand = new DelegateCommand<SizeChangedEventArgs>(SizeChanged);
            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
            SelectionChangedCommand = new DelegateCommand<int?>(SelectionChanged);
        }

        private void InitData()
        {
            _appData.CollectExistedFileAction = CollectExistedFile;

            XamlTabs = new ObservableCollection<TabViewModel>(_appData.Config.Files.Select(f => new TabViewModel(f, CloseXamlTab)));
            if (XamlTabs.Count == 0)
                XamlTabs.Add(new TabViewModel("NewFile.xaml", TabStatus.NoSave, CloseXamlTab));

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
                    ".xaml", TabStatus.NoSave, CloseXamlTab);

            XamlTabs.Insert(0, newTab);
            newTab.IsSelected = true;
        }

        private void Open()
        {
            var ofd = new SWF.OpenFileDialog { Filter = "XAML|*.xaml" };
            if (ofd.ShowDialog() == SWF.DialogResult.OK)
            {
                var tab = XamlTabs.FirstOrDefault(t => t.FileName == ofd.FileName);
                if (tab != null)
                {
                    tab.IsSelected = true;
                    MoveToVisible(tab);
                }
                else
                {
                    var newTab = new TabViewModel(ofd.FileName, CloseXamlTab);
                    XamlTabs.Insert(0, newTab);
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

                        var msg = string.Format("{0}\n\nThis file has been modified outside of the editor.\nDo you want to reload it?", curTab.FileName);
                        _dialogService.ShowMessage(msg, MessageButton.YesNo, MessageType.Question, r =>
                        {
                            if (r.Result == ButtonResult.Yes)
                            {
                                curTab.FileContent = fileContent;
                                curTab.Status &= ~(TabStatus.NoSave);

                                curTab.UpdateTextToEditor();
                            }
                            else
                            {
                                curTab.Status |= TabStatus.NoSave;
                            }
                        });
                    }
                }
                else
                {
                    if (Path.IsPathRooted(curTab.FileName) && !string.IsNullOrEmpty(curTab.MD5Code))
                    {
                        curTab.MD5Code = null;

                        var msg = string.Format("{0}\n\nThis file no longer exists.\nDo you want to remove it?", curTab.FileName);
                        _dialogService.ShowMessage(msg, MessageButton.YesNo, MessageType.Question, r =>
                        {
                            if (r.Result == ButtonResult.Yes)
                            {
                                Remove(curTab);
                                i--;
                            }
                            else
                            {
                                curTab.UpdateFileName(Path.GetFileName(curTab.FileName));
                                curTab.Status |= TabStatus.NoSave;
                            }
                        });
                    }
                }
            }
        }

        private void Help()
        {
            var helpTab = XamlTabs.FirstOrDefault(tab => (tab.Status & TabStatus.Inner) == TabStatus.Inner);
            if (helpTab == null)
            {
                helpTab = new TabViewModel("Help.xaml", TabStatus.Inner, CloseXamlTab);
                XamlTabs.Insert(0, helpTab);

                helpTab.IsSelected = true;
            }
            else
            {
                helpTab.IsSelected = true;
                MoveToVisible(helpTab);
            }
        }

        private void SizeChanged(SizeChangedEventArgs e)
        {
            var itemsControl = e.Source as ItemsControl;
            if (itemsControl == null)
                return;

            for (var i = 0; i < XamlTabs.Count; i++)
            {
                var curTab = XamlTabs[i];
                if (!curTab.IsSelected)
                    continue;

                var container = (UIElement)itemsControl.ItemContainerGenerator.ContainerFromIndex(i);
                if (container == null)
                    return;

                if (DoubleUtil.LessThan(container.TranslatePoint(new Point(), itemsControl).Y, itemsControl.ActualHeight))
                    return;

                XamlTabs.Move(i, i - 1);
                return;
            }
        }

        private void OnLoaded(RoutedEventArgs e)
        {
            _itemsControl = e.OriginalSource as ItemsControl;
        }

        private void SelectionChanged(int? seletedIndex)
        {
            if (!seletedIndex.HasValue || seletedIndex.Value < 0 || seletedIndex.Value > XamlTabs.Count - 1)
                return;

            IsOpenActiveFiles = false;

            var tab = XamlTabs[seletedIndex.Value];
            if (!tab.IsSelected)
            {
                tab.IsSelected = true;
                MoveToVisible(tab);
            }
        }

        #endregion

        #region Event


        #endregion

        #region Func

        private void MoveToVisible(TabViewModel curTab)
        {
            if (_itemsControl == null || curTab == null)
                return;

            var container = (UIElement)_itemsControl.ItemContainerGenerator.ContainerFromItem(curTab);
            if (container == null)
                return;

            if (DoubleUtil.LessThan(container.TranslatePoint(new Point(), _itemsControl).Y, _itemsControl.ActualHeight))
                return;

            var idnex = _itemsControl.ItemContainerGenerator.IndexFromContainer(container);
            XamlTabs.Move(idnex, 0);
        }

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
                    _dialogService.ShowMessage(string.Format("Save file \"{0}\"?", tab.FileName), MessageButton.YesNo, MessageType.Question, r =>
                    {
                        if (r.Result == ButtonResult.Yes)
                        {
                            var sfd = new SWF.SaveFileDialog { Filter = "XAML|*.xaml", FileName = Path.GetFileNameWithoutExtension(tab.FileName) };
                            if (sfd.ShowDialog() == SWF.DialogResult.OK)
                            {
                                tab.UpdateFileName(sfd.FileName);
                                tab.SaveToFile();
                            }
                        }
                    });
                }
                else
                {
                    if ((tab.Status & TabStatus.NoSave) == TabStatus.NoSave)
                    {
                        _dialogService.ShowMessage(string.Format("Save file \"{0}\"?", Path.GetFileName(tab.FileName)), MessageButton.YesNo, MessageType.Question, r =>
                        {
                            if (r.Result == ButtonResult.Yes)
                            {
                                tab.SaveToFile();
                            }
                        });
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
