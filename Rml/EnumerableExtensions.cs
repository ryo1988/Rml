using System;
using System.Collections.Generic;
using System.Linq;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TSource> MinByCanEmpty<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source
                .GroupBy(keySelector)
                .OrderBy(o => o.Key)
                .Take(1)
                .SelectMany(o => o);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TSource> MaxByCanEmpty<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source
                .GroupBy(keySelector)
                .OrderByDescending(o => o.Key)
                .Take(1)
                .SelectMany(o => o);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> o) where T:class
            => o.Where(x => x != null)!;

        /// <summary>
        ///
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<(T previous,T current)> Pairwise<T>(this IEnumerable<T> source)
        {
            using var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                yield break;
            }
            var previous = enumerator.Current;
            while (enumerator.MoveNext())
            {
                yield return (previous, enumerator.Current);
                previous = enumerator.Current;
            }
        }
    }
}