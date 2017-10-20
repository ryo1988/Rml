using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

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
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        return attributes.Length > 0 && !String.IsNullOrEmpty(attributes[0].Description) ? attributes[0].Description : value.ToString();
                    }
                }

                return string.Empty;
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
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                var description = (string)value;
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