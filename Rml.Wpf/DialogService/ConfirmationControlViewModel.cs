﻿using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Rml.Wpf.DialogService
{
    /// <summary>
    /// 確認コントロール
    /// </summary>
    public class ConfirmationControlViewModel : DisposableBindableBase, IDialogAware
    {
        /// <inheritdoc />
        public string Title { get; private set; }

        /// <inheritdoc />
        public event Action<IDialogResult> RequestClose;

        /// <summary>
        /// 
        /// </summary>
        public object Content { get; private set; }
        
        public ReactiveCommand CopyToClipboardCommand { get; }

        /// <summary>
        /// 
        /// </summary>
        public ConfirmationChoiceViewModel[] Choices { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int DefaultIndex { get; private set; }

        private CompositeDisposable _cd;

        public ConfirmationControlViewModel()
        {
            CopyToClipboardCommand = new ReactiveCommand().AddTo(Cd);
            CopyToClipboardCommand
                .Subscribe(_ =>
                {
                    var choices = Choices?
                        .Select(o => o.Label)
                        .ToArray() ?? Array.Empty<string>();
                    Clipboard.SetText($"{Title}{Environment.NewLine}{Content}{Environment.NewLine}[{string.Join("][", choices)}]");
                })
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
            _cd?.Dispose();
            _cd = null;
        }

        /// <inheritdoc />
        public void OnDialogOpened(IDialogParameters parameters)
        {
            _cd = new CompositeDisposable();

            Title = parameters.GetValue<string>(ConfirmationDialogParameters.TitleKey);
            RaisePropertyChanged(nameof(Title));
            Content = parameters.GetValue<object>(NotificationDialogParameters.ContentKey);
            RaisePropertyChanged(nameof(Content));
            Choices = parameters
                .GetValue<object[]>(ConfirmationDialogParameters.ChoicesKey)
                .Select((o, i) => new ConfirmationChoiceViewModel(o, i))
                .Do(o => o.AddTo(_cd))
                .ToArray();
            DefaultIndex = parameters.GetValue<int>(ConfirmationDialogParameters.DefaultIndexKey);
            RaisePropertyChanged(nameof(DefaultIndex));
            Choices
                .ForEach(o => BindCommand(o.ExecuteCommand, o.Index, parameters).AddTo(_cd));
            RaisePropertyChanged(nameof(Choices));
        }

        private IDisposable BindCommand(ReactiveCommand command, int index, IDialogParameters parameters)
        {
            return command
                .Do(_ => parameters.Add(ConfirmationDialogParameters.ResultIndexKey, index))
                .Subscribe(_ => RequestClose(new DialogResult(ButtonResult.OK, parameters)));
        }
    }

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
}