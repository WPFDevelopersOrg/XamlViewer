using System.Windows.Controls;
using Prism.Common;
using Prism.Regions;
using XamlDesigner.ViewModels;

namespace XamlDesigner.Views
{
    /// <summary>
    /// Interaction logic for DesignerControl.xaml
    /// </summary>
    public partial class DesignerControl : UserControl
    {
        public DesignerControl()
        {
            InitializeComponent();
            RegionContext.GetObservableContext(this).PropertyChanged += PersonDetail_PropertyChanged;
        }

        private void PersonDetail_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var context = (ObservableObject<object>)sender;
            var fileGuid = (string)context.Value;

            (DataContext as DesignerControlViewModel).FileGuid = fileGuid;
        }
    }
}
