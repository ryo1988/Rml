using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Rml.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    public class DispatcherYieldable : IYieldable
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly DispatcherPriority _dispatcherPriority;

        /// <summary>
        ///
        /// </summary>
        /// <param name="dispatcherPriority"></param>
        public DispatcherYieldable(DispatcherPriority dispatcherPriority = DispatcherPriority.ApplicationIdle)
        {
            _dispatcherPriority = dispatcherPriority;
        }

        /// <inheritdoc />
        public async ValueTask Yield()
        {
            // UIスレッドでないなら、特に何もしない
            if (Thread.CurrentThread != Application.Current.Dispatcher.Thread)
                return;
            
            // Dispatcher.Yieldを呼びすぎるとパフォーマンスが低下するので50msほど猶予を
            lock (_stopwatch)
            {
                if (_stopwatch.IsRunning && _stopwatch.ElapsedMilliseconds < 50)
                    return;
            
                _stopwatch.Restart();
            }
            
            // UIスレッドで処理している最中にUIを強制的に更新するため
            await Dispatcher.Yield(_dispatcherPriority);
        }
    }
}