using System;
using System.Threading.Tasks;
using Prism.Interactivity.InteractionRequest;

namespace Rml.Wpf.Interactivity.InteractionRequest
{
    /// <inheritdoc />
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AsyncInteractionRequest<T> : IInteractionRequest where T : INotification
    {
        /// <inheritdoc />
        public event EventHandler<InteractionRequestedEventArgs> Raised;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<T> Raise(T context)
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            Raised?.Invoke(this, new InteractionRequestedEventArgs(context, () => taskCompletionSource.SetResult(context)));
            return taskCompletionSource.Task;
        }
    }
}