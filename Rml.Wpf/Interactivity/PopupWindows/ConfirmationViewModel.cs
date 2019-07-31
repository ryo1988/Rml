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
    public class ConfirmationItemViewModel : DisposableBindableBase
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
        public ConfirmationItemViewModel(object label, int index)
        {
            Label = label;
            ExecuteCommand = new ReactiveCommand().AddTo(Cd);
            Index = index;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConfirmationViewModel : DisposableBindableBase, IInteractionRequestAware
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
                    RaisePropertyChanged(nameof(Confirmation));
                }
            }
        }

        #endregion

        /// <inheritdoc />
        public Action FinishInteraction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public InteractionRequest.Confirmation Confirmation => Notification as InteractionRequest.Confirmation;

        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyReactiveProperty<ConfirmationItemViewModel[]> Items { get; }

        /// <summary>
        /// 
        /// </summary>
        public ConfirmationViewModel()
        {
            Items = this.ObserveProperty(o => o.Confirmation) 
                .Select(o => o?.LabelList.Select((oo, i) => new ConfirmationItemViewModel(oo, i)).ToArray())
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
                .Do(_ => Confirmation.ResultIndex = index)
                .Subscribe(_ => FinishInteraction());
        }
    }
}