using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Reactive.Bindings;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReadOnlyReactiveCollectionExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <param name="scheduler"></param>
        /// <param name="disposeElement"></param>
        /// <returns></returns>
        public static ReadOnlyReactiveCollection<T> ToReadOnlyReactiveCollection<T>(this IObservable<T[]?> self, IScheduler? scheduler = null, bool disposeElement = true)
        {
            return self
                .Select(o =>
                {
                    var reset = new[]
                    {
                        CollectionChanged<T>.Reset,
                    };
                    var add = o?.Select((oo, ii) => CollectionChanged<T>.Add(ii, oo)) ?? Enumerable.Empty<CollectionChanged<T>>();

                    return reset.Concat(add);
                })
                .Select(o => o.ToObservable())
                .Switch()
                .ToReadOnlyReactiveCollection(scheduler, disposeElement);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="self"></param>
        /// <param name="converter"></param>
        /// <param name="scheduler"></param>
        /// <param name="disposeElement"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static ReadOnlyReactiveCollection<U> ToReadOnlyReactiveCollection<T, U>(this IObservable<T[]?> self, Func<T[]?, U[]?> converter, IScheduler? scheduler = null, bool disposeElement = true)
        {
            var collectionChanged = self
                .Select(o =>
                {
                    var reset = new[]
                    {
                        CollectionChanged<U>.Reset,
                    };
                    var add = converter(o)?.Select((oo, ii) => CollectionChanged<U>.Add(ii, oo)) ?? Enumerable.Empty<CollectionChanged<U>>();

                    return reset.Concat(add);
                })
                .Select(o => o.ToObservable())
                .Switch();
            return Enumerable.Empty<U>()
                .ToReadOnlyReactiveCollection(collectionChanged, scheduler, disposeElement);
        }
    }
}