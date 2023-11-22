using System;
using System.Reactive.Disposables;
using System.Windows;
using Prism.Events;
using Reactive.Bindings.Extensions;

namespace Rml.Wpf.Behavior;

public class ActivateWindowBehavior : EventAggregatorBehavior<FrameworkElement>
{
    public class ActivateWindowEvent : PubSubEvent;

    protected override IDisposable Subscribe()
    {
        var cd = new CompositeDisposable();

        EventAggregator
            .GetEvent<ActivateWindowEvent>()
            .Subscribe(ActivateWindow)
            .AddTo(cd);

        return cd;
    }

    private void ActivateWindow()
    {
        (AssociatedObject as Window ?? Window.GetWindow(AssociatedObject))?.Activate();
    }
}