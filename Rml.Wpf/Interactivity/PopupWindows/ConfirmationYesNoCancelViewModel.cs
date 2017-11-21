using System;
using System.Reactive.Linq;
using Prism.Interactivity.InteractionRequest;
using Prism.Mvvm;
using Reactive.Bindings;
using Rml.Wpf.Interactivity.InteractionRequest;

namespace Rml.Wpf.Interactivity.PopupWindows
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfirmationYesNoCancelViewModel : BindableBase, IInteractionRequestAware
    {
        #region Notification

        private INotification _notification;

        /// <inheritdoc />
        public INotification Notification
        {
            get => _notification;
            set
            {
                if (SetProperty(ref _notification, value))
                {
                    RaisePropertyChanged(nameof(ConfirmationYesNoCancel));
                }
            }
        }

        #endregion

        /// <inheritdoc />
        public Action FinishInteraction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public InteractionRequest.ConfirmationYesNoCancel ConfirmationYesNoCancel => Notification as InteractionRequest.ConfirmationYesNoCancel;

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand Yes { get; }
        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand No { get; }
        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand Cancel { get; }

        /// <summary>
        /// 
        /// </summary>
        public ConfirmationYesNoCancelViewModel()
        {
            Yes = CreateCommand(ConfirmationYesNoCancelResult.Yes);
            No = CreateCommand(ConfirmationYesNoCancelResult.No);
            Cancel = CreateCommand(ConfirmationYesNoCancelResult.Cancel);
        }

        private ReactiveCommand CreateCommand(ConfirmationYesNoCancelResult result)
        {
            var command = new ReactiveCommand();
            command
                .Select(_ => ConfirmationYesNoCancel)
                .Where(o => o != null)
                .Do(o => o.Result = result)
                .Subscribe(_ => FinishInteraction());
            return command;
        }
    }
}