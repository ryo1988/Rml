using System;
using System.Reactive.Concurrency;
using System.Reflection;
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
        private static object GetFieldValue(Type type, object instance, string fieldName)
        {
            var bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                     | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public static void ScheduleUIDispatcher(Action action)
        {
            var isSchedulerCreated = (bool)GetFieldValue(typeof(UIDispatcherScheduler), null, "IsSchedulerCreated");
            if (isSchedulerCreated)
            {
                UIDispatcherScheduler.Default.Schedule(action);
            }
            else
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