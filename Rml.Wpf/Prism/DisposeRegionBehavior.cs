using System;
using System.Collections.Specialized;
using Prism.Common;
using Prism.Regions;

namespace Rml.Wpf.Prism;

public class DisposeRegionBehavior : IRegionBehavior
{
    public const string Key = nameof(DisposeRegionBehavior);
    public IRegion Region { get; set; }

    public void Attach()
    {
        Region.Views.CollectionChanged += Views_CollectionChanged;
    }

    private void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems is null)
                return;
            
            foreach (var o in e.OldItems)
            {
                MvvmHelpers.ViewAndViewModelAction<IDisposable>(o, oo => oo.Dispose());
            }
        }
    }
}