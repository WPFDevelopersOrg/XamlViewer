using System.Windows.Controls;
using Prism.Common;
using Prism.Regions;
using XamlEditor.ViewModels;

namespace XamlEditor.Views
{
    /// <summary>
    /// Interaction logic for EditorControl.xaml
    /// </summary>
    public partial class EditorControl : UserControl
    {
        public EditorControl()
        {
            InitializeComponent();
            RegionContext.GetObservableContext(this).PropertyChanged += PersonDetail_PropertyChanged;
        }

        private void PersonDetail_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var context = (ObservableObject<object>)sender;
            var fileGuid = (string)context.Value;

            (DataContext as EditorControlViewModel).FileGuid = fileGuid;
        }
    }
}
