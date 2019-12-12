using System;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Rml.Wpf.DialogService
{
    /// <summary>
    /// 通知コントロール
    /// </summary>
    public class NotificationControlViewModel : DisposableBindableBase, IDialogAware
    {
        /// <inheritdoc />
        public string Title { get; private set; }

        /// <inheritdoc />
        public event Action<IDialogResult> RequestClose;

        /// <summary>
        /// 
        /// </summary>
        public object Content { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ReactiveCommand OkCommand { get; }

        /// <inheritdoc />
        public NotificationControlViewModel()
        {
            OkCommand = new ReactiveCommand().AddTo(Cd);
            OkCommand
                .Subscribe(_ => RequestClose(new DialogResult(ButtonResult.OK)))
                .AddTo(Cd);
        }

        /// <inheritdoc />
        public bool CanCloseDialog()
        {
            return true;
        }

        /// <inheritdoc />
        public void OnDialogClosed()
        {
        }

        /// <inheritdoc />
        public void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>(NotificationDialogParameters.TitleKey);
            RaisePropertyChanged(nameof(Title));
            Content = parameters.GetValue<object>(NotificationDialogParameters.ContentKey);
            RaisePropertyChanged(nameof(Content));
        }
    }
}