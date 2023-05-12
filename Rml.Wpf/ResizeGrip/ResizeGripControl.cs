using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Rml.Wpf.ResizeGrip;

public class ResizeGripControl : Border
{
    /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.Register(
            "IsDragging", typeof(bool), typeof(ResizeGripControl), new PropertyMetadata(default(bool)));

        /// <summary>
        ///
        /// </summary>
        public bool IsDragging
        {
            get => (bool) GetValue(IsDraggingProperty);
            set => SetValue(IsDraggingProperty, value);
        }

        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty ResizeGripTemplateProperty = DependencyProperty.Register(
            "ResizeGripTemplate", typeof(ControlTemplate), typeof(ResizeGripControl), new PropertyMetadata(default(ControlTemplate), ResizeGripTemplateChanged));

        private static void ResizeGripTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ResizeGripControl)d;
            self.ResizeGripTemplateChanged();
        }

        private void ResizeGripTemplateChanged()
        {
            _resizeGrip.Template = ResizeGripTemplate;
        }

        /// <summary>
        ///
        /// </summary>
        public ControlTemplate ResizeGripTemplate
        {
            get => (ControlTemplate) GetValue(ResizeGripTemplateProperty);
            set => SetValue(ResizeGripTemplateProperty, value);
        }

        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty ResizeGripWidthProperty = DependencyProperty.Register(
            "ResizeGripWidth", typeof(double), typeof(ResizeGripControl), new PropertyMetadata(default(double), ResizeGripWidthChanged));

        private static void ResizeGripWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ResizeGripControl)d;
            self.ResizeGripWidthChanged();
        }

        private void ResizeGripWidthChanged()
        {
            _resizeGrip.Width = ResizeGripWidth;
        }

        /// <summary>
        ///
        /// </summary>
        public double ResizeGripWidth
        {
            get => (double) GetValue(ResizeGripWidthProperty);
            set => SetValue(ResizeGripWidthProperty, value);
        }

        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty ResizeGripHeightProperty = DependencyProperty.Register(
            "ResizeGripHeight", typeof(double), typeof(ResizeGripControl), new PropertyMetadata(default(double), ResizeGripHeightChanged));

        private static void ResizeGripHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ResizeGripControl)d;
            self.ResizeGripHeightChanged();
        }

        private void ResizeGripHeightChanged()
        {
            _resizeGrip.Height = ResizeGripHeight;
        }

        /// <summary>
        ///
        /// </summary>
        public double ResizeGripHeight
        {
            get => (double) GetValue(ResizeGripHeightProperty);
            set => SetValue(ResizeGripHeightProperty, value);
        }

        public static readonly DependencyProperty TargetElementProperty = DependencyProperty.Register(
            nameof(TargetElement), typeof(FrameworkElement), typeof(ResizeGripControl), new PropertyMetadata(default(FrameworkElement), TargetElementChanged));

        private static void TargetElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ResizeGripControl)d;
            self.TargetElementChanged((FrameworkElement)e.OldValue, (FrameworkElement)e.NewValue);
        }
        
        private void TargetElementChanged(FrameworkElement oldValue, FrameworkElement newValue)
        {
            if (oldValue is not null)
                oldValue.SizeChanged -= TargetElementOnSizeChanged;
            
            if (newValue is not null)
                newValue.SizeChanged += TargetElementOnSizeChanged;
        }

        private void TargetElementOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            InvalidateArrange();
        }

        public FrameworkElement TargetElement
        {
            get { return (FrameworkElement)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }

        private readonly Thumb _resizeGrip;
        private readonly VisualCollection _visualChildren;

        static ResizeGripControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeGripControl), new FrameworkPropertyMetadata(typeof(ResizeGripControl)));
        }

        /// <inheritdoc />
        public ResizeGripControl()
        {
            _resizeGrip = new Thumb {Cursor = Cursors.SizeNWSE};
            _resizeGrip.DragDelta += ResizeGripOnDragDelta;
            _resizeGrip.MouseDoubleClick += ResizeGripOnMouseDoubleClick;
            _resizeGrip.DragStarted += ResizeGripOnDragStarted;
            _resizeGrip.DragCompleted += ResizeGripOnDragCompleted;
            _resizeGrip.HorizontalAlignment = HorizontalAlignment.Right;
            _resizeGrip.VerticalAlignment = VerticalAlignment.Bottom;
            _resizeGrip.HorizontalContentAlignment = HorizontalAlignment.Right;
            _resizeGrip.VerticalContentAlignment = VerticalAlignment.Bottom;

            _visualChildren = new VisualCollection(this) {_resizeGrip};
        }

        private void ResizeGripOnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (IsDragging is false) return;

            var frameworkElement = TargetElement ?? throw new InvalidOperationException();

            var width = frameworkElement.Width.Equals(double.NaN) ? frameworkElement.DesiredSize.Width : frameworkElement.ActualWidth;
            var height = frameworkElement.Height.Equals(double.NaN) ? frameworkElement.DesiredSize.Height : frameworkElement.ActualHeight;

            width += e.HorizontalChange;
            height += e.VerticalChange;

            width = System.Math.Max(_resizeGrip.Width, width);
            height = System.Math.Max(_resizeGrip.Height, height);
            width = System.Math.Max(frameworkElement.MinWidth, width);
            height = System.Math.Max(frameworkElement.MinHeight, height);
            width = System.Math.Min(frameworkElement.MaxWidth, width);
            height = System.Math.Min(frameworkElement.MaxHeight, height);
            frameworkElement.SetValue(WidthProperty, width);
            frameworkElement.SetValue(HeightProperty, height);
            frameworkElement.UpdateLayout();

            var contentBounds = VisualTreeHelper.GetDescendantBounds(frameworkElement);
            width = System.Math.Max(contentBounds.Right, width);
            height = System.Math.Max(contentBounds.Bottom, height);
            frameworkElement.SetValue(WidthProperty, width);
            frameworkElement.SetValue(HeightProperty, height);
        }

        private void ResizeGripOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var frameworkElement = TargetElement ?? throw new InvalidOperationException();

            IsDragging = true;
            if (!double.IsNaN(frameworkElement.Height))
            {
                frameworkElement.SetValue(HeightProperty, double.NaN);
            }
            else
            {
                frameworkElement.SetValue(WidthProperty, double.NaN);
                frameworkElement.SetValue(HeightProperty, double.NaN);
            }
            IsDragging = false;
        }

        private void ResizeGripOnDragStarted(object sender, DragStartedEventArgs e)
        {
            IsDragging = true;
        }

        private void ResizeGripOnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            IsDragging = false;
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size finalSize)
        {
            var frameworkElement = TargetElement ?? throw new InvalidOperationException();

            var width = _resizeGrip.Width;
            var height = _resizeGrip.Height;
            var x = frameworkElement.ActualWidth - width;
            var y = frameworkElement.ActualHeight - height;

            _resizeGrip.Arrange(new Rect(x, y, width, height));

            return finalSize;
        }

        /// <inheritdoc />
        protected override int VisualChildrenCount => _visualChildren.Count;

        /// <inheritdoc />
        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }
}