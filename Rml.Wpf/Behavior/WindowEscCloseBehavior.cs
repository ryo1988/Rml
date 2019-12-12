using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// 
    /// </summary>
    public class WindowEscCloseBehavior : Behavior<FrameworkElement>
    {
        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += AssociatedObject_KeyDown;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            AssociatedObject.KeyDown -= AssociatedObject_KeyDown;
            base.OnDetaching();
        }

        void AssociatedObject_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var window = AssociatedObject as Window ?? Window.GetWindow(AssociatedObject);
                if (window == null)
                {
                    return;
                }
                window.Close();
            }
        }
    }
}