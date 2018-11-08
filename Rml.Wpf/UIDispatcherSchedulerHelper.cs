using System;
using System.Reactive.Concurrency;
using System.Threading;
using System.Windows.Threading;
using Reactive.Bindings;

namespace Rml.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    public static class UIDispatcherSchedulerHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public static void ScheduleUIDispatcher(Action action)
        {
            try
            {
                UIDispatcherScheduler.Default.Schedule(action);
            }
            catch
            {
                var thread = new Thread(() =>
                {
                    var dispatcherSynchronizationContext = new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher);
                    SynchronizationContext.SetSynchronizationContext(dispatcherSynchronizationContext);

                    UIDispatcherScheduler.Default.Schedule(action);

                    Dispatcher.Run();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }
    }
}