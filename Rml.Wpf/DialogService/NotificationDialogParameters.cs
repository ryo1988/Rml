using Prism.Regions;
using Prism.Services.Dialogs;

namespace Rml.Wpf.DialogService
{
    /// <summary>
    /// 通知ダイアログパラメータ
    /// </summary>
    public class NotificationDialogParameters : NavigationParameters, IDialogParameters
    {
        internal const string TitleKey = "Title";
        internal const string ContentKey = "Content";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public NotificationDialogParameters(string title, object content)
        {
            Add(TitleKey, title);
            Add(ContentKey, content);
        }
    }
}