using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// 
    /// </summary>
    public class WindowClosingBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty ClosingProperty = DependencyProperty.Register(
            "Closing", typeof(ICommand), typeof(WindowClosingBehavior), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// 
        /// </summary>
        public ICommand Closing
        {
            get { return (ICommand) GetValue(ClosingProperty); }
            set { SetValue(ClosingProperty, value); }
        }

        private Window _window;

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs e)
        {
            DetachClosing();

            _window = AssociatedObject as Window ?? Window.GetWindow(AssociatedObject);
            if (_window == null)
            {
                return;
            }
            _window.Closing += WindowOnClosing;
        }

        private void WindowOnClosing(object sender, CancelEventArgs args)
        {
            if (Closing == null)
            {
                return;
            }
            if (Closing.CanExecute(args))
            {
                Closing.Execute(args);
            }
        }

        private void DetachClosing()
        {
            if (_window != null)
            {
                _window.Closing -= WindowOnClosing;
            }

            _window = null;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            DetachClosing();

            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;

            base.OnDetaching();
        }
    }
}