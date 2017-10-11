using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public static class DescriptionExtension
    {
        /// <summary>
        /// <see cref="DescriptionAttribute.Description"/>を取得します
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="source"></param>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string GetDescription<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            if (propertyExpression.Body is MemberExpression == false)
                throw new ArgumentException(nameof(propertyExpression));

            var memberInfo = ((MemberExpression)propertyExpression.Body).Member;
            if (memberInfo == null)
            {
                return null;
            }

            var attributes = (DescriptionAttribute[])memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 && !String.IsNullOrEmpty(attributes[0].Description) ? attributes[0].Description : memberInfo.Name;
        }
    }
}