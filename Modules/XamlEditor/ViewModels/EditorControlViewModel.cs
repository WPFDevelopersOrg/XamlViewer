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
        private string _fileName = null;
        private bool _isReseting = false;

        private XsdParser _xsdParser = null;
        private TextEditorEx _textEditor = null;

        private IEventAggregator _eventAggregator = null;
        private IApplicationCommands _appCommands = null;

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }
        public DelegateCommand DelayArrivedCommand { get; private set; }

        public DelegateCommand CompileCommand { get; private set; }
        public DelegateCommand<string> SaveCommand { get; private set; }
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
            _eventAggregator.GetEvent<ConfigEvents>().Subscribe(OnEditorConfig);
            _eventAggregator.GetEvent<LoadTextEvent>().Subscribe(OnLoadText);
            _eventAggregator.GetEvent<UpdateTabStatusEvent>().Subscribe(OnUpdateTabStatus);
        }

        private void InitCommand()
        {
            //Command
            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
            DelayArrivedCommand = new DelegateCommand(OnDelayArrived);

            CompileCommand = new DelegateCommand(Compile, CanCompile);
            _appCommands.CompileCommand.RegisterCommand(CompileCommand);

            SaveCommand = new DelegateCommand<string>(Save, CanSave);
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

        #region Command

        private bool CanSave(string fileName)
        {
            return !IsReadOnly;
        }

        private void Save(string fileName)
        {
            _fileName = fileName;

            Reset();
            
            if (_eventAggregator != null)
                _eventAggregator.GetEvent<SaveTextEvent>().Publish(new TabInfo { FileName = _fileName, FileContent = _textEditor.Text });
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
            Compile(null);
        }

        #endregion

        #region Event

        private void OnEditorConfig(EditorConfig config)
        {
            FontFamily = config.FontFamily;
            FontSize = config.FontSize;
            ShowLineNumber = config.ShowLineNumber;
            WordWrap = config.WordWrap;

            AutoCompile = config.AutoCompile;
            AutoCompileDelay = config.AutoCompileDelay;
        }

        private void OnLoadText(TabInfo tabInfo)
        {
            if (!string.IsNullOrEmpty(_fileName))
            {
                if (_eventAggregator != null)
                    _eventAggregator.GetEvent<CacheTextEvent>().Publish(new TabInfo { FileName = _fileName, FileContent = _textEditor.Text });
            }

            Reset(() =>
            {
                _fileName = tabInfo.FileName;
                _textEditor.Text = tabInfo.FileContent;
                IsReadOnly = tabInfo.IsReadOnly;

                Compile(tabInfo.FileContent);
            });
        }

        private void OnUpdateTabStatus(TabFlag tabFlag)
        {
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

        private void Compile(string fileContent)
        {
            if (_eventAggregator != null && _textEditor != null)
                _eventAggregator.GetEvent<RefreshDesignerEvent>().Publish(fileContent ?? _textEditor.Text);
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

        #endregion
    }
}
