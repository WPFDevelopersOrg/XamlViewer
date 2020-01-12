using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace XamlViewer.Regions
{
    public class GridRegionAdapter : RegionAdapterBase<Grid>
    {
        public GridRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {

        }

        protected override void Adapt(IRegion region, Grid regionTarget)
        {
            region.Views.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (FrameworkElement view in e.NewItems)
                    {
                        regionTarget.Children.Add(view);
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (FrameworkElement view in e.OldItems)
                    {
                        regionTarget.Children.Remove(view);
                        ClearChildRegionsAndViews(view); 
                    }
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }

        private void ClearChildRegionsAndViews(FrameworkElement curView)
        {
            var childRegionManager = RegionManager.GetRegionManager(curView);
            if (childRegionManager != null)
            {
                var regionNames = new List<string>();

                foreach (var childRegion in childRegionManager.Regions)
                {
                    foreach (FrameworkElement childView in childRegion.Views)
                    {
                        ClearChildRegionsAndViews(childView);
                    }

                    childRegion.RemoveAll();
                    regionNames.Add(childRegion.Name);
                }

                regionNames.ForEach(name => childRegionManager.Regions.Remove(name));
                regionNames.Clear();
            } 

            curView.ClearValue(RegionManager.RegionManagerProperty);
            curView.ClearValue(RegionManager.RegionContextProperty);

            DisposeView(curView);
        }

        private void DisposeView(FrameworkElement view)
        {
            if (view == null)
                return; 

            var disposableView = view as IDisposable;
            var disposableViewModel = view.DataContext as IDisposable; 

            if(disposableView != null)
                disposableView.Dispose();

            if (disposableViewModel != null)
                disposableViewModel.Dispose();
        }
    }
}
