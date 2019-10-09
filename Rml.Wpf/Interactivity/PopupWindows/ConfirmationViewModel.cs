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
    public class ConfirmationChoiceViewModel : DisposableBindableBase
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
        public ConfirmationChoiceViewModel(object label, int index)
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
        public ReadOnlyReactiveProperty<ConfirmationChoiceViewModel[]> Choices { get; }

        /// <summary>
        /// 
        /// </summary>
        public ReadOnlyReactiveProperty<int> DefaultIndex { get; }

        /// <summary>
        /// 
        /// </summary>
        public ConfirmationViewModel()
        {
            Choices = this.ObserveProperty(o => o.Confirmation) 
                .Select(o => o?.Choices?.Select((oo, i) => new ConfirmationChoiceViewModel(oo, i)).ToArray())
                .DisposeBefore()
                .ToReadOnlyReactiveProperty()
                .AddTo(Cd);

            Choices
                .Select(o => o?.Select(oo => BindCommand(oo.ExecuteCommand, oo.Index)).ToArray())
                .DisposeBefore()
                .Subscribe()
                .AddTo(Cd);

            DefaultIndex = this.ObserveProperty(o => o.Confirmation)
                .Select(o => o?.DefaultIndex ?? 0)
                .ToReadOnlyReactiveProperty()
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