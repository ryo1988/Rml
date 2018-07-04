using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// 
    /// </summary>
    public class WindowClosedBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty ClosedProperty = DependencyProperty.Register(
            "Closed", typeof(ICommand), typeof(WindowClosedBehavior), new PropertyMetadata(default(ICommand)));

        /// <summary>
        /// 
        /// </summary>
        public ICommand Closed
        {
            get { return (ICommand) GetValue(ClosedProperty); }
            set { SetValue(ClosedProperty, value); }
        }

        private Window _window;

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs e)
        {
            DetachClosed();

            _window = AssociatedObject as Window ?? Window.GetWindow(AssociatedObject);
            if (_window == null)
            {
                return;
            }
            _window.Closed += WindowOnClosed;
        }

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs e)
        {
            DetachClosed();
        }

        private void WindowOnClosed(object sender, EventArgs args)
        {
            if (Closed == null)
            {
                return;
            }
            if (Closed.CanExecute(args))
            {
                Closed.Execute(args);
            }
        }

        private void DetachClosed()
        {
            if (_window != null)
            {
                _window.Closed -= WindowOnClosed;
            }

            _window = null;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            DetachClosed();

            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;

            base.OnDetaching();
        }
    }
}