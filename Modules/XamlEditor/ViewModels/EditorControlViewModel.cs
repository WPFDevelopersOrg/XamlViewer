using Prism.Mvvm;
using System;
using Prism.Commands;
using Prism.Events;
using XamlService.Events;
using XamlService.Commands;
using XamlService.Payloads;
using System.Windows;
using XamlTheme.Controls;

namespace XamlEditor.ViewModels
{
    public class EditorControlViewModel : BindableBase
    {
        private string _fileName = null;
        private bool _isReseting = false;

        private TextEditorEx _textEditor = null;
        private IEventAggregator _eventAggregator = null;

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }
        public DelegateCommand DelayArrivedCommand { get; private set; }

        public DelegateCommand CompileCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        public EditorControlViewModel(IEventAggregator eventAggregator, IApplicationCommands appCommands)
        {
            _eventAggregator = eventAggregator;

            //event
            _eventAggregator.GetEvent<ConfigEvents>().Subscribe(OnEditorConfig);
            _eventAggregator.GetEvent<LoadTextEvent>().Subscribe(OnLoadText);
            _eventAggregator.GetEvent<UpdateTabStatusEvent>().Subscribe(OnUpdateTabStatus);

            //Command
            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
            DelayArrivedCommand = new DelegateCommand(OnDelayArrived);

            CompileCommand = new DelegateCommand(Compile);
            appCommands.CompileCommand.RegisterCommand(CompileCommand);

            SaveCommand = new DelegateCommand(Save);
            appCommands.SaveCommand.RegisterCommand(SaveCommand);
        }

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

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set { SetProperty(ref _isReadOnly, value); }
        }

        private bool _isModified;
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                SetProperty(ref _isModified, value);

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

        #region Command

        private void Save()
        {
            Reset();

            if (_eventAggregator != null)
                _eventAggregator.GetEvent<SaveTextEvent>().Publish(new TabInfo { FileName = _fileName, FileContent = _textEditor.Text });
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

        private void Reset(Action reset = null)
        {
            _isReseting = true;

            if (reset != null)
                reset();

            IsModified = false;

            _isReseting = false;
        }

        private void Compile()
        {
            Compile(null);
        }

        private void Compile(string fileContent)
        {
            if (_eventAggregator != null && _textEditor != null)
                _eventAggregator.GetEvent<RefreshDesignerEvent>().Publish(fileContent ?? _textEditor.Text);
        }
    }
}
