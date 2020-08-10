using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using XamlViewer.Models;

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
