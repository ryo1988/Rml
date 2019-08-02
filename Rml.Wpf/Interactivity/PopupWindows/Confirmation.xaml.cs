using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;

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
                    Debug.Assert(property != null, nameof(property) + " != null");
                    property.SetValue(null, true, null);

                    var button = VisualTreeHelperEx.FindDescendantByType<Button>(ButtonList);
                    button?.Focus();
                    break;
            }
        }
    }
}
