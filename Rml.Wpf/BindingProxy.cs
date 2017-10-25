using System.Windows;

namespace Rml.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    public class BindingProxy : Freezable
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        /// <summary>
        /// 
        /// </summary>
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}