using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel; 
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit;
using Prism.Events;
using XamlService.Events;
using XamlService;
using XamlService.Commands;
using XamlService.Payloads;
using System.Windows;
using System.Windows.Media;

namespace XamlEditor.ViewModels
{
    public class EditorControlViewModel : BindableBase
    {
        private string _fileName = null;

        private TextEditor _textEditor = null;
        private FoldingManager _foldingManager = null;
        private XmlFoldingStrategy _foldingStrategy = null; 

        private IEventAggregator _eventAggregator = null;

        public DelegateCommand<RoutedEventArgs> LoadedCommand { get; private set; }
        public DelegateCommand<TextEditor> TextChangedCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        public EditorControlViewModel(IEventAggregator eventAggregator, IApplicationCommands appCommands)
        {
            _eventAggregator = eventAggregator;
            _foldingStrategy = new XmlFoldingStrategy() { ShowAttributesWhenFolded = true };

            //event
            _eventAggregator.GetEvent<EditorConfigEvent>().Subscribe(OnEditorConfig);
            _eventAggregator.GetEvent<LoadTextEvent>().Subscribe(OnReloadText);

            //Command
            LoadedCommand = new DelegateCommand<RoutedEventArgs>(OnLoaded);
            TextChangedCommand = new DelegateCommand<TextEditor>(OnTextChanged);

            SaveCommand = new DelegateCommand(Save);
            appCommands.SaveCommand.RegisterCommand(SaveCommand);

        }

        private void OnLoaded(RoutedEventArgs e)
        {
            _textEditor = e.OriginalSource as TextEditor;
            if(_textEditor!=null)
            {
                _textEditor.TextArea.SelectionCornerRadius = 0;
                _textEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
                _textEditor.TextArea.SelectionBorder = null;
                _textEditor.TextArea.SelectionForeground = null;
            }
        }

        private void OnTextChanged(TextEditor textEditor)
        { 
            if (_foldingManager == null)
                _foldingManager = FoldingManager.Install(textEditor.TextArea);

            _foldingStrategy.UpdateFoldings(_foldingManager, textEditor.Document); 
        }

        private string _text = "";
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        private bool _isModified;
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                SetProperty(ref _isModified, value);

                _eventAggregator.GetEvent<TextChangedEvent>().Publish(new EditorInfo
                {
                    IsModified = _isModified,
                    CanRedo = _textEditor.CanRedo,
                    CanUndo = _textEditor.CanUndo,
                });
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
        }

        private void OnReloadText(TabInfo tabInfo)
        {
            if(!string.IsNullOrEmpty(_fileName))
            {
                if (_eventAggregator != null)
                    _eventAggregator.GetEvent<CacheTextEvent>().Publish(new TabInfo { FileName = _fileName, FileContent = _textEditor.Text});
            }

            _fileName = tabInfo.FileName;
            _textEditor.Text = tabInfo.FileContent;

            Reset();
        }

        #endregion

        private void Reset()
        {
            SetProperty(ref _isModified, false);
        }
    }
}
