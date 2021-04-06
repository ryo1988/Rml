using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rml.Wpf.Converter
{
    /// <summary>
    /// 
    /// </summary>
    public class EnumToBoolConverter : IValueConverter
    {
        /// <summary>
        /// falseの際の値
        /// </summary>
        public object FalseValue { get; set; } = DependencyProperty.UnsetValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                return DependencyProperty.UnsetValue;
            }
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }
            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return DependencyProperty.UnsetValue;
            }

            return value.Equals(parameter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool == false)
            {
                return DependencyProperty.UnsetValue;
            }

            if ((bool)value == false)
            {
                return FalseValue;
            }

            return parameter;
        }
    }
}