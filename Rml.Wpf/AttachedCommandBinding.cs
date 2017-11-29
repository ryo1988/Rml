using System.Windows;
using System.Windows.Input;

namespace Rml.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    public class AttachedCommandBinding
    {
        /// <summary>
        /// 
        /// </summary>
        public static DependencyProperty RegisterCommandBindingsProperty =
            DependencyProperty.RegisterAttached("RegisterCommandBindings", typeof(CommandBindingCollection),
                typeof(AttachedCommandBinding), new PropertyMetadata(null, OnRegisterCommandBindingChanged));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetRegisterCommandBindings(UIElement element, CommandBindingCollection value)
        {
            element?.SetValue(RegisterCommandBindingsProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static CommandBindingCollection GetRegisterCommandBindings(UIElement element)
        {
            return (CommandBindingCollection) element?.GetValue(RegisterCommandBindingsProperty);
        }

        private static void OnRegisterCommandBindingChanged
            (DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is UIElement element)) return;

            element.CommandBindings.Clear();

            if (e.NewValue is CommandBindingCollection bindings)
            {
                element.CommandBindings.AddRange(bindings);
            }
        }
    }
}