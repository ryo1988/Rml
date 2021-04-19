using System.Windows.Controls;

namespace Rml.Wpf
{
    /// <summary>
    ///
    /// </summary>
    public static class TreeViewExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static TreeViewItem GetTreeViewItem(this ItemsControl container, object item)
        {
            if (container == null) return null;

            if (container.DataContext == item)
            {
                return container as TreeViewItem;
            }

            if (container is TreeViewItem treeViewItem && !treeViewItem.IsExpanded)
            {
                treeViewItem.SetValue(TreeViewItem.IsExpandedProperty, true);
            }

            container.ApplyTemplate();

            for (int i = 0, count = container.Items.Count; i < count; i++)
            {
                var subContainer = (TreeViewItem) container.ItemContainerGenerator.ContainerFromIndex(i);

                subContainer.BringIntoView();

                var resultContainer = GetTreeViewItem(subContainer, item);
                if (resultContainer != null)
                {
                    return resultContainer;
                }

                subContainer.IsExpanded = false;
            }

            return null;
        }
    }
}