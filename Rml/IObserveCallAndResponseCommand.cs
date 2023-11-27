using System;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCall"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public interface IObserveCallAndResponseCommand<TCall, TResponse>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IObservable<TCall> ObserveCall();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IObservable<TResponse?> ObserveResponse();
    }
}