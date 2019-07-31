using System;
using System.Linq;
using System.Reactive.Linq;
using Prism.Interactivity.InteractionRequest;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Rml.Wpf.Interactivity.PopupWindows
{
    /// <summary>
    /// 
    /// </summary>
    public class ConfirmationYesNoCanceItemlViewModel : DisposableBindableBase
    {
        /// <summary>
        /// 
        /// </summary>
        public object Label { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand ExecuteCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 
        /// </summary>
        public ConfirmationYesNoCanceItemlViewModel(object label, int index)
        {
            Label = label;
            ExecuteCommand = new ReactiveCommand().AddTo(Cd);
            Index = index;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConfirmationYesNoCancelViewModel : DisposableBindableBase, IInteractionRequestAware
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
        public ReadOnlyReactiveProperty<ConfirmationYesNoCanceItemlViewModel[]> Items { get; }

        /// <summary>
        /// 
        /// </summary>
        public ConfirmationYesNoCancelViewModel()
        {
            Items = this.ObserveProperty(o => o.ConfirmationYesNoCancel) 
                .Select(o => o?.LabelList.Select((oo, i) => new ConfirmationYesNoCanceItemlViewModel(oo, i)).ToArray())
                .DisposeBefore()
                .ToReadOnlyReactiveProperty()
                .AddTo(Cd);

            Items
                .Select(o => o?.Select(oo => BindCommand(oo.ExecuteCommand, oo.Index)).ToArray())
                .DisposeBefore()
                .Subscribe()
                .AddTo(Cd);
        }

        private IDisposable BindCommand(ReactiveCommand command, int index)
        {
            return command
                .Do(_ => ConfirmationYesNoCancel.ResultIndex = index)
                .Subscribe(_ => FinishInteraction());
        }
    }
}