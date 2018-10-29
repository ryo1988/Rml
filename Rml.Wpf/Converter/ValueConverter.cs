using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Rml.Wpf.Converter
{
    /// <summary>
    /// 変換定義
    /// </summary>
    public class ValueConverterInfo
    {
        /// <summary>
        /// 変換元
        /// </summary>
        public object From { get; set; }
        /// <summary>
        /// 変換先
        /// </summary>
        public object To { get; set; }
    }

    /// <summary>
    /// 変換定義を元に変換
    /// </summary>
    public class ValueConverter : IValueConverter
    {
        /// <summary>
        /// 変換定義
        /// </summary>
        public Collection<ValueConverterInfo> Converters { get; }

        /// <summary>
        /// 
        /// </summary>
        public ValueConverter()
        {
            Converters = new Collection<ValueConverterInfo>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = Converters.FirstOrDefault(o => o.From.Equals(value));
            if (converter == null)
            {
                return value;
            }
            return converter.To;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var converter = Converters.FirstOrDefault(o => o.To.Equals(value));
            if (converter == null)
            {
                return value;
            }
            return converter.From;
        }
    }
}