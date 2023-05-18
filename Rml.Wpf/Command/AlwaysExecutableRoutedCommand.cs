using System;
using System.Windows;
using System.Windows.Input;

namespace Rml.Wpf.Command;

public sealed class AlwaysExecutableRoutedCommand : ICommand
{
    private readonly RoutedCommand _originalCommand;
    private readonly IInputElement _target;

    public AlwaysExecutableRoutedCommand(RoutedCommand originalCommand, IInputElement target)
    {
        _originalCommand = originalCommand ?? throw new ArgumentNullException(nameof(originalCommand));
        _target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public event EventHandler CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter) => _originalCommand.Execute(parameter, _target);
}