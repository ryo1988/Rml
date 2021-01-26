using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Rml.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    public class DispatcherYieldable : IYieldable
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <inheritdoc />
        public async ValueTask Yield()
        {
            // UIスレッドでないなら、特に何もしない
            if (Dispatcher.FromThread(Thread.CurrentThread) is null)
                return;
            
            // Dispatcher.Yieldを呼びすぎるとパフォーマンスが低下するので50msほど猶予を
            lock (_stopwatch)
            {
                if (_stopwatch.IsRunning && _stopwatch.ElapsedMilliseconds < 50)
                    return;
            
                _stopwatch.Restart();
            }
            
            // UIスレッドで処理している最中にUIを強制的に更新するため
            await Dispatcher.Yield(DispatcherPriority.Background);
        }
    }
}