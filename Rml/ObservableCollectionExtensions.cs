using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        private static IObservable<TResult> ObserveElementCore<TCollection, TElement, TResult>(TCollection source, Func<TElement, IObserver<TResult>?, IDisposable> subscribeAction) where TCollection : INotifyCollectionChanged, IEnumerable<TElement> where TElement : class
        {
            return Observable.Create((Func<IObserver<TResult>, IDisposable>)(observer =>
            {
                var subscriptionCache = new Dictionary<object, IDisposable>();

                Subscribe(source, observer, subscribeAction, subscriptionCache);
                var disposable = source.CollectionChangedAsObservable().Subscribe(x =>
                {
                    if (x.Action == NotifyCollectionChangedAction.Remove || x.Action == NotifyCollectionChangedAction.Replace)
                    {
                        foreach (var element in x.OldItems?.Cast<TElement>() ?? Enumerable.Empty<TElement>())
                        {
                            subscriptionCache[element].Dispose();
                            subscriptionCache.Remove(element);
                        }
                    }
                    if (x.Action == NotifyCollectionChangedAction.Add || x.Action == NotifyCollectionChangedAction.Replace)
                        Subscribe(x.NewItems?.Cast<TElement>() ?? Enumerable.Empty<TElement>(), observer, subscribeAction, subscriptionCache);
                    if (x.Action != NotifyCollectionChangedAction.Reset)
                        return;
                    UnsubscribeAll(subscriptionCache);
                    Subscribe(source, observer, subscribeAction, subscriptionCache);
                });
                return System.Reactive.Disposables.Disposable.Create(() =>
                {
                    disposable.Dispose();
                    UnsubscribeAll(subscriptionCache);
                });

                static void Subscribe(IEnumerable<TElement> elements, IObserver<TResult> observer, Func<TElement, IObserver<TResult>?, IDisposable> subscribeAction, Dictionary<object, IDisposable> subscriptionCache)
                {
                    foreach (var element in elements)
                    {
                        var disposable1 = subscribeAction(element, observer);
                        subscriptionCache.Add(element, disposable1);
                    }
                }

                static void UnsubscribeAll(Dictionary<object, IDisposable> subscriptionCache)
                {
                    foreach (var disposable1 in subscriptionCache.Values)
                        disposable1.Dispose();
                    subscriptionCache.Clear();
                }
            }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="getObservableFunc"></param>
        /// <typeparam name="TCollection"></typeparam>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObservable<(TElement element, TProperty property)> ObserveElementObservable<TCollection, TElement, TProperty>(this TCollection source, Func<TElement, IObservable<TProperty>> getObservableFunc) where TCollection : INotifyCollectionChanged, IEnumerable<TElement> where TElement : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (getObservableFunc == null)
                throw new ArgumentNullException(nameof(getObservableFunc));

            return ObserveElementCore<TCollection, TElement, (TElement element, TProperty property)>(source, (e, o) =>
            {
                var observable = getObservableFunc(e);
                return observable.Subscribe(oo => o?.OnNext((e, oo)));
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="getObservableFunc"></param>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IObservable<(TElement element, TProperty property)> ObserveElementObservable<TElement, TProperty>(this ReadOnlyObservableCollection<TElement> source, Func<TElement, IObservable<TProperty>> getObservableFunc) where TElement : class
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (getObservableFunc == null)
                throw new ArgumentNullException(nameof(getObservableFunc));

            return ObserveElementCore<ReadOnlyObservableCollection<TElement>, TElement, (TElement element, TProperty property)>(source, (e, o) =>
            {
                var observable = getObservableFunc(e);
                return observable.Subscribe(oo => o?.OnNext((e, oo)));
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="source"></param>
        /// <param name="getObservableFunc"></param>
        /// <returns></returns>
        public static IObservable<(TElement element, TProperty property)> ObserveElementObservable<TElement, TProperty>(this ObservableCollection<TElement> source, Func<TElement, IObservable<TProperty>> getObservableFunc) where TElement : class
        {
            return source.ObserveElementObservable<ObservableCollection<TElement>, TElement, TProperty>(getObservableFunc);
        }
    }
}