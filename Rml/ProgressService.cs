using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Rml
{
    /// <summary>
    /// 進捗
    /// </summary>
    public class Progress : Disposable
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly ReactivePropertySlim<string> Label;
        
        /// <summary>
        /// 
        /// </summary>
        public readonly ReactivePropertySlim<bool> IsIndeterminate;
        
        /// <summary>
        /// 
        /// </summary>
        public readonly ReactivePropertySlim<int> Max;
        
        /// <summary>
        /// 
        /// </summary>
        public readonly ReactivePropertySlim<int> Current;
        private readonly IYieldable _yieldable;

        /// <summary>
        ///
        /// </summary>
        public readonly CancellationToken? CancellationToken;

        private readonly CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        ///
        /// </summary>
        public bool CanCancel => CancellationToken is not null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposeAction"></param>
        /// <param name="yieldable"></param>
        /// <param name="isCancelable"></param>
        public Progress(Action<Progress> disposeAction, IYieldable yieldable, bool isCancelable)
        {
            _yieldable = yieldable;
            _cancellationTokenSource = isCancelable ? new CancellationTokenSource() : null;
            Cd.Add(System.Reactive.Disposables.Disposable.Create(() =>
            {
                _cancellationTokenSource?.Dispose();
                disposeAction(this);
            }));
            
            Label = new ReactivePropertySlim<string>().AddTo(Cd);
            IsIndeterminate = new ReactivePropertySlim<bool>().AddTo(Cd);
            Max = new ReactivePropertySlim<int>().AddTo(Cd);
            Current = new ReactivePropertySlim<int>().AddTo(Cd);
            CancellationToken = _cancellationTokenSource?.Token;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask IncrementAsync()
        {
            Current.Value++;
            await _yieldable.Yield();
        }

        /// <summary>
        ///
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IYieldable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ValueTask Yield();
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IProgressService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ReadOnlyObservableCollection<Progress> GetProgresses();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Progress Create(string label, bool isCancelable, int max = 0);
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProgressService : IProgressService
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly ReadOnlyObservableCollection<Progress> Progresses;
        private readonly ObservableCollection<Progress> _progresses = new ObservableCollection<Progress>();

        private readonly IYieldable _yieldable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="yieldable"></param>
        public ProgressService(IYieldable yieldable)
        {
            _yieldable = yieldable;
            Progresses = new ReadOnlyObservableCollection<Progress>(_progresses);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ReadOnlyObservableCollection<Progress> GetProgresses()
        {
            return Progresses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Progress Create(string label, bool isCancelable, int max = 0)
        {
            var progress = new Progress(o =>
            {
                lock(this)
                    _progresses.Remove(o);
            }, _yieldable, isCancelable)
            {
                Label = { Value = label},
                Max = { Value = max},
                IsIndeterminate = { Value = max is 0}
            };
            
            lock(this)
                _progresses.Add(progress);
            
            return progress;
        }
    }
}