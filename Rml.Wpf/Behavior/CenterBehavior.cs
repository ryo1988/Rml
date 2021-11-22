using System.Windows;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// Centerから実際の中心位置をOutXOutYに返します
    /// </summary>
    public class CenterBehavior : Microsoft.Xaml.Behaviors.Behavior<FrameworkElement>
    {
        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty OutXProperty = DependencyProperty.Register(
            "OutX", typeof(double), typeof(CenterBehavior), new PropertyMetadata(default(double)));

        /// <summary>
        ///
        /// </summary>
        /// <exception cref="Exception"></exception>
        public double OutX
        {
            get { return (double) GetValue(OutXProperty); }
            set { throw new Exception("this property is readOnly"); }
        }

        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty OutYProperty = DependencyProperty.Register(
            "OutY", typeof(double), typeof(CenterBehavior), new PropertyMetadata(default(double)));

        /// <summary>
        ///
        /// </summary>
        /// <exception cref="Exception"></exception>
        public double OutY
        {
            get { return (double) GetValue(OutYProperty); }
            set { throw new Exception("this property is readOnly"); }
        }

        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register(
            "Center", typeof(Point), typeof(CenterBehavior), new PropertyMetadata(default(Point), CenterChanged));

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
            var self = (CenterBehavior)d;
            self.Update();
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

        private void Update()
        {
            if (AssociatedObject is null)
                return;

            AssociatedObject.RenderTransformOrigin = new Point(0.5, 0.5);
            SetValue(OutXProperty, Center.X - AssociatedObject.RenderTransformOrigin.X * AssociatedObject.ActualWidth);
            SetValue(OutYProperty, Center.Y - AssociatedObject.RenderTransformOrigin.Y * AssociatedObject.ActualHeight);
        }
    }
}