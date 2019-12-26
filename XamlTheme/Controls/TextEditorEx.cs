using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Rendering;
using XamlTheme.Datas;

namespace XamlTheme.Controls
{
    [TemplatePart(Name = TextEditorTemplateName, Type = typeof(TextEditor))]
    public class TextEditorEx : Control
    {
        private static readonly Type _typeofSelf = typeof(TextEditorEx);

        private const string TextEditorTemplateName = "PART_TextEditor";

        private TextEditor _partTextEditor = null;
        private FoldingManager _foldingManager = null;
        private XmlFoldingStrategy _foldingStrategy = null;
        private CompletionWindow _completionWindow = null;

        private DispatcherTimer _timer = null;
        private bool _disabledTimer = false;

        public string Text
        {
            get
            {
                if (_partTextEditor != null)
                    return _partTextEditor.Text;

                return string.Empty;
            }
            set
            {
                if (_partTextEditor != null)
                {
                    _disabledTimer = true;
                    _partTextEditor.Text = value;
                }
            }
        }

        public bool CanRedo
        {
            get { return _partTextEditor.CanRedo; }
        }

        public bool CanUndo
        {
            get { return _partTextEditor.CanUndo; }
        }

        static TextEditorEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(_typeofSelf, new FrameworkPropertyMetadata(_typeofSelf));
        }

