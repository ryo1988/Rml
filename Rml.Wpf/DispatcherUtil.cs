using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Rml.Wpf;

public static class DispatcherUtil
{
    public static T WaitAndPushDispatcher<T>(this Task<T> task, DispatcherPriority priority = DispatcherPriority.Background)
    {
        WaitAndPushDispatcher(task as Task);

        return task.Result;
    }

    public static void WaitAndPushDispatcher(this Task task, DispatcherPriority priority = DispatcherPriority.Background)
    {
        var spinWait = new SpinWait();

        var complited = false;
        task.ContinueWith(_ => complited = true);
        while (complited is false)
        {
            Dispatcher.CurrentDispatcher.Invoke(() => { }, priority);
            spinWait.SpinOnce();
        }
    }
}