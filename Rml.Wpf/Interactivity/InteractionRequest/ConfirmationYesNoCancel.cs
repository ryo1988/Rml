using System.Collections.Generic;
using Prism.Interactivity.InteractionRequest;

namespace Rml.Wpf.Interactivity.InteractionRequest
{
    /// <summary>
    /// 
    /// </summary>
    public enum ConfirmationYesNoCancelResult
    {
        /// <summary>
        /// 
        /// </summary>
        Cancel,
        /// <summary>
        /// 
        /// </summary>
        Yes,
        /// <summary>
        /// 
        /// </summary>
        No,
    }

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    public class ConfirmationYesNoCancel : INotification
    {
        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public object Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<object> LabelList { get; set; } = new List<object>();

        /// <summary>
        /// 
        /// </summary>
        public int ResultIndex { get; set; } = -1;
    }
}