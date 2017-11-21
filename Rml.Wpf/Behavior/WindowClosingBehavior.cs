using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

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

            AssociatedObject.Loaded += AssociatedObjectOnLayoutUpdated;
        }

        private void AssociatedObjectOnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            if (_window != null)
            {
                _window.Closing -= WindowOnClosing;
            }
            _window = AssociatedObject as Window ?? Window.GetWindow(AssociatedObject);
            if (_window == null)
            {
                return;
            }
            _window.Closing += WindowOnClosing;
        }

        private void WindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (Closing == null)
            {
                return;
            }
            if (Closing.CanExecute(cancelEventArgs))
            {
                Closing.Execute(cancelEventArgs);
            }
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (_window != null)
            {
                _window.Closing -= WindowOnClosing;
            }

            AssociatedObject.LayoutUpdated -= AssociatedObjectOnLayoutUpdated;
        }
    }
}