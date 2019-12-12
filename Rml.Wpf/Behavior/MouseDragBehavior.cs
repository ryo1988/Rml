using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// 
    /// </summary>
    public class MouseDragBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty StartProperty = DependencyProperty.Register(
            "Start", typeof(Point), typeof(MouseDragBehavior), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 
        /// </summary>
        public Point Start
        {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty CurrentProperty = DependencyProperty.Register(
            "Current", typeof(Point), typeof(MouseDragBehavior), new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// 
        /// </summary>
        public Point Current
        {
            get { return (Point)GetValue(CurrentProperty); }
            set { SetValue(CurrentProperty, value); }
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            var mouseDown = Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
                h => (s, e) => h(e),
                h => AssociatedObject.MouseDown += h,
                h => AssociatedObject.MouseDown -= h);

            var mouseMove = Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                h => (s, e) => h(e),
                h => AssociatedObject.MouseMove += h,
                h => AssociatedObject.MouseMove -= h);

            var mouseUp = Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
                h => (s, e) => h(e),
                h => AssociatedObject.MouseUp += h,
                h => AssociatedObject.MouseUp -= h);

            mouseDown
                .Select(o => o.GetPosition(AssociatedObject))
                .Select(o => mouseMove
                    .Select(oo => new
                    {
                        Start = o,
                        Current = oo.GetPosition(AssociatedObject),
                    })
                    .StartWith(new
                    {
                        Start = o,
                        Current = o,
                    }))
                .Switch()
                .TakeUntil(mouseUp)
                .Repeat()
                .Subscribe(o =>
                {
                    Start = o.Start;
                    Current = o.Current;
                });
        }
    }
}