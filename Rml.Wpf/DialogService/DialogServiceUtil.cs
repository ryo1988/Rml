using System;
using System.Threading;
using System.Windows;
using Prism.Services.Dialogs;

namespace Rml.Wpf.DialogService
{
    /// <summary>
    ///
    /// </summary>
    public static class DialogServiceUtil
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="action"></param>
        public static void InvokeUiThread(Action action)
        {
            if (Application.Current.Dispatcher != null && Application.Current.Dispatcher.Thread != Thread.CurrentThread)
            {
                Exception invokeException = null;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception e)
                    {
                        invokeException = e;
                    }
                });
                if (invokeException is not null)
                    throw new System.Reflection.TargetInvocationException(invokeException);
            }
            else
            {
                action();
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void Notification(this IDialogService dialogService, string title, object content)
        {
            InvokeUiThread(() => dialogService.ShowDialog(nameof(NotificationControl), new NotificationDialogParameters(title, content), null));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dialogService"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="choices"></param>
        /// <param name="defaultIndex"></param>
        /// <returns></returns>
        public static int Confirmation(this IDialogService dialogService, string title, object content, object[] choices, int defaultIndex)
        {
            var result = -1;
            InvokeUiThread(() => dialogService.ShowDialog(nameof(ConfirmationControl), new ConfirmationDialogParameters(title, content, choices, defaultIndex),
                o => result = o.Parameters switch
                {
                    ConfirmationDialogParameters confirmationDialogParameters => confirmationDialogParameters
                        .ResultIndex,
                    _ => result
                }));

            return result;
        }

        public static T GetValue<T>(this IDialogResult dialogResult, string propertyName)
        {
            return dialogResult.Parameters?.TryGetValue<T>(propertyName, out var parameter) ?? false
                ? parameter
                : default;
        }
    }
}