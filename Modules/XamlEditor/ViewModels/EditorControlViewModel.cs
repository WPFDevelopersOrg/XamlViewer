using Prism.Mvvm;
using System;
using Prism.Commands;
using Prism.Events;
using XamlService.Events;
using XamlService.Commands;
using XamlService.Payloads;
using System.Windows;
using XamlTheme.Controls;
using System.Collections.Generic;
using XamlEditor.Utils;
using System.Threading.Tasks;
using Prism.Regions;
using XamlEditor.Views;
using Prism;

namespace XamlEditor.ViewModels
{ 
    public class EditorControlViewModel : BindableBase, IDisposable, IActiveAware
    {
        private string _fileGuid = null;
        private bool _isReseting = false;

        private XsdParser _xsdParser = null;
        private TextEditorEx _textEditor = null;

        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;
        private IRegionManager _regionManager = null;

        private SettingChangedEvent _settingChangedEvent = null;
        private LoadTextEvent _loadTextEvent = null;
        private UpdateTabStatusEvent _updateTabStatusEvent = null;
        private CompileTabEvent _compileEvent = null;
        private ActiveTabEvent _activeTabEvent = null;

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }
        public DelegateCommand DelayArrivedCommand { get; private set; }
         
        public DelegateCommand<string> SaveCommand { get; private set; }
        public DelegateCommand RedoCommand { get; private set; }
        public DelegateCommand UndoCommand { get; private set; }

        public EditorControlViewModel(IEventAggregator eventAggregator, IApplicationCommands appCommands, IRegionManager regionManager)
        {
            _eventAggregator = eventAggregator;
            _appCommands = appCommands;
            _regionManager = regionManager;

            InitEvent();
            InitCommand();
            InitCodeCompletionParser();
        }

        #region Init

        private void InitEvent()
        {
            //event
            _settingChangedEvent = _eventAggregator.GetEvent<SettingChangedEvent>();
            _settingChangedEvent.Subscribe(OnSettingChanged, ThreadOption.PublisherThread, false, info => string.IsNullOrEmpty(info.Guid) || info.Guid == _fileGuid);

            _loadTextEvent = _eventAggregator.GetEvent<LoadTextEvent>();
            _loadTextEvent.Subscribe(OnLoadText, ThreadOption.UIThread, false, tab => tab.Guid == _fileGuid);

            _updateTabStatusEvent = _eventAggregator.GetEvent<UpdateTabStatusEvent>();
            _updateTabStatusEvent.Subscribe(OnUpdateTabStatus, ThreadOption.PublisherThread, false, tab => tab.Guid == _fileGuid);

            _compileEvent = _eventAggregator.GetEvent<CompileTabEvent>();
            _compileEvent.Subscribe(OnCompile, ThreadOption.UIThread, false, guid => guid == _fileGuid);

            _activeTabEvent = _eventAggregator.GetEvent<ActiveTabEvent>();
            _activeTabEvent.Subscribe(OnActiveTab, ThreadOption.PublisherThread, false, info => info.Guid == _fileGuid);
        }

        private void InitCommand()
        {
            //Command
            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
            DelayArrivedCommand = new DelegateCommand(OnDelayArrived); 

            SaveCommand = new DelegateCommand<string>(Save, CanSave);
            _appCommands.SaveCommand.RegisterCommand(SaveCommand);

            RedoCommand = new DelegateCommand(Redo, CanRedo);
            _appCommands.RedoCommand.RegisterCommand(RedoCommand);

            UndoCommand = new DelegateCommand(Undo, CanUndo);
            _appCommands.UndoCommand.RegisterCommand(UndoCommand);
        }

        #endregion 

        #region Property

