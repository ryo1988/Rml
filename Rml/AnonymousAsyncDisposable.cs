using System;
using System.Threading.Tasks;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public class AnonymousAsyncDisposable : IDisposable, IAsyncDisposable
    {
        private readonly Action _action;
        private readonly Func<ValueTask> _func;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public AnonymousAsyncDisposable(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="func"></param>
        public AnonymousAsyncDisposable(Func<ValueTask> func)
        {
            _func = func;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_action is null)
            {
                _func().GetAwaiter().GetResult();
                return;
            }

            _action();
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            if (_func is null)
            {
                _action();
                return new ValueTask();
            }

            return _func();
        }
    }
}