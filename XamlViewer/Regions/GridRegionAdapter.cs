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
                    foreach (UIElement element in e.NewItems)
                    {
                        regionTarget.Children.Add(element);
                    }
                }

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (UIElement element in e.OldItems)
                    {
                        regionTarget.Children.Remove(element);

                        var childRegionManager = RegionManager.GetRegionManager(element);
                        var regionNames = new List<string>();

                        foreach (var childRegion in childRegionManager.Regions)
                        {
                            foreach (var view in childRegion.Views)
                            {
                                //...
                            }

                            childRegion.RemoveAll();
                            regionNames.Add(childRegion.Name);
                        }

                        regionNames.ForEach(name => childRegionManager.Regions.Remove(name));

                        RegionManager.SetRegionManager(element, null);
                        RegionManager.SetRegionContext(element, null);
                    }
                }
            };
        }

        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
