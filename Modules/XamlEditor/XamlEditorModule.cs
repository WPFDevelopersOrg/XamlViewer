using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using XamlService;
using XamlEditor.Views;

namespace XamlEditor
{
    [ModuleDependency("XamlDesignerModule")]
    public class XamlEditorModule : IModule
    {
        private IRegionManager _regionManager = null;

        #region IModule Members

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager = containerProvider.Resolve<IRegionManager>();
            _regionManager.RegisterViewWithRegion(RegionNames.EditorName,typeof(EditorControl));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }

        #endregion
    }
}
