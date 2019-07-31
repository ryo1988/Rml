using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Rml.Wpf.Interactivity.PopupWindows
{
    /// <summary>
    /// ConfirmationYesNoCancel.xaml の相互作用ロジック
    /// </summary>
    public partial class Confirmation
    {
        /// <summary>
        /// 
        /// </summary>
        public Confirmation()
        {
            InitializeComponent();

            ((INotifyCollectionChanged)ButtonList.Items).CollectionChanged += ListBoxCollectionChanged;
        }

        private void ListBoxCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    // FocusVisualStyleを常に表示する
                    var property = typeof(KeyboardNavigation).GetProperty("AlwaysShowFocusVisual", BindingFlags.NonPublic | BindingFlags.Static);
                    property.SetValue(null, true, null);

                    // ListBoxの先頭を選択
                    ButtonList.SelectedItem = ButtonList.Items[0];
                    ButtonList.UpdateLayout();

                    // DataTemplate内のButton要素を取り出す
                    var listBoxItem = (ListBoxItem)ButtonList
                        .ItemContainerGenerator
                        .ContainerFromItem(ButtonList.SelectedItem);
                    var contentPresenter = FindVisualChild<ContentPresenter>(listBoxItem);
                    var button = (Button)contentPresenter.ContentTemplate.FindName("Button", contentPresenter);

                    // フォーカスする
                    button.Focus();

                    // ListBoxの選択を解除
                    ButtonList.SelectedItem = null;
                    ButtonList.UpdateLayout();
                    break;
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }
    }
}