        private bool _isActive = false;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                SetProperty(ref _isActive, value);
                OnIsActiveChanged();
            }
        }

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                SetProperty(ref _isReadOnly, value);

                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private int _caretLine = 0;
        public int CaretLine
        {
            get { return _caretLine; }
            set
            {
                SetProperty(ref _caretLine, value);
                CaretPosChanged();
            }
        }

        private int _caretColumn = 0;
        public int CaretColumn
        {
            get { return _caretColumn; }
            set
            {
                SetProperty(ref _caretColumn, value);
                CaretPosChanged();
            }
        }

        private bool _isModified;
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                SetProperty(ref _isModified, value);

                RedoCommand.RaiseCanExecuteChanged();
                UndoCommand.RaiseCanExecuteChanged();

                if (!_isReseting)
                {
                    _eventAggregator.GetEvent<TextChangedEvent>().Publish(new EditorInfo
                    {
                        Guid = _fileGuid,
                        IsModified = _isModified,
                    });
                }
            }
        }

        private string _fontFamily = "Calibri";
        public string FontFamily
        {
            get { return _fontFamily; }
            set { SetProperty(ref _fontFamily, value); }
        }

        private double _fontSize = 10d;
        public double FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, value); }
        }

        private bool _wordWrap = false;
        public bool WordWrap
        {
            get { return _wordWrap; }
            set { SetProperty(ref _wordWrap, value); }
        }

        private bool _showLineNumber = true;
        public bool ShowLineNumber
        {
            get { return _showLineNumber; }
            set { SetProperty(ref _showLineNumber, value); }
        }

        private bool _autoCompile = true;
        public bool AutoCompile
        {
            get { return _autoCompile; }
            set
            {
                if (_autoCompile == value)
                    return;

                SetProperty(ref _autoCompile, value);

                if (_autoCompile)
                    Compile();
            }
        }

        private double _autoCompileDelay = 1d;
        public double AutoCompileDelay
        {
            get { return _autoCompileDelay; }
            set { SetProperty(ref _autoCompileDelay, value); }
        }

        private bool _isCodeCompletion = true;
        public bool IsCodeCompletion
        {
            get { return _isCodeCompletion; }
            set { SetProperty(ref _isCodeCompletion, value); }
        }

        private Func<string, string, string, List<string>> _generateCompletionDataFunc = null;
        public Func<string, string, string, List<string>> GenerateCompletionDataFunc
        {
            get
            {
                if (_generateCompletionDataFunc == null)
                    _generateCompletionDataFunc = (parentElement, element, attribute) =>
                    {
                        if (!string.IsNullOrWhiteSpace(parentElement))
                            return _xsdParser.GetChildElements(parentElement);

                        if (!string.IsNullOrWhiteSpace(element))
                        {
                            if (!string.IsNullOrWhiteSpace(attribute))
                                return _xsdParser.GetValues(element, attribute);

                            return _xsdParser.GetAttributes(element);
                        }

                        return _xsdParser.GetElements();
                    };

                return _generateCompletionDataFunc;
            }
        }

        #endregion

        #region Command

        private void OnLoaded(RoutedEventArgs e)
        {
            var editorControl = e.OriginalSource as EditorControl; 
            _textEditor = editorControl.XamlTextEditorEx; 

            _fileGuid = (string)(RegionContext.GetObservableContext(editorControl).Value);

            if (_eventAggregator != null)
            {
                _eventAggregator.GetEvent<RequestSettingEvent>().Publish(_fileGuid);
                _eventAggregator.GetEvent<RequestTextEvent>().Publish(new TabInfo { Guid = _fileGuid });
            }
        }

        private void OnDelayArrived()
        {
            if (AutoCompile)
                Compile();
        }

        private bool CanSave(string fileGuid)
        {
            return !IsReadOnly;
        }

        private void Save(string fileGuid)
        { 
            if(IsActive || fileGuid == _fileGuid)
            {
                Reset();

                var text = string.Empty;

                if (!_textEditor.CheckAccess())
                    _textEditor.Dispatcher.Invoke((Action)(() => text = _textEditor.Text));
                else
                    text = _textEditor.Text;

                if (_eventAggregator != null)
                    _eventAggregator.GetEvent<SaveTextEvent>().Publish(new TabInfo { Guid = _fileGuid, FileContent = text });
            } 
        }

        private bool CanRedo()
        {
            if (_textEditor == null)
                return false;

            return _textEditor.CanRedo;
        }

        private void Redo()
        {
            _textEditor.Redo();

            RedoCommand.RaiseCanExecuteChanged();
            UndoCommand.RaiseCanExecuteChanged();
        }

        private bool CanUndo()
        {
            if (_textEditor == null)
                return false;

            return _textEditor.CanUndo;
        }

        private void Undo()
        {
            _textEditor.Undo();

            RedoCommand.RaiseCanExecuteChanged();
            UndoCommand.RaiseCanExecuteChanged();
        }  

        #endregion

        #region Event

        private void OnSettingChanged(ValueWithGuid<EditorSetting> valueWithGuid)
        {
            if (!string.IsNullOrEmpty(valueWithGuid.Guid) && valueWithGuid.Guid != _fileGuid)
                return;

            FontFamily = valueWithGuid.Value.FontFamily;
            FontSize = valueWithGuid.Value.FontSize;
            ShowLineNumber = valueWithGuid.Value.ShowLineNumber;
            WordWrap = valueWithGuid.Value.WordWrap;

            AutoCompile = valueWithGuid.Value.AutoCompile;
            AutoCompileDelay = valueWithGuid.Value.AutoCompileDelay; 
        }

        private void OnLoadText(TabInfo tabInfo)
        {
            if (tabInfo.Guid != _fileGuid || _textEditor == null)
                return;

            Reset(() =>
            {
                _fileGuid = tabInfo.Guid;
                _textEditor.Text = tabInfo.FileContent;
                IsReadOnly = tabInfo.IsReadOnly;

                Compile(tabInfo.FileContent);
            });

            if (IsActive)
                _textEditor.Focus();
        }

        private void OnUpdateTabStatus(TabFlag tabFlag)
        {
            if (tabFlag.Guid != _fileGuid)
                return;

            IsReadOnly = tabFlag.IsReadOnly;
        }

        private void OnCompile(string guid)
        {
            if (guid != _fileGuid || IsReadOnly)
                return;

            Compile();
        }

        private void OnActiveTab(TabActiveInfo info)
        {
            if (info.Guid != _fileGuid)
                return;

            IsActive = info.IsActive;
        }

        #endregion

        #region IActiveAware

        public event EventHandler IsActiveChanged;

        private void OnIsActiveChanged()
        { 
            RedoCommand.IsActive = IsActive;
            RedoCommand.RaiseCanExecuteChanged();

            UndoCommand.IsActive = IsActive;
            UndoCommand.RaiseCanExecuteChanged();

            if (IsActiveChanged != null)
                IsActiveChanged.Invoke(this, new EventArgs());

            if (IsActive && _textEditor != null)
                _textEditor.Focus();

            CaretPosChanged();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _settingChangedEvent.Unsubscribe(OnSettingChanged);
            _loadTextEvent.Unsubscribe(OnLoadText);
            _updateTabStatusEvent.Unsubscribe(OnUpdateTabStatus);
            _compileEvent.Unsubscribe(OnCompile);
            _activeTabEvent.Unsubscribe(OnActiveTab);

            if (_xsdParser != null)
            {
                _xsdParser.Dispose();
                _xsdParser = null;
            }

            if (_textEditor != null)
            {
                _textEditor.Dispose();
                _textEditor = null;
            }
        }

        #endregion

        #region Func

        private void Compile(string content = null)
        {
            if (_eventAggregator != null && _textEditor != null)
                _eventAggregator.GetEvent<RefreshDesignerEvent>().Publish(new TabInfo { Guid = _fileGuid, FileContent = string.IsNullOrEmpty(content)  ?_textEditor.Text : content });
        }

        private void Reset(Action reset = null)
        {
            _isReseting = true;

            if (reset != null)
                reset();

            IsModified = false;

            _isReseting = false;
        }

        private void CaretPosChanged()
        {
            if (_eventAggregator != null && IsActive)
                _eventAggregator.GetEvent<CaretPositionEvent>().Publish(new CaretPosition() { Line = CaretLine, Column = CaretColumn });
        }

        private void InitCodeCompletionParser()
        {
            _xsdParser = new XsdParser();

            Task.Run(() => IsCodeCompletion = _xsdParser.TryParse());
        }  

        #endregion
    }
}
