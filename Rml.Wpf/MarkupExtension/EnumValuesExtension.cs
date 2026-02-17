using System;

namespace Rml.Wpf.MarkupExtension
{
    /// <summary>
    /// 
    /// </summary>
    public class EnumValuesExtension : System.Windows.Markup.MarkupExtension
    {
        private readonly Type _enumType;
        
        public bool IncludeNull { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        public EnumValuesExtension(Type enumType)
        {
            _enumType = enumType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            var enumValues = Enum.GetValues(actualEnumType);

            if (IncludeNull is false && actualEnumType == _enumType)
                return enumValues;

            var tempArray = Array.CreateInstance(typeof(Nullable<>).MakeGenericType(actualEnumType), enumValues.Length + 1);
            for (int i = 0; i < enumValues.Length; i++)
            {
                tempArray.SetValue(enumValues.GetValue(i), i + 1);
            }
            return tempArray;
        }
    }
}
