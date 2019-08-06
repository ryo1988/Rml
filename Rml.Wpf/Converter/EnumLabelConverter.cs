using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Rml.Wpf.Converter
{
    /// <summary>
    /// 変換定義
    /// </summary>
    public class EnumLabelConverterInfo
    {
        /// <summary>
        /// 変換元
        /// </summary>
        public object From { get; set; }
        /// <summary>
        /// 変換先
        /// </summary>
        public string To { get; set; }
    }

    /// <summary>
    /// Enumとラベル情報
    /// </summary>
    public class EnumLabel
    {
        /// <summary>
        /// 値
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "xamlから使う")]
        public object Value { get; }

        /// <summary>
        /// ラベル
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "xamlから使う")]
        public string Label { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="value"></param>
        /// <param name="label"></param>
        public EnumLabel(object value, string label)
        {
            Value = value;
            Label = label;
        }
    }

    /// <summary>
    /// 変換定義を元にEnumをEnumとラベル情報に変換
    /// </summary>
    public class EnumLabelConverter : IValueConverter
    {
        /// <summary>
        /// 変換定義
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "xamlから使う")]
        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global", Justification = "xamlから使う")]
        public Collection<EnumLabelConverterInfo> Converters { get; }

        /// <summary>
        /// 
        /// </summary>
        public EnumLabelConverter()
        {
            Converters = new Collection<EnumLabelConverterInfo>();
        }

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var array = value as Array;
            if (array == null)
            {
                return value;
            }

            var result = new EnumLabel[array.Length];
            var index = 0;
            foreach (var arrayValue in array)
            {
                var converter = Converters.FirstOrDefault(o => o.From.Equals(arrayValue));
                if (converter == null)
                {
                    result[index++] = new EnumLabel(arrayValue, arrayValue.ToString());
                }
                else
                {
                    result[index++] = new EnumLabel(arrayValue, converter.To);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}