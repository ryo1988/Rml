using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Rml.Wpf.DialogService
{
    /// <summary>
    /// NotificationControl.xaml の相互作用ロジック
    /// </summary>
    public partial class NotificationControl
    {
        /// <inheritdoc />
        public NotificationControl()
        {
            InitializeComponent();

            // FocusVisualStyleを常に表示する
            var property = typeof(KeyboardNavigation).GetProperty("AlwaysShowFocusVisual", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(property != null, nameof(property) + " != null");
            property.SetValue(null, true, null);
        }
    }
}
