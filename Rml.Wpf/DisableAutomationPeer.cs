using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;

namespace Rml.Wpf
{
    /// <summary>
    /// UIオートメーションを無効化
    /// </summary>
    public class DisableAutomationPeer : FrameworkElementAutomationPeer
    {
        private static readonly List<AutomationPeer> Children = new List<AutomationPeer>();

        /// <inheritdoc />
        public DisableAutomationPeer(FrameworkElement owner) : base(owner) { }

        /// <inheritdoc />
        protected override string GetNameCore()
        {
            return nameof(DisableAutomationPeer);
        }

        /// <inheritdoc />
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Window;
        }

        /// <inheritdoc />
        protected override List<AutomationPeer> GetChildrenCore()
        {
            return Children;
        }
    }
}