using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XamlDesigner.ViewModels
{
    public class DesignerControlViewModel : BindableBase
    {
        private string _show = "Designer";
        public string Show
        {
            get { return _show; }
            set { SetProperty(ref _show, value); }
        }
    }
}
