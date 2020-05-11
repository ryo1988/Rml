using System;
using System.ComponentModel;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string? ToDescription(this Enum value)
        {
            var converter = TypeDescriptor.GetConverter(value);
            return converter.ConvertToString(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T ToEnum<T>(this string value)
            where T : struct
        {
            var converter = TypeDescriptor.GetConverter(typeof(T));
            var convertFromString = converter.ConvertFromString(value);
            if (convertFromString == null)
            {
                return default;
            }
            return (T)convertFromString;
        }
    }
}