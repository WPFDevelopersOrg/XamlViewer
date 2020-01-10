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

namespace XamlEditor.ViewModels
{
    public class EditorControlViewModel : BindableBase
    {
        private bool _isReseting = false;
        private bool _canSyncSearchConfig = false;

        private XsdParser _xsdParser = null;
        private TextEditorEx _textEditor = null;

        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }
        public DelegateCommand DelayArrivedCommand { get; private set; }

        public DelegateCommand CompileCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }
        public DelegateCommand RedoCommand { get; private set; }
        public DelegateCommand UndoCommand { get; private set; }

        public EditorControlViewModel(IEventAggregator eventAggregator, IApplicationCommands appCommands)
        {
            _eventAggregator = eventAggregator;
            _appCommands = appCommands;

            InitEvent();
            InitCommand();
            InitCodeCompletionParser();
        }

        #region Init

        private void InitEvent()
        {
            //event
            _eventAggregator.GetEvent<SettingChangedEvents>().Subscribe(OnEditorConfig);
            _eventAggregator.GetEvent<LoadTextEvent>().Subscribe(OnLoadText, ThreadOption.PublisherThread, false, tab => tab.Guid == FileGuid);
            _eventAggregator.GetEvent<UpdateTabStatusEvent>().Subscribe(OnUpdateTabStatus, ThreadOption.PublisherThread, false, tab => tab.Guid == FileGuid);
        }

        private void InitCommand()
        {
            //Command
            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
            DelayArrivedCommand = new DelegateCommand(OnDelayArrived);

            CompileCommand = new DelegateCommand(Compile, CanCompile);
            _appCommands.CompileCommand.RegisterCommand(CompileCommand);

            SaveCommand = new DelegateCommand(Save, CanSave);
            _appCommands.SaveCommand.RegisterCommand(SaveCommand);

            RedoCommand = new DelegateCommand(Redo, CanRedo);
            _appCommands.RedoCommand.RegisterCommand(RedoCommand);

            UndoCommand = new DelegateCommand(Undo, CanUndo);
            _appCommands.UndoCommand.RegisterCommand(UndoCommand);
        }

        #endregion

        #region Command

        private void OnLoaded(RoutedEventArgs e)
        {
            _textEditor = e.OriginalSource as TextEditorEx;

            if (_eventAggregator != null)
                _eventAggregator.GetEvent<RequestTextEvent>().Publish(new TabInfo());
        }

        private void OnDelayArrived()
        {
            if (AutoCompile)
                Compile();
        }

        #endregion

        private string _fileGuid = null;
        public string FileGuid
        {
            get { return _fileGuid; }
            set { SetProperty(ref _fileGuid, value); }
        }

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                SetProperty(ref _isReadOnly, value);

                SaveCommand.RaiseCanExecuteChanged();
                CompileCommand.RaiseCanExecuteChanged();
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
                SetProperty(ref _autoCompile, value);

                if (AutoCompile)
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

        #region Search

        private bool _isMatchCase;
        public bool IsMatchCase
        {
            get { return _isMatchCase; }
            set
            {
                if (_isMatchCase == value)
                    return;

                SetProperty(ref _isMatchCase, value);
                ApplySearchConfig();
            }
        }

        private bool _isWholeWords;
        public bool IsWholeWords
        {
            get { return _isWholeWords; }
            set
            {
                if (_isWholeWords == value)
                    return;

                SetProperty(ref _isWholeWords, value);
                ApplySearchConfig();
            }
        }

        private bool _useRegex;
        public bool UseRegex
        {
            get { return _useRegex; }
            set
            {
                if (_useRegex == value)
                    return;

                SetProperty(ref _useRegex, value);
                ApplySearchConfig();
            }
        }

        #endregion

        #region Command

        private bool CanSave()
        {
            return !IsReadOnly;
        }

        private void Save()
        {
            Reset();

            if (_eventAggregator != null)
                _eventAggregator.GetEvent<SaveTextEvent>().Publish(new TabInfo { Guid = FileGuid, FileContent = _textEditor.Text });
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

        private bool CanCompile()
        {
            return !IsReadOnly;
        }

        private void Compile()
        {
            if (_eventAggregator != null && _textEditor != null)
                _eventAggregator.GetEvent<RefreshDesignerEvent>().Publish(new TabInfo { Guid = FileGuid, FileContent = _textEditor.Text });
        }

        #endregion

        #region Event

        private void OnEditorConfig(EditorSetting config)
        {
            FontFamily = config.FontFamily;
            FontSize = config.FontSize;
            ShowLineNumber = config.ShowLineNumber;
            WordWrap = config.WordWrap;

            AutoCompile = config.AutoCompile;
            AutoCompileDelay = config.AutoCompileDelay;

            IsMatchCase = config.IsMatchCase;
            IsWholeWords = config.IsWholeWords;
            UseRegex = config.UseRegex;

            _canSyncSearchConfig = true;
        }

        private void OnLoadText(TabInfo tabInfo)
        {
            if (tabInfo.Guid != FileGuid || _textEditor == null)
                return;

            Reset(() =>
            {
                FileGuid = tabInfo.Guid;
                _textEditor.Text = tabInfo.FileContent;
                IsReadOnly = tabInfo.IsReadOnly;

                Compile();
            });
        }

        private void OnUpdateTabStatus(TabFlag tabFlag)
        {
            if (tabFlag.Guid != FileGuid)
                return;

            IsReadOnly = tabFlag.IsReadOnly;
        }

        #endregion

        #region Func

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
            if (_eventAggregator != null)
                _eventAggregator.GetEvent<CaretPositionEvent>().Publish(new CaretPosition() { Line = CaretLine, Column = CaretColumn });
        }

        private void InitCodeCompletionParser()
        {
            _xsdParser = new XsdParser();

            Task.Run(() => IsCodeCompletion = _xsdParser.TryParse());
        }

        private void ApplySearchConfig()
        {
            if (_eventAggregator != null && _canSyncSearchConfig)
            {
                _eventAggregator.GetEvent<SearchFilterChangedEvents>().Publish(new SearchFilter
                {
                    IsMatchCase = IsMatchCase,
                    IsWholeWords = IsWholeWords,
                    UseRegex = UseRegex,
                });
            }
        }

        #endregion
    }
}
