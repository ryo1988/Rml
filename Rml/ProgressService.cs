using System;
using System.Collections.ObjectModel;
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
        /// <param name="disposeAction"></param>
        /// <param name="yieldable"></param>
        public Progress(Action<Progress> disposeAction, IYieldable yieldable)
        {
            _yieldable = yieldable;
            Cd.Add(System.Reactive.Disposables.Disposable.Create(() => disposeAction(this)));
            
            Label = new ReactivePropertySlim<string>().AddTo(Cd);
            IsIndeterminate = new ReactivePropertySlim<bool>().AddTo(Cd);
            Max = new ReactivePropertySlim<int>().AddTo(Cd);
            Current = new ReactivePropertySlim<int>().AddTo(Cd);
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
    public class ProgressService
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
        public Progress Create(string label, int max = 0)
        {
            var progress = new Progress(o =>
            {
                lock(this)
                    _progresses.Remove(o);
            }, _yieldable)
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