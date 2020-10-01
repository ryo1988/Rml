using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;
using ValueTaskSupplement;

namespace Rml
{
    internal sealed class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public async Task<IDisposable> LockAsync()
        {
            await _semaphore.WaitAsync().ConfigureAwait(true);
            return System.Reactive.Disposables.Disposable.Create(() => _semaphore.Release());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCall"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    public class CallAndResponseCommand<TCall, TResponse> : Disposable, IObserveCallAndResponseCommand<TCall, TResponse>
    {
        private readonly Subject<TCall> _callTrigger;
        private readonly Subject<TResponse> _responseTrigger;
        private readonly List<Func<TCall, ValueTask<TResponse>>> _callAndResponses = new List<Func<TCall, ValueTask<TResponse>>>();
        private readonly AsyncLock? _asyncLock;

        /// <summary>
        /// 
        /// </summary>
        public CallAndResponseCommand(bool isParallelExecute = false)
        {
            _callTrigger = new Subject<TCall>().AddTo(Cd);
            _responseTrigger = new Subject<TResponse>().AddTo(Cd);
            _asyncLock = isParallelExecute ? null : new AsyncLock();
            System.Reactive.Disposables.Disposable.Create(() =>
            {
                lock (this)
                {
                    _callAndResponses.Clear();
                }
            }).AddTo(Cd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public ValueTask<TResponse[]> ExecuteAsync(in TCall args)
        {
            Func<TCall, ValueTask<TResponse>>[] callAndResponses;
            lock (this)
            {
                callAndResponses = _callAndResponses.ToArray();
            }
            
            var call = args;
            return ValueTaskEx.WhenAll(callAndResponses.Select(o => o(call)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callAndResponse"></param>
        /// <returns></returns>
        public IDisposable Subscribe(Func<TCall, ValueTask<TResponse>> callAndResponse)
        {
            var task = new Func<TCall, ValueTask<TResponse>>(async o =>
            {
                if (_asyncLock is null)
                {
                    return await Do();
                }
                
                using (await _asyncLock.LockAsync().ConfigureAwait(true))
                {
                    return await Do();
                }

                async ValueTask<TResponse> Do()
                {
                    _callTrigger.OnNext(o);
                    var response = await callAndResponse(o).ConfigureAwait(true);
                    _responseTrigger.OnNext(response);
                    return response;
                }
            });

            lock (this)
            {
                _callAndResponses.Add(task);
            }

            return System.Reactive.Disposables.Disposable.Create(() =>
            {
                lock(this)
                {
                    _callAndResponses.Remove(task);
                }
            });
        }

        /// <inheritdoc />
        public IObservable<TCall> ObserveCall()
        {
            return _callTrigger;
        }

        /// <inheritdoc />
        public IObservable<TResponse> ObserveResponse()
        {
            return _responseTrigger;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public class CallAndResponseCommand<TResponse> : Disposable, IObserveCallAndResponseCommand<Unit, TResponse>
    {
        private readonly Subject<Unit> _callTrigger;
        private readonly Subject<TResponse> _responseTrigger;
        private readonly List<Func<ValueTask<TResponse>>> _callAndResponses = new List<Func<ValueTask<TResponse>>>();
        private readonly AsyncLock? _asyncLock;

        /// <summary>
        /// 
        /// </summary>
        public CallAndResponseCommand(bool isParallelExecute = false)
        {
            _callTrigger = new Subject<Unit>().AddTo(Cd);
            _responseTrigger = new Subject<TResponse>().AddTo(Cd);
            _asyncLock = isParallelExecute ? null : new AsyncLock();
            System.Reactive.Disposables.Disposable.Create(() =>
            {
                lock (this)
                {
                    _callAndResponses.Clear();
                }
            }).AddTo(Cd);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ValueTask<TResponse[]> ExecuteAsync()
        {
            Func<ValueTask<TResponse>>[] callAndResponses;
            lock (this)
            {
                callAndResponses = _callAndResponses.ToArray();
            }
            
            return ValueTaskEx.WhenAll(callAndResponses.Select(o => o()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callAndResponse"></param>
        /// <returns></returns>
        public IDisposable Subscribe(Func<ValueTask<TResponse>> callAndResponse)
        {
            var task = new Func<ValueTask<TResponse>>(async () =>
            {
                if (_asyncLock is null)
                {
                    return await Do();
                }
                
                using (await _asyncLock.LockAsync().ConfigureAwait(true))
                {
                    return await Do();
                }

                async ValueTask<TResponse> Do()
                {
                    _callTrigger.OnNext(Unit.Default);
                    var response = await callAndResponse().ConfigureAwait(true);
                    _responseTrigger.OnNext(response);
                    return response;
                }
            });

            lock (this)
            {
                _callAndResponses.Add(task);
            }

            return System.Reactive.Disposables.Disposable.Create(() =>
            {
                lock(this)
                {
                    _callAndResponses.Remove(task);
                }
            });
        }

        /// <inheritdoc />
        public IObservable<Unit> ObserveCall()
        {
            return _callTrigger;
        }

        /// <inheritdoc />
        public IObservable<TResponse> ObserveResponse()
        {
            return _responseTrigger;
        }
    }
}