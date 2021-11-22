using System;
using System.ComponentModel;
using System.Globalization;

namespace Rml
{
    /// <summary>
    /// <see cref="DescriptionAttribute"/>に変換
    /// </summary>
    public class EnumDescriptionConverter : EnumConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public EnumDescriptionConverter(Type type)
            : base(type)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
#if NET6_0
        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
#else
        public override object? ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object? value, Type destinationType)
#endif
        {
            if (destinationType == typeof(string))
            {
                if (value is null)
                    return null;

                var fi = value.GetType().GetField(value.ToString() ?? throw new InvalidOperationException());
                if (fi != null)
                {
                    var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    return attributes.Length > 0 && !String.IsNullOrEmpty(attributes[0].Description) ? attributes[0].Description : value.ToString();
                }

                return null;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <returns></returns>
#if NET6_0
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
#else
        public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
#endif
        {
            if (value is string description)
            {
                foreach (var fi in EnumType.GetFields())
                {
                    var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attributes.Length > 0 && attributes[0].Description == description)
                        return fi.GetValue(fi.Name);
                    if (fi.Name == description)
                        return fi.GetValue(fi.Name);
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}