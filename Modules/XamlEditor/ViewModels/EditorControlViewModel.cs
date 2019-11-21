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

namespace XamlEditor.ViewModels
{
    public class EditorControlViewModel : BindableBase
    {
        private FoldingManager _foldingManager = null;
        private XmlFoldingStrategy _foldingStrategy = null;

        public DelegateCommand<TextEditor> TextChangedCommand { get; private set; }

        public EditorControlViewModel()
        {
            _foldingStrategy = new XmlFoldingStrategy();

            TextChangedCommand = new DelegateCommand<TextEditor>(OnTextChanged);
        }

        private void OnTextChanged(TextEditor textEditor)
        {
            if (_foldingManager == null)
                _foldingManager = FoldingManager.Install(textEditor.TextArea);

            _foldingStrategy.UpdateFoldings(_foldingManager, textEditor.Document);
        }

        private double _fontSize = 12d;
        public double FontSize
        {
            get { return _fontSize; }
            set { SetProperty(ref _fontSize, value); }
        }

        private FontFamily _fontFamily = new FontFamily("Microsoft YaHei");
        public FontFamily FontFamily
        {
            get { return _fontFamily; }
            set { SetProperty(ref _fontFamily, value); }
        }


    }
}
