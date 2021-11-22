using System.Reactive.Concurrency;
using System.Reflection;
using System.Windows.Threading;
using Reactive.Bindings;

namespace Rml.Wpf
{
    /// <summary>
    /// 
    /// </summary>
    public static class UiDispatcherSchedulerHelper
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
        public static void ScheduleUiDispatcher(Action action)
        {
            var isSchedulerCreated = (bool)GetFieldValue(typeof(UIDispatcherScheduler), null, "IsSchedulerCreated");
            if (isSchedulerCreated || SynchronizationContext.Current != null)
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