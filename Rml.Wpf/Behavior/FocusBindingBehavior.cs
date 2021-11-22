using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    ///
    /// </summary>
    public class FocusBindingBehavior : Behavior<UIElement>
    {
        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty IsFocusProperty = DependencyProperty.Register(
            "IsFocus", typeof(bool), typeof(FocusBindingBehavior), new PropertyMetadata(default(bool)));

        /// <summary>
        ///
        /// </summary>
        /// <exception cref="Exception"></exception>
        public bool IsFocus
        {
            get { return (bool) GetValue(IsFocusProperty); }
            set { throw new Exception("this property is readOnly"); }
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.GotFocus += AssociatedObjectOnGotFocus;
            AssociatedObject.LostFocus += AssociatedObjectOnLostFocus;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject.LostFocus -= AssociatedObjectOnLostFocus;
            AssociatedObject.GotFocus -= AssociatedObjectOnGotFocus;

            base.OnDetaching();
        }

        private void AssociatedObjectOnGotFocus(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(IsFocusProperty, true);
        }

        private void AssociatedObjectOnLostFocus(object sender, RoutedEventArgs e)
        {
            SetCurrentValue(IsFocusProperty, false);
        }
    }
}