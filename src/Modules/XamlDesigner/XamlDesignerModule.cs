using XamlService;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using XamlDesigner.Views;

namespace XamlDesigner
{
    public class XamlDesignerModule : IModule
    {
        private IRegionManager _regionManager = null;

        #region IModule Members

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager = containerProvider.Resolve<IRegionManager>();
            _regionManager.RegisterViewWithRegion(RegionNames.DesignerName, typeof(DesignerControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        #endregion
    }
}
