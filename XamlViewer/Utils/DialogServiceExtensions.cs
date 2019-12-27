using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Services.Dialogs;
using XamlViewer.Models;

namespace XamlViewer.Utils
{
    public static class DialogServiceExtensions
    {
        public static void ShowMessage(this IDialogService dialogService, string message, MessageButton button = MessageButton.OK, MessageType type = MessageType.Information, Action<IDialogResult> callBack = null)
        {
            var parameters = new DialogParameters();
            parameters.Add("Message", message);
            parameters.Add("Button", button);
            parameters.Add("Type", type);

            dialogService.ShowDialog("MessageDialog", parameters, callBack);
        }
    }
}
