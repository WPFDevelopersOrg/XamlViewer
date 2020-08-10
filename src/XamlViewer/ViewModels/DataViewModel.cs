using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Utils.IO;
using XamlService.Events;
using XamlService.Payloads;
using XamlUtil.Common;
using XamlViewer.Models;
using XamlViewer.Utils;

namespace XamlViewer.ViewModels
{
    public class DataViewModel : BindableBase
    {
        private AppData _appData = null;
        private IEventAggregator _eventAggregator = null;

        public DataViewModel(IContainerExtension container, IEventAggregator eventAggregator)
        {
            _appData = container.Resolve<AppData>();
            _eventAggregator = eventAggregator;
        }
    }
}
