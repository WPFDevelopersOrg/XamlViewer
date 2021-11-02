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
using XamlUtil.IO;
using XamlService.Commands;
using XamlUtil.Common;
using XamlViewer.Models;
using XamlViewer.Utils;
using XUCommon = XamlUtil.Common.Common;
using XVCommon = XamlViewer.Utils.Common;
using SWF = System.Windows.Forms;
using Prism.Events;
using XamlService.Events;
using System.Threading.Tasks;

namespace XamlViewer.ViewModels
{
    public class TabsViewModel : BindableBase
    {
        private bool _isWorkAreaIniting = false;
        private ItemsControl _itemsControl = null;

        private AppData _appData = null;
        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;
        private IDialogService _dialogService = null;

        public DelegateCommand NewCommand { get; private set; }
        public DelegateCommand OpenCommand { get; private set; }
        public DelegateCommand<bool?> SaveAllCommand { get; private set; }
        public DelegateCommand<string[]> DropCommand { get; private set; }
        public DelegateCommand RefreshCommand { get; private set; }
        public DelegateCommand HelpCommand { get; private set; }
        public DelegateCommand<int?> ExampleCommand { get; private set; }

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }
        public DelegateCommand<SizeChangedEventArgs> SizeChangedCommand { get; private set; }
        public DelegateCommand<int?> SelectionChangedCommand { get; private set; }

        public ObservableCollection<TabViewModel> XamlTabs { get; private set; }

