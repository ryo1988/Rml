using System;
using System.Reactive.Disposables;
using System.Windows;
using Prism.Events;
using Reactive.Bindings.Extensions;

namespace Rml.Wpf.Behavior;

public class CloseWindowBehavior : EventAggregatorBehavior<FrameworkElement>
{
    public class CloseWindowEvent : PubSubEvent;

    protected override IDisposable Subscribe()
    {
        var cd = new CompositeDisposable();

        EventAggregator
            .GetEvent<CloseWindowEvent>()
            .Subscribe(CloseWindow)
            .AddTo(cd);

        return cd;
    }

    private void CloseWindow()
    {
        (AssociatedObject as Window ?? Window.GetWindow(AssociatedObject))?.Close();
    }
}