using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using XamlService.Commands;

namespace XamlViewer.ViewModels
{
    public class ToolbarViewModel : BindableBase
    {
        public ToolbarViewModel(IApplicationCommands applicationCommands)
        {
            ApplicationCommands = applicationCommands;
        }

        private IApplicationCommands _applicationCommands;
        public IApplicationCommands ApplicationCommands
        {
            get { return _applicationCommands; }
            set { SetProperty(ref _applicationCommands, value); }
        }
    }
}