        public TextEditorEx()
        {
            _foldingStrategy = new XmlFoldingStrategy() { ShowAttributesWhenFolded = true };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(Math.Max(1, Delay)) };
            _timer.Tick += _timer_Tick;
        }

        #region RouteEvent

        public static readonly RoutedEvent DelayArrivedEvent = EventManager.RegisterRoutedEvent("DelayArrived", RoutingStrategy.Bubble, typeof(RoutedEventArgs), _typeofSelf);
        public event RoutedEventHandler DelayArrived
        {
            add { AddHandler(DelayArrivedEvent, value); }
            remove { RemoveHandler(DelayArrivedEvent, value); }
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty LinePositionProperty = DependencyProperty.Register("LinePosition", typeof(int), _typeofSelf, new PropertyMetadata(OnLinePositionPropertyChanged));
        public int LinePosition
        {
            get { return (int)GetValue(LinePositionProperty); }
            set { SetValue(LinePositionProperty, value); }
        }

        private static void OnLinePositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var pos = (int)e.NewValue;

            ctrl._partTextEditor.TextArea.Caret.Column = pos;
        }

        public static readonly DependencyProperty LineNumberProperty = DependencyProperty.Register("LineNumber", typeof(int), _typeofSelf, new PropertyMetadata(OnLineNumberPropertyChanged));
        public int LineNumber
        {
            get { return (int)GetValue(LineNumberProperty); }
            set { SetValue(LineNumberProperty, value); }
        }

        private static void OnLineNumberPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var line = (int)e.NewValue;

            ctrl._partTextEditor.TextArea.Caret.Line = line;
        }

        public static readonly DependencyProperty IsModifiedProperty = TextEditor.IsModifiedProperty.AddOwner(_typeofSelf);
        public bool IsModified
        {
            get { return (bool)GetValue(IsModifiedProperty); }
            set { SetValue(IsModifiedProperty, value); }
        }

        public static readonly DependencyProperty WordWrapProperty = TextEditor.WordWrapProperty.AddOwner(_typeofSelf);
        public bool WordWrap
        {
            get { return (bool)GetValue(WordWrapProperty); }
            set { SetValue(WordWrapProperty, value); }
        }

        public static readonly DependencyProperty ShowLineNumbersProperty = TextEditor.ShowLineNumbersProperty.AddOwner(_typeofSelf);
        public bool ShowLineNumbers
        {
            get { return (bool)GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = TextEditor.IsReadOnlyProperty.AddOwner(_typeofSelf);
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty IsCodeCompletionProperty = DependencyProperty.Register("IsCodeCompletion", typeof(bool), _typeofSelf);
        public bool IsCodeCompletion
        {
            get { return (bool)GetValue(IsCodeCompletionProperty); }
            set { SetValue(IsCodeCompletionProperty, value); }
        }

        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register("Delay", typeof(double), _typeofSelf, new PropertyMetadata(1d, OnDelayPropertyChanged));
        public double Delay
        {
            get { return (double)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        private static void OnDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as TextEditorEx;
            var delay = (double)e.NewValue;

            if (ctrl._timer != null)
                ctrl._timer.Interval = TimeSpan.FromSeconds(Math.Max(1, delay));
        }

        public static readonly DependencyProperty GenerateCompletionDataProperty =
           DependencyProperty.Register("GenerateCompletionData", typeof(Func<string, string, string, List<string>>), _typeofSelf);
        public Func<string, string, string, List<string>> GenerateCompletionData
        {
            get { return (Func<string, string, string, List<string>>)GetValue(GenerateCompletionDataProperty); }
            set { SetValue(GenerateCompletionDataProperty, value); }
        }

        #endregion

        #region Override

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_partTextEditor != null)
            {
                _partTextEditor.TextChanged -= _partTextEditor_TextChanged;

                _partTextEditor.TextArea.TextEntering -= TextArea_TextEntering;
                _partTextEditor.TextArea.TextEntered -= TextArea_TextEntered;
                _partTextEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
            }

            _partTextEditor = GetTemplateChild(TextEditorTemplateName) as TextEditor;

            if (_partTextEditor != null)
            {
                _partTextEditor.TextChanged += _partTextEditor_TextChanged;

                _partTextEditor.TextArea.TextEntering += TextArea_TextEntering;
                _partTextEditor.TextArea.TextEntered += TextArea_TextEntered;
                _partTextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;

                _partTextEditor.TextArea.SelectionCornerRadius = 0;
                _partTextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
                _partTextEditor.TextArea.SelectionBorder = null;
                _partTextEditor.TextArea.SelectionForeground = null;
            }
        }

        #endregion

        #region Event

        private void _timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            RaiseEvent(new RoutedEventArgs(DelayArrivedEvent));
        }

        private void _partTextEditor_TextChanged(object sender, EventArgs e)
        {
            RefreshFoldings();

            if (_disabledTimer)
            {
                _disabledTimer = false;
                return;
            }

            if (_timer != null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            if (_partTextEditor == null)
                return;

            LineNumber = _partTextEditor.TextArea.Caret.Location.Line;
            LinePosition = _partTextEditor.TextArea.Caret.Location.Column;
        }

        private string FindPreviousNonSpaceChars(int startOffset, int charLength = 1)
        {
			var foundChars = string.Empty;
			
            while (startOffset >= 0)
            {
                var curChar = _partTextEditor.Text[startOffset];
				
                if (!char.IsWhiteSpace(curChar))
				{
                    foundChars += curChar;
					if(foundChars.Length == charLength)
						break;
				}

                startOffset--;
            }

            return foundChars;
        }
		
		private string FindNextNonSpaceChars(int startOffset, int charLength = 1)
        {
			var foundChars = string.Empty;
			var length = _partTextEditor.Text.Length;
			
            while (startOffset < length)
            {
                var curChar = _partTextEditor.Text[startOffset];
				
                if (!char.IsWhiteSpace(curChar))
				{
					foundChars += curChar;
					if(foundChars.Length == charLength)
						break;
				}

                startOffset++;
            }

            return foundChars;
        } 
		
        private string GetParentElement(int startOffset)
        {  
            var foundCount = 0;
            var foundEnd = false;
            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];

                if (curChar == '>' && i > 0 && FindPreviousNonSpaceChars(i - 1) != "/")
                {
                    foundCount++;

                    if (foundCount % 2 == 1)
                        foundEnd = true;

                    continue;
                }

                if (curChar == '/' && i > 0 && FindPreviousNonSpaceChars(i - 1) == "<")
                {
                    foundEnd = false;
                    continue;
                }

                if (curChar == '<' && foundEnd)
                {
                    var element = "";
                    for (int j = i + 1; j <= startOffset; j++)
                    {
                        curChar = _partTextEditor.Text[j];
                        var isWhiteSpace = char.IsWhiteSpace(curChar);

                        if (!string.IsNullOrEmpty(element) && isWhiteSpace)
                            return element;

                        if (!isWhiteSpace)
                            element += curChar;
                    }
                }
            }

            return null;
        }

        private string GetElementInFrontOfSymbol(int startOffset)
        {
            var element = "";

            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];
                var isLetter = char.IsLetter(curChar);

                if (!string.IsNullOrEmpty(element) && !isLetter)
                    return new string(element.Reverse().ToArray());

                if (isLetter)
                    element += curChar;
            }

            return null;
        }

        private Tuple<string, string> GetElementAndAttributeInFrontOfSymbol(int startOffset)
        {
            var finishAttribute = false;
            var attribute = "";

            var element = "";

            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];

                //attribute
                var isLetter = char.IsLetter(curChar);
                if (!string.IsNullOrEmpty(attribute) && !isLetter && !finishAttribute)
                {
                    finishAttribute = true;
                    attribute = new string(attribute.Reverse().ToArray());
                }

                if (isLetter && !finishAttribute)
                    attribute += curChar;

                //element
                if (curChar == '>')
                    break;

                if (curChar == '/' && i > 0 && FindPreviousNonSpaceChars(i - 1) == "<")
                    break;

                if (curChar == '<')
                {
                    for (int j = i + 1; j <= startOffset; j++)
                    {
                        curChar = _partTextEditor.Text[j];
                        var isWhiteSpace = char.IsWhiteSpace(curChar);

                        if (!string.IsNullOrEmpty(element) && isWhiteSpace)
                            break;

                        if (!isWhiteSpace)
                            element += curChar;
                    }
                }
            }

            return new Tuple<string, string>(element, attribute);
        }

        private string GetElement(int startOffset)
        { 
            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];
                if (curChar == '>')
                    return null;

                if (curChar == '/' && i > 0 && FindPreviousNonSpaceChars(i - 1) == "<")
                    return null;

                if (curChar == '<')
                {
                    var element = "";
                    for (int j = i + 1; j <= startOffset; j++)
                    {
                        curChar = _partTextEditor.Text[j];
                        var isWhiteSpace = char.IsWhiteSpace(curChar);

                        if (!string.IsNullOrEmpty(element) && isWhiteSpace)
                            return element;

                        if (!isWhiteSpace)
                            element += curChar;
                    }
                }
            }

            return null;
        }
		
		private void DealBreak()
        { 
		    var curOffset = _partTextEditor.TextArea.Caret.Offset;
			if(curOffset == 0)
				return;
			
			var length = _partTextEditor.Text.Length;
			
			var previousNonSpaceChars = FindPreviousNonSpaceChars(curOffset - 1, 2);
			
		    if(previousNonSpaceChars.Length > 1 && previousNonSpaceChars[1] == '>' && previousNonSpaceChars != "/>" && FindNextNonSpaceChars(curOffset, 2) == "</")
			{
				_partTextEditor.TextArea.Document.Insert(curOffset, "\n");
				_partTextEditor.TextArea.Caret.Line -= 1;
			} 
        }

		private bool IsEvenQuoteInElement(int startOffset)
		{  
			var quoteCount = 0;
		    
            for (int i = startOffset; i >= 0; i--)
            {
                var curChar = _partTextEditor.Text[i];
                if (curChar == '>')
                    return false; 

                if (curChar == '/' && i > 0 && FindPreviousNonSpaceChars(i - 1) == "<")
                    return false;

                if (curChar == '\"') 
					quoteCount++;  
				
                if (curChar == '<')
                    return quoteCount % 2 == 0;
            }
			
			return false;
		}

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (IsReadOnly || !IsCodeCompletion || GenerateCompletionData == null)
                return;

            var offset = _partTextEditor.TextArea.Caret.Offset;

            switch (e.Text)
            {
                case ".": //get attributes
                    {
                        var element = GetElementInFrontOfSymbol(offset - 1);
                        if (!string.IsNullOrEmpty(element))
                            ShowCompletionWindow(GenerateCompletionData(null, element, null));

                        break;
                    }

                case " ": //get attributes
                    {
                        var element = GetElement(offset - 1);
                        if (!string.IsNullOrEmpty(element))
                            ShowCompletionWindow(GenerateCompletionData(null, element, null));

                        break;
                    }

                case "<": //get child elements
                    {
                        var parentElement = GetParentElement(offset - 2);
						var elements = GenerateCompletionData(parentElement, null, null);
						if(elements.Count > 0)
						{
							elements.Insert(0, @"!--");
							elements.Insert(1, @"![CDATA[");
							ShowCompletionWindow(elements);
						}

                        break;
                    }

                case "{":
                    {
                        InsertText("}");
                        ShowCompletionWindow(new List<string> { "Binding", "DynamicResource", "StaticResource", "x:Null", "x:Static" });

                        break;
                    }

                case "=": //get values
                    {
						if(!IsEvenQuoteInElement(offset - 2))
							break;
						
                        InsertText("\"\"");

                        var element = GetElementAndAttributeInFrontOfSymbol(offset - 2);

                        if (element != null && !string.IsNullOrEmpty(element.Item1) && !string.IsNullOrEmpty(element.Item2))
                            ShowCompletionWindow(GenerateCompletionData(null, element.Item1, element.Item2));

                        break;
                    }

                case "\"": //get values
                    {
                        InsertText("\"");
                         
                        if (offset > 2 && _partTextEditor.Text[offset - 2] == '=')
                        {
                            var element = GetElementAndAttributeInFrontOfSymbol(offset - 2);

                            if (element != null && !string.IsNullOrEmpty(element.Item1) && !string.IsNullOrEmpty(element.Item2))
                                ShowCompletionWindow(GenerateCompletionData(null, element.Item1, element.Item2));
                        }

                        break;
                    }

                case ">":  // auto add </XXX>
					{  
                        if (FindPreviousNonSpaceChars(offset - 1, 2) == "/>")
							break;
						
						var element = GetElement(offset - 2);
						if (!string.IsNullOrEmpty(element))
						{
							var insertStr = string.Format("</{0}>", element);
							InsertText(insertStr, true, insertStr.Length); 
						}
					
						break;
					}

                case "/":
					{
						if (FindPreviousNonSpaceChars(offset - 1, 2) == "</")
						{
							var parentElement = GetParentElement(offset - 3);
							if(!string.IsNullOrEmpty(parentElement))
    							ShowCompletionWindow(new List<string> { parentElement + ">" });
						}
						
						break;
					}

                case "\n":  // auto align or insert one space line
				    DealBreak();
                    break;
            }
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (IsReadOnly || !IsCodeCompletion || _completionWindow == null)
                return;

            if (e.Text.Length > 0)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void CompletionList_InsertionRequested(object sender, EventArgs e)
        {
			
            //insert =""
            //_partTextEditor.Document.Insert(_partTextEditor.TextArea.Caret.Offset, "=\"\"");
			
			var offset = _partTextEditor.TextArea.Caret.Offset;
			
			if (FindPreviousNonSpaceChars(offset - 1, 3) == "!--")
			{
				InsertText("-->", true, 3);
			}
			else if(FindPreviousNonSpaceChars(offset - 1, 8) == "![CDATA[")
			{
				InsertText("]]>", true, 3);
			}
        }

        #endregion

        #region Func

        public void Redo()
        {
            if (_partTextEditor == null)
                return;

            _partTextEditor.Redo();
        }

        public void Undo()
        {
            if (_partTextEditor == null)
                return;

            _partTextEditor.Undo();
        }

        private void InsertText(string text, bool caretFallBack = true, int fallbackLength = 1)
        {
            _partTextEditor.TextArea.Document.Insert(_partTextEditor.TextArea.Caret.Offset, text);

            if (caretFallBack)
                _partTextEditor.TextArea.Caret.Column -= fallbackLength;
        }

        private void ShowCompletionWindow(List<string> showDatas)
        {
            if (showDatas == null || showDatas.Count == 0)
                return;

            _completionWindow = new CompletionWindow(_partTextEditor.TextArea);
            _completionWindow.Resources = Resources;
            _completionWindow.MinWidth = 300;
            _completionWindow.MaxHeight = 300;
            _completionWindow.SizeToContent = SizeToContent.Height;

            _completionWindow.CompletionList.InsertionRequested += CompletionList_InsertionRequested;

            var datas = _completionWindow.CompletionList.CompletionData;
            showDatas.ForEach(d => datas.Add(new EditorCompletionData(d)));

            EventHandler handler = null;
            handler = (s, e) =>
            {
                if (_completionWindow != null)
                {
                    _completionWindow.CompletionList.InsertionRequested -= CompletionList_InsertionRequested;

                    _completionWindow.Closed -= handler;
                    _completionWindow.Resources = null;
                    _completionWindow = null;
                }
            };

            _completionWindow.Closed -= handler;
            _completionWindow.Closed += handler;
            _completionWindow.Show();
        } 

        private void RefreshFoldings()
        {
            if (_partTextEditor == null)
                return;

            if (_foldingManager == null)
                _foldingManager = FoldingManager.Install(_partTextEditor.TextArea);

            _foldingStrategy.UpdateFoldings(_foldingManager, _partTextEditor.Document);
        }

        #endregion
    }
}
