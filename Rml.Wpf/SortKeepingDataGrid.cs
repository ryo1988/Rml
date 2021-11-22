using System.Collections;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rml.Wpf
{
    /// <summary>
    ///
    /// </summary>
    public class SortKeepingDataGrid : DataGrid
    {
        private readonly Dictionary<object, SortDescription[]> _mSortDescriptions = new();

        /// <inheritdoc />
        protected override void OnSorting(DataGridSortingEventArgs eventArgs)
        {
            base.OnSorting(eventArgs);

            var collectionView = CollectionViewSource.GetDefaultView(ItemsSource);
            _mSortDescriptions[ItemsSource] = collectionView.SortDescriptions.ToArray();
        }

        /// <inheritdoc />
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            var collectionView = CollectionViewSource.GetDefaultView(newValue);

            if (_mSortDescriptions.ContainsKey(newValue) is false)
                _mSortDescriptions[newValue] = collectionView.SortDescriptions.ToArray();

            collectionView.SortDescriptions.Clear();

            if (_mSortDescriptions.ContainsKey(newValue) is false) return;

            foreach (var sortDescription in _mSortDescriptions[newValue])
            {
                collectionView.SortDescriptions.Add(sortDescription);

                var column = Columns.FirstOrDefault(c => c.SortMemberPath == sortDescription.PropertyName);
                if (column is null) continue;

                column.SortDirection = sortDescription.Direction;
            }
        }
    }
}