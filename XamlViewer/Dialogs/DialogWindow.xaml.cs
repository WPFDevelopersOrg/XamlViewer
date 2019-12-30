using Prism.Services.Dialogs;
using XamlTheme.Controls;

namespace XamlViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : UserWindow, IDialogWindow
    {
        public DialogWindow()
        {
            InitializeComponent(); 
        }

        #region IDialogWindow Members

        public IDialogResult Result { get; set; }

        #endregion
    }
}
