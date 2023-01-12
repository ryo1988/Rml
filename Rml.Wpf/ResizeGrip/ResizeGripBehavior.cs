using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.ResizeGrip
{
    /// <summary>
    ///
    /// </summary>
    public class ResizeGripBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.Register(
            "IsDragging", typeof(bool), typeof(ResizeGripBehavior), new PropertyMetadata(default(bool)));

        /// <summary>
        ///
        /// </summary>
        public bool IsDragging
        {
            get { return (bool) GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            nameof(IsEnabled), typeof(bool), typeof(ResizeGripBehavior), new PropertyMetadata(true));

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;

            base.OnDetaching();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var layer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            if (layer is null)
                throw new InvalidOperationException();

            var adorner = new ResizeGripAdorner(AssociatedObject);
            adorner.SetBinding(ResizeGripAdorner.IsDraggingProperty, new Binding(nameof(IsDragging))
            {
                Source = this,
                Mode = BindingMode.OneWayToSource,
            });
            adorner.SetBinding(UIElement.IsEnabledProperty, new Binding(nameof(IsEnabled))
            {
                Source = this,
                Mode = BindingMode.TwoWay,
            });
            layer.Add(adorner);
        }
    }
}