using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// SelectionUnit="CellOrRowHeader" の時の選択されたアイテムを取得します
    /// </summary>
    public class DataGridSelectedItemBehavior : Behavior<DataGrid>
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems", typeof(object[]), typeof(DataGridSelectedItemBehavior), new PropertyMetadata(default(object[])));

        /// <summary>
        /// 選択されたアイテム
        /// </summary>
        public object[] SelectedItems
        {
            get { return (object[]) GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectedCellsChanged += OnSelectedCellsChanged;
        }

        private void OnSelectedCellsChanged(object s, SelectedCellsChangedEventArgs e)
        {
            SelectedItems = AssociatedObject.SelectedCells
                .Select(o => o.Item)
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.SelectedCellsChanged -= OnSelectedCellsChanged;

            base.OnDetaching();
        }
    }
}