using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
