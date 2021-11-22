using System.Reactive.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Rml.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    public class ProgressViewModel : DisposableBindableBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyReactiveProperty<string> Label { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyReactiveProperty<bool> IsIndeterminate { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyReactiveProperty<int> Max { get; }
        
        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyReactiveProperty<int> Current { get; }

        /// <summary>
        ///
        /// </summary>
        public ReactiveCommand CancelCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="progress"></param>
        public ProgressViewModel(Progress progress)
        {
            Label = progress.Label
                .Sample(TimeSpan.FromMilliseconds(50))
                .ToReadOnlyReactiveProperty()
                .AddTo(Cd);
            IsIndeterminate = progress.IsIndeterminate
                .Sample(TimeSpan.FromMilliseconds(50))
                .ToReadOnlyReactiveProperty()
                .AddTo(Cd);
            Max = progress.Max
                .Sample(TimeSpan.FromMilliseconds(50))
                .ToReadOnlyReactiveProperty()
                .AddTo(Cd);
            Current = progress.Current
                .Sample(TimeSpan.FromMilliseconds(50))
                .ToReadOnlyReactiveProperty()
                .AddTo(Cd);
            CancelCommand = progress.CanCancel ? new ReactiveCommand().AddTo(Cd) : null;
            CancelCommand?
                .Subscribe(_ => progress.Cancel())
                .AddTo(Cd);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class ProgressControlViewModel : DisposableBindableBase
    {
        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyReactiveCollection<ProgressViewModel> Progresses { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public ProgressControlViewModel(IProgressService service)
        {
            Progresses = service.GetProgresses()
                .ToReadOnlyReactiveCollection(o => new ProgressViewModel(o))
                .AddTo(Cd);
        }
    }
}