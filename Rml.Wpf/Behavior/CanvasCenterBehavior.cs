using System.Windows;
using System.Windows.Controls;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// Canvasで配置物のセンターを指定できる
    /// </summary>
    public class CanvasCenterBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
    {
        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Point), typeof(CanvasCenterBehavior), new PropertyMetadata(default(Point), CenterChanged));

        /// <summary>
        ///
        /// </summary>
        public Point Center
        {
            get { return (Point) GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        private static void CenterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (CanvasCenterBehavior)d;
            self.Update();
        }

        private void Update()
        {
            if (AssociatedObject is null)
                return;

            AssociatedObject.RenderTransformOrigin = new Point(0.5, 0.5);
            Canvas.SetLeft(AssociatedObject, Center.X - AssociatedObject.RenderTransformOrigin.X * AssociatedObject.ActualWidth);
            Canvas.SetTop(AssociatedObject, Center.Y - AssociatedObject.RenderTransformOrigin.Y * AssociatedObject.ActualHeight);
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SizeChanged += AssociatedObjectOnSizeChanged;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject.SizeChanged -= AssociatedObjectOnSizeChanged;

            base.OnDetaching();
        }

        private void AssociatedObjectOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Update();
        }
    }
}