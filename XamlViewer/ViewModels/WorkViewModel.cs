using System.Windows;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using XamlService.Commands;
using XamlService.Payloads;

namespace XamlViewer.ViewModels
{
    public class WorkViewModel : BindableBase
    {
        private IApplicationCommands _appCommands = null;

        public DelegateCommand SwapCommand { get; private set; }
        public DelegateCommand HorSplitCommand { get; private set; }
        public DelegateCommand VerSplitCommand { get; private set; }

        public WorkViewModel(IContainerExtension container)
        {
            _appCommands = container.Resolve<IApplicationCommands>();

            InitCommand();
        }

        private TabSelectInfo _selectInfo;
        public TabSelectInfo SelectInfo
        {
            get { return _selectInfo; }
            set { SetProperty(ref _selectInfo, value); }
        }

        private bool _isReadOnly;
        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                if (value)
                {
                    SplitterRowHeight = new GridLength(0);
                    EditorRowHeight = new GridLength(0);
                }
                else
                {
                    SplitterRowHeight = GridLength.Auto;
                    EditorRowHeight = new GridLength(1, GridUnitType.Star);
                }

                SetProperty(ref _isReadOnly, value);
            }
        }

        #region Init

        private void InitCommand()
        {
            SwapCommand = new DelegateCommand(Swap);
            HorSplitCommand = new DelegateCommand(HorSplit);
            VerSplitCommand = new DelegateCommand(VerSplit);
        }

        #endregion

        public void Example(int type)
        {
            _appCommands.ExampleCommand.Execute(type);
        }

        #region Command

        private void Swap()
        {
            if (DesignerRow == 0)
            {
                DesignerRow = 2;
                EditorRow = 0;
            }
            else
            {
                DesignerRow = 0;
                EditorRow = 2;
            }
        }

        private void HorSplit()
        {
            GridAngle = 0d;
            PaneAngle = 0d;
            HorSplitAngle = 0d;
            VerSplitAngle = 90d;
            CursorSource = @"./Assets/Cursors/Splitter_ud.cur";
        }

        private void VerSplit()
        {
            GridAngle = -90d;
            PaneAngle = 90d;
            HorSplitAngle = 90d;
            VerSplitAngle = 0d;
            CursorSource = @"./Assets/Cursors/Splitter_lr.cur";
        } 

        #endregion

        #region Layout

        private GridLength _splitterRowHeight = GridLength.Auto;
        public GridLength SplitterRowHeight
        {
            get { return _splitterRowHeight; }
            set { SetProperty(ref _splitterRowHeight, value); }
        }

        private GridLength _editorRowHeight = new GridLength(1, GridUnitType.Star);
        public GridLength EditorRowHeight
        {
            get { return _editorRowHeight; }
            set { SetProperty(ref _editorRowHeight, value); }
        }

        private int _designerRow = 0;
        public int DesignerRow
        {
            get { return _designerRow; }
            set { SetProperty(ref _designerRow, value); }
        }

        private int _editorRow = 2;
        public int EditorRow
        {
            get { return _editorRow; }
            set { SetProperty(ref _editorRow, value); }
        }

        private string _cursorSource = @"./Assets/Cursors/Splitter_ud.cur";
        public string CursorSource
        {
            get { return _cursorSource; }
            set { SetProperty(ref _cursorSource, value); }
        }

        private double _gridAngle = 0d;
        public double GridAngle
        {
            get { return _gridAngle; }
            set { SetProperty(ref _gridAngle, value); }
        }

        private double _paneAngle = 0d;
        public double PaneAngle
        {
            get { return _paneAngle; }
            set { SetProperty(ref _paneAngle, value); }
        }

        private double _horSplitAngle = 0d;
        public double HorSplitAngle
        {
            get { return _horSplitAngle; }
            set { SetProperty(ref _horSplitAngle, value); }
        }

        private double _verSplitAngle = 90d;
        public double VerSplitAngle
        {
            get { return _verSplitAngle; }
            set { SetProperty(ref _verSplitAngle, value); }
        }

        #endregion
    }
}
