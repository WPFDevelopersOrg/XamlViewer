using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using XamlService.Payloads;
using XamlViewer.ViewModels;

namespace XamlViewer.Views
{
    /// <summary>
    /// WorkControl.xaml 的交互逻辑
    /// </summary>
    public partial class WorkControl : UserControl
    {
        private WorkViewModel _workViewModel = null;
        public WorkControl()
        {
            InitializeComponent();
        }

        public WorkControl(string tabGuid, bool isSelected, bool isShowEditor = true)
            : this()
        {
            _workViewModel = DataContext as WorkViewModel;
            if (_workViewModel != null)
            {
                _workViewModel.SelectInfo = new TabSelectInfo { Guid = tabGuid, IsSelected = isSelected };
                _workViewModel.IsShowEditor = isShowEditor;
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            if (_workViewModel == null)
                return;

            var hyperlink = e.OriginalSource as Hyperlink;
            if (hyperlink == null || hyperlink.Tag == null)
                return;

            _workViewModel.Example(int.Parse(hyperlink.Tag.ToString()));
        }
    }
}