        public TabsViewModel(IContainerExtension container, IDialogService dialogService)
        {
            _dialogService = dialogService;

            _appData = container.Resolve<AppData>();
            _eventAggregator = container.Resolve<IEventAggregator>();
            _appCommands = container.Resolve<IApplicationCommands>();

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

        public bool IsOpenDataSource
        {
            get { return _appData.Config.IsOpenDataSource; }
            set 
            {
                if (_appData.Config.IsOpenDataSource == value)
                    return;

                _appData.Config.IsOpenDataSource=value;
                RaisePropertyChanged();

                _eventAggregator.GetEvent<OpenDataSourceEvent>().Publish(value);
            }
        }
        
        private bool _isSyncDataSource;
        public bool IsSyncDataSource
        {
            get { return _isSyncDataSource; }
            private set { SetProperty(ref _isSyncDataSource, value); }
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
            _eventAggregator.GetEvent<InitWorkAreaEvent>().Subscribe(OnInitWorkArea);
            _eventAggregator.GetEvent<SyncDataSourceEvent>().Subscribe(OnSyncDataSource);
            _eventAggregator.GetEvent<OpenFilesEvent>().Subscribe(OpenFiles);
        }

        private void InitCommand()
        {
            NewCommand = new DelegateCommand(New, CanNew);
            _appCommands.NewCommand.RegisterCommand(NewCommand);

            OpenCommand = new DelegateCommand(Open, CanOpen);
            _appCommands.OpenCommand.RegisterCommand(OpenCommand);

            SaveAllCommand = new DelegateCommand<bool?>(SaveAll, CanSaveAll);
            _appCommands.SaveAllCommand.RegisterCommand(SaveAllCommand);

            DropCommand = new DelegateCommand<string[]>(Drop, CanDrop);
            _appCommands.DropCommand.RegisterCommand(DropCommand);

            RefreshCommand = new DelegateCommand(Refresh);
            _appCommands.RefreshCommand.RegisterCommand(RefreshCommand);

            HelpCommand = new DelegateCommand(Help);
            _appCommands.HelpCommand.RegisterCommand(HelpCommand);

            ExampleCommand=new DelegateCommand<int?>(Example);
            _appCommands.ExampleCommand.RegisterCommand(ExampleCommand);

            SizeChangedCommand = new DelegateCommand<SizeChangedEventArgs>(SizeChanged);
            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
            SelectionChangedCommand = new DelegateCommand<int?>(SelectionChanged);
        }

        private void InitData()
        {
            _appData.DealExistedFileAction = DealExistedFileAsync;
            IsSyncDataSource = _appData.Config.IsSyncDataSource;

            XamlTabs = new ObservableCollection<TabViewModel>(_appData.Config.Files.Select(f => new TabViewModel(f, CloseXamlTab)));
            if (XamlTabs.Count == 0)
                XamlTabs.Add(new TabViewModel("NewFile.xaml", TabStatus.NoSave, CloseXamlTab));

            XamlTabs[0].IsSelected = true;
        }

        #endregion

        #region Command

        private bool CanNew()
        {
            return !_isWorkAreaIniting;
        }

        private void New()
        {
            var newTab =
                new TabViewModel(
                    XUCommon.GetCopyName("NewFile", " ",
                        n => XamlTabs.Any(tab => Path.GetFileNameWithoutExtension(tab.Title).ToLower() == n.ToLower())) +
                    ".xaml", TabStatus.NoSave, CloseXamlTab);

            XamlTabs.Insert(0, newTab);
            newTab.IsSelected = true;
            
            SyncDataSource();
        }

        private bool CanOpen()
        {
            return !_isWorkAreaIniting;
        }

        private void Open()
        {
            var ofd = new SWF.OpenFileDialog { Filter = "XAML|*.xaml", Multiselect = true };
            if (ofd.ShowDialog() == SWF.DialogResult.OK)
            {
                Open(ofd.FileNames);
            }
        } 

        private bool CanSaveAll(bool? ignoreUnsavedTab)
        {
            return !_isWorkAreaIniting;
        }

        private void SaveAll(bool? ignoreUnsavedTab)
        {
            var ignore = ignoreUnsavedTab.HasValue && ignoreUnsavedTab.Value;

            foreach (var curTab in XamlTabs)
            {
                if ((curTab.Status & TabStatus.NoSave) != TabStatus.NoSave)
                    continue;

                if (!FileHelper.Exists(curTab.FileName) && ignore)
                    continue;

                curTab.Save();
            }
        }

        private bool CanDrop(string[] files)
        {
            return !_isWorkAreaIniting;
        }

        private void Drop(string[] files)
        {
            Open(files);
        }

        private void Refresh()
        {
            for (int i = 0; i < XamlTabs.Count; i++)
            {
                var curTab = XamlTabs[i];
                if (FileHelper.Exists(curTab.FileName))
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
            var helpTab = XamlTabs.FirstOrDefault(tab => (tab.Status & TabStatus.Inner) == TabStatus.Inner && tab.FileName == InternalConstStrings.HelpFileName);
            if (helpTab == null)
            {
                helpTab = new TabViewModel(InternalConstStrings.HelpFileName, TabStatus.Inner, CloseXamlTab, false);
                XamlTabs.Insert(0, helpTab);

                helpTab.IsSelected = true;
            }
            else
            {
                helpTab.IsSelected = true;
                MoveToVisible(helpTab);
            }
        }

        private void Example(int? type)
        {
            if (type == null || !ResourcesMap.ExampleFileNameDic.ContainsKey(type.Value))
                return;

            var fileName = ResourcesMap.ExampleFileNameDic[type.Value];

            var exampleTab = XamlTabs.FirstOrDefault(tab => (tab.Status & TabStatus.Inner) == TabStatus.Inner && tab.FileName == fileName);
            if (exampleTab == null)
            {
                exampleTab = new TabViewModel(fileName, TabStatus.Inner, CloseXamlTab);
                XamlTabs.Insert(0, exampleTab);

                exampleTab.IsSelected = true;
            }
            else
            {
                exampleTab.IsSelected = true;
                MoveToVisible(exampleTab);
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

        private void OnInitWorkArea()
        {
            _isWorkAreaIniting = true;

            NewCommand.RaiseCanExecuteChanged();
            OpenCommand.RaiseCanExecuteChanged();
            SaveAllCommand.RaiseCanExecuteChanged();

            //Work Area
            var selectedTab = XamlTabs.FirstOrDefault(tab => tab.IsSelected);
            selectedTab?.InitWorkArea();

            _isWorkAreaIniting = false;

            NewCommand.RaiseCanExecuteChanged();
            OpenCommand.RaiseCanExecuteChanged();
            SaveAllCommand.RaiseCanExecuteChanged();
        }

        private void OnSyncDataSource(string jsonString)
        {
            IsSyncDataSource = !string.IsNullOrWhiteSpace(jsonString);
        }

        private void OpenFiles(string[] xamlFiles)
        {
            Open(xamlFiles);
        }

        #endregion

        #region Func

        private void Open(string[] files)
        {
            var length = files.Length;
            for (int i = 0; i < length; i++)
            {
                var fileName = files[i];
                var tab = XamlTabs.FirstOrDefault(t => t.FileName == fileName);
                if (tab != null)
                {
                    if (i == length - 1)
                    {
                        tab.IsSelected = true;
                        MoveToVisible(tab);
                    }
                }
                else
                {
                    tab = new TabViewModel(fileName, CloseXamlTab);
                    XamlTabs.Insert(0, tab);

                    if (i == length - 1)
                        tab.IsSelected = true;
                }
            }
        }

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

        private async Task DealExistedFileAsync()
        {
            var validTabs = XamlTabs.Where(tab => FileHelper.Exists(tab.FileName));
            var noSavedTabs = validTabs.Where(tab => (tab.Status & TabStatus.NoSave) == TabStatus.NoSave).ToList();

            if (noSavedTabs.Count > 0)
            {
                await Task.Run(() =>
                {
                    _appCommands.SaveAllCommand.Execute(true);

                    while (noSavedTabs.Any(tab => (tab.Status & TabStatus.NoSave) == TabStatus.NoSave))
                    {
                        Task.Delay(200);
                    }
                });
            }

            _appData.Config.Files = validTabs.Select(tab => tab.FileName).ToList();
        }

        private void CloseXamlTab(TabViewModel tab, bool ignoreSaving = false)
        {
            if (!ignoreSaving && (tab.Status & TabStatus.Inner) != TabStatus.Inner)
            {
                if (!FileHelper.Exists(tab.FileName))
                {
                    _dialogService.ShowMessage(string.Format("Save file \"{0}\"?", tab.FileName), MessageButton.YesNo, MessageType.Question, r =>
                    {
                        if (r.Result == ButtonResult.Yes)
                        {
                            var fileName = XVCommon.ShowSaveFileDialog(tab.FileName);
                            if (!string.IsNullOrEmpty(fileName))
                            {
                                tab.UpdateFileName(fileName);
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

        private void SyncDataSource()
        {
            if(_appData.Config.IsSyncDataSource)
                _eventAggregator?.GetEvent<SyncDataSourceEvent>().Publish(_appData.Config.DataSourceJsonString?.Trim());
        }

        #endregion
    }
}
