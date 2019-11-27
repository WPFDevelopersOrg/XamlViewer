using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
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

namespace XamlEditor.ViewModels
{
    public class EditorControlViewModel : BindableBase
    {
        private FoldingManager _foldingManager = null;
        private XmlFoldingStrategy _foldingStrategy = null; 

        private IEventAggregator _eventAggregator = null;

        public DelegateCommand<TextEditor> TextChangedCommand { get; private set; }
        public DelegateCommand SaveCommand { get; private set; }

        public EditorControlViewModel(IEventAggregator eventAggregator, IApplicationCommands appCommands)
        {
            _eventAggregator = eventAggregator;
            _foldingStrategy = new XmlFoldingStrategy() { ShowAttributesWhenFolded = true };

            //event
            _eventAggregator.GetEvent<EditorConfigEvent>().Subscribe(OnEditorConfig);
            _eventAggregator.GetEvent<ReloadTextEvent>().Subscribe(OnReloadText);

            //command
            TextChangedCommand = new DelegateCommand<TextEditor>(OnTextChanged);

            SaveCommand = new DelegateCommand(Save);
            appCommands.SaveCommand.RegisterCommand(SaveCommand);

        }

        private void OnTextChanged(TextEditor textEditor)
        {
            Text = textEditor.Text;

            if (_foldingManager == null)
                _foldingManager = FoldingManager.Install(textEditor.TextArea);

            _foldingStrategy.UpdateFoldings(_foldingManager, textEditor.Document);

            _eventAggregator.GetEvent<TextChangedEvent>().Publish(new EditorInfo
            {
                CanRedo = textEditor.CanRedo,
                CanUndo = textEditor.CanUndo,
            });
        }

        private string _text = "";
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
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
            if (_eventAggregator != null)
                _eventAggregator.GetEvent<SaveTextEvent>().Publish(Text);
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

        private void OnReloadText(string text)
        {
            Text = text;
        }

        #endregion
    }
}
