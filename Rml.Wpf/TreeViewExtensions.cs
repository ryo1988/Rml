using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;

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
            if (container is null) return null;

            if (container.DataContext == item)
            {
                return container as TreeViewItem;
            }

            if (container is TreeViewItem treeViewItem && !treeViewItem.IsExpanded)
            {
                treeViewItem.SetValue(TreeViewItem.IsExpandedProperty, true);
            }

            container.ApplyTemplate();

            var itemsPresenter = (ItemsPresenter)container.Template.FindName("ItemsHost", container);
            if (itemsPresenter is not null)
            {
                itemsPresenter.ApplyTemplate();
            }
            else
            {
                itemsPresenter = VisualTreeHelperEx.FindDescendantByType<ItemsPresenter>(container);
                if (itemsPresenter == null)
                {
                    container.UpdateLayout();

                    itemsPresenter = VisualTreeHelperEx.FindDescendantByType<ItemsPresenter>(container);
                }
            }

            var itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

            var virtualizingPanel = itemsHostPanel as VirtualizingStackPanel;

            for (int i = 0, count = container.Items.Count; i < count; i++)
            {
                TreeViewItem subContainer;
                if (virtualizingPanel is not null)
                {
                    virtualizingPanel.BringIndexIntoViewPublic(i);

                    subContainer =
                        (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                }
                else
                {
                    subContainer =
                        (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                    subContainer?.BringIntoView();
                }

                if (subContainer is null) continue;

                var resultContainer = GetTreeViewItem(subContainer, item);
                if (resultContainer is not null)
                {
                    return resultContainer;
                }

                subContainer.IsExpanded = false;
            }

            return null;
        }
    }
}