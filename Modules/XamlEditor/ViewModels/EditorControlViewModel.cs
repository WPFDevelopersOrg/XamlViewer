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

namespace XamlEditor.ViewModels
{
    public class EditorControlViewModel : BindableBase
    {
        private FoldingManager _foldingManager = null;
        private XmlFoldingStrategy _foldingStrategy = null;

        private IEventAggregator _eventAggregator = null;

        public DelegateCommand<TextEditor> TextChangedCommand { get; private set; }

        public EditorControlViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _foldingStrategy = new XmlFoldingStrategy() { ShowAttributesWhenFolded = true };

            //event
            _eventAggregator.GetEvent<EditorConfigEvent>().Subscribe(OnEditorConfigEvent);

            //command
            TextChangedCommand = new DelegateCommand<TextEditor>(OnTextChanged);
        }

        private void OnTextChanged(TextEditor textEditor)
        {
            if (_foldingManager == null)
                _foldingManager = FoldingManager.Install(textEditor.TextArea);

            _foldingStrategy.UpdateFoldings(_foldingManager, textEditor.Document); 
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

        private void OnEditorConfigEvent(EditorConfig config)
        {
            FontFamily = config.FontFamily;
            FontSize = config.FontSize;
            ShowLineNumber = config.ShowLineNumber;
            WordWrap = config.WordWrap;
        }
    }
}
