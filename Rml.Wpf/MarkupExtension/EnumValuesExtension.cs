using System;

namespace Rml.Wpf.MarkupExtension
{
    /// <summary>
    /// 
    /// </summary>
    public class EnumValuesExtension : System.Windows.Markup.MarkupExtension
    {
        private readonly Type _enumType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enumType"></param>
        public EnumValuesExtension(Type enumType)
        {
            if (null == enumType)
                throw new Exception("EnumType is null");

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

            if (actualEnumType == _enumType)
                return enumValues;

            var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return enumValues;
        }
    }
}