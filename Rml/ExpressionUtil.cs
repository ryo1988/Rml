using System;
using System.Linq.Expressions;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public class ExpressionUtil
    {
        /// <summary>
        /// プロパティ名取得
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static string GetPropertyName<TProperty>(Expression<Func<TProperty>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            if (propertyExpression.Body is MemberExpression == false)
                throw new ArgumentException(nameof(propertyExpression));

            return ((MemberExpression) propertyExpression.Body).Member.Name;
        }
    }
}