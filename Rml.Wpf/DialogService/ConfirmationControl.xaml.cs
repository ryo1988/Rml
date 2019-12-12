using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Rml.Wpf.DialogService
{
    /// <summary>
    /// ConfirmationControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfirmationControl
    {
        /// <inheritdoc />
        public ConfirmationControl()
        {
            InitializeComponent();

            ((INotifyCollectionChanged)ButtonList.Items).CollectionChanged += ListBoxCollectionChanged;
        }

        /// <summary>
        /// 初期フォーカス位置
        /// </summary>
        public static readonly DependencyProperty DefaultIndexProperty = DependencyProperty.Register(
            "DefaultIndex", typeof(int), typeof(ConfirmationControl), new PropertyMetadata(default(int)));

        /// <summary>
        /// 初期フォーカス位置
        /// </summary>
        public int DefaultIndex
        {
            get { return (int) GetValue(DefaultIndexProperty); }
            set { SetValue(DefaultIndexProperty, value); }
        }

        private void ListBoxCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    // FocusVisualStyleを常に表示する
                    var property = typeof(KeyboardNavigation).GetProperty("AlwaysShowFocusVisual", BindingFlags.NonPublic | BindingFlags.Static);
                    Debug.Assert(property != null, nameof(property) + " != null");
                    property.SetValue(null, true, null);

                    var item = (FrameworkElement)ButtonList.ItemContainerGenerator.ContainerFromIndex(DefaultIndex);
                    var button = VisualTreeHelperEx.FindDescendantByType<Button>(item);
                    button?.Focus();
                    break;
            }
        }
    }
}
