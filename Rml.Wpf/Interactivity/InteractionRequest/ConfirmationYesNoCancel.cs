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
        public object YesLabel { get; set; } = "Yes";

        /// <summary>
        /// 
        /// </summary>
        public object NoLabel { get; set; } = "No";

        /// <summary>
        /// 
        /// </summary>
        public object CancelLabel { get; set; } = "Cancel";

        /// <summary>
        /// 
        /// </summary>
        public ConfirmationYesNoCancelResult Result { get; set; }
    }
}