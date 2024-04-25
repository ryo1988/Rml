using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// Ctrl+ホイールでFontSizeを調整します
    /// </summary>
    public class ControlZoomBehavior : Behavior<Control>
    {
        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseWheel += AssociatedObjectOnPreviewMouseWheel;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= AssociatedObjectOnPreviewMouseWheel;

            base.OnDetaching();
        }

        private void AssociatedObjectOnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
                return;

            var fontSize = System.Math.Max(System.Math.Floor(AssociatedObject.FontSize + (double)e.Delta / 120), 1);
            AssociatedObject.SetCurrentValue(Control.FontSizeProperty, fontSize);

            e.Handled = true;
        }
    }
}