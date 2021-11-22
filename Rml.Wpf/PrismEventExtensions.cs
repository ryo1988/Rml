using System;
using System.Reactive.Linq;
using Prism.Events;

namespace Rml.Wpf
{
    /// <summary>
    ///
    /// </summary>
    public static class PrismEventExtensions
    {
        /// <summary>
        /// PubSubEvent to IObservable
        /// </summary>
        /// <param name="self"></param>
        /// <typeparam name="TPayload"></typeparam>
        /// <returns></returns>
        public static IObservable<TPayload> ToObservable<TPayload>(this PubSubEvent<TPayload> self)
        {
            return Observable.Create<TPayload>(observer => self.Subscribe(observer.OnNext));
        }
    }
}