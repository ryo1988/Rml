using Prism.Regions;
using Prism.Services.Dialogs;

namespace Rml.Wpf.DialogService
{
    /// <summary>
    /// 確認ダイアログパラメータ
    /// </summary>
    public class ConfirmationDialogParameters : NavigationParameters, IDialogParameters
    {
        internal const string TitleKey = "Title";
        internal const string ContentKey = "Content";
        internal const string ChoicesKey = "Choices";
        internal const string DefaultIndexKey = "DefaultIndex";
        internal const string ResultIndexKey = "ResultIndex";

        /// <summary>
        /// 
        /// </summary>
        public int ResultIndex => TryGetValue<int>(ResultIndexKey, out var value) ? value : -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="choices"></param>
        /// <param name="defaultIndex"></param>
        public ConfirmationDialogParameters(string title, object content, object[] choices, int defaultIndex)
        {
            Add(TitleKey, title);
            Add(ContentKey, content);
            Add(ChoicesKey, choices);
            Add(DefaultIndexKey, defaultIndex);
        }
    }
}