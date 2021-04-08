using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <param name="action"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public static IObservable<T?> DoBefore<T>(this IObservable<T?> observable, Action<T?> action, T? initialValue = default)
        {
            return observable
                .StartWith(initialValue)
                .Pairwise()
                .Do(o => action(o.OldItem))
                .Select(o => o.NewItem);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public static IObservable<T?> DisposeBefore<T>(this IObservable<T?> observable, T? initialValue = default)
        where T : IDisposable
        {
            return observable
                .DoBefore(o => o?.Dispose(), initialValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public static IObservable<IEnumerable<T>?> DisposeBefore<T>(this IObservable<IEnumerable<T>?> observable, IEnumerable<T>? initialValue = default)
            where T : IDisposable
        {
            return observable
                .DoBefore(o =>
                {
                    if (o is null)
                        return;

                    foreach (var disposable in o)
                    {
                        disposable.Dispose();
                    }
                }, initialValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="observable"></param>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        public static IObservable<T[]?> DisposeBefore<T>(this IObservable<T[]?> observable, T[]? initialValue = default)
            where T : IDisposable
        {
            return observable
                .DoBefore(o =>
                {
                    if (o is null)
                        return;

                    foreach (var disposable in o)
                    {
                        disposable.Dispose();
                    }
                }, initialValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> WhereNotNull<T>(this IObservable<T?> o) where T:class
            => o.Where(x => x != null)!;
    }
}