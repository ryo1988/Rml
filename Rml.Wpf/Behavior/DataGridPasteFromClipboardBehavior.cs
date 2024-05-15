using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.Behavior;

public record struct DataGridPasteFromClipboardPasteEventArgs(
    object Item,
    int RowIndex,
    int ColumnIndex,
    string Value);

public delegate void DataGridPasteFromClipboardPasteEventHandler(
    object sender,
    DataGridPasteFromClipboardPasteEventArgs e);

public class DataGridPasteFromClipboardBehavior : Behavior<DataGrid>
{
    public event DataGridPasteFromClipboardPasteEventHandler Paste;
    
    private static readonly char[] Separator = ['\r', '\n'];
    
    private readonly CommandBinding _commandBinding;
    private readonly KeyBinding _keyBinding;

    public DataGridPasteFromClipboardBehavior()
    {
        _commandBinding = new CommandBinding
        {
            Command = ApplicationCommands.Paste,
        };
        _commandBinding.Executed += PasteExecute;
        _commandBinding.CanExecute += PasteCanExecute;

        _keyBinding = new KeyBinding
        {
            Command = ApplicationCommands.Paste,
            Key = Key.V,
            Modifiers = ModifierKeys.Control,
        };
    }

    private void PasteExecute(object sender, ExecutedRoutedEventArgs e)
    {
        PasteFromClipboard(sender);
    }
    
    private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = Clipboard.ContainsText();
    }
    
    private void PasteFromClipboard(object sender)
    {
        if (Paste is null)
            return;
        
        var clipboardText = Clipboard.GetText();
        var rows = clipboardText.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

        if (AssociatedObject.SelectedCells.Count == 0)
        {
            MessageBox.Show("少なくとも1つのセルを選択してペーストしてください");
            return;
        }

        var startCell = AssociatedObject.SelectedCells[0];
        var startRowIndex = AssociatedObject.Items.IndexOf(startCell.Item);
        var startColumnIndex = startCell.Column.DisplayIndex;

        for (var i = 0; i < rows.Length; i++)
        {
            var rowCells = rows[i].Split('\t');
            for (var j = 0; j < rowCells.Length; j++)
            {
                var rowIndex = startRowIndex + i;
                var columnIndex = startColumnIndex + j;

                if (rowIndex >= AssociatedObject.Items.Count || columnIndex >= AssociatedObject.Columns.Count)
                {
                    continue;
                }

                var item = AssociatedObject.Items[rowIndex];

                Paste(sender, new DataGridPasteFromClipboardPasteEventArgs
                {
                    Item = item,
                    RowIndex = rowIndex,
                    ColumnIndex = columnIndex,
                    Value = rowCells[j],
                });
            }
        }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.CommandBindings.Add(_commandBinding);
        AssociatedObject.InputBindings.Add(_keyBinding);
    }

    protected override void OnDetaching()
    {
        AssociatedObject.InputBindings.Remove(_keyBinding);
        AssociatedObject.CommandBindings.Remove(_commandBinding);
        
        base.OnDetaching();
    }
}