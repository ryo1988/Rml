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
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BindingProxy<T> : Freezable
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(T), typeof(BindingProxy<T>), new UIPropertyMetadata(null));

        /// <summary>
        /// 
        /// </summary>
        public T Data
        {
            get => (T)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }
    }
}