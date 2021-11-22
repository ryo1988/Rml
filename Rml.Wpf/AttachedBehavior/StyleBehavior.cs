using System.Linq;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.AttachedBehavior
{
    /// <summary>
    /// StyleでBehaviorを使えるように
    /// </summary>
    public class StyleBehavior : FreezableCollection<Microsoft.Xaml.Behaviors.Behavior>
    {
        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached(
            "Behaviors", typeof(StyleBehavior), typeof(StyleBehavior), new PropertyMetadata(default(StyleBehavior), BehaviorsChanged));

        private static void BehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            UpdateBehaviors(d);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetBehaviors(DependencyObject element, StyleBehavior value)
        {
            element.SetValue(BehaviorsProperty, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static StyleBehavior GetBehaviors(DependencyObject element)
        {
            return (StyleBehavior) element.GetValue(BehaviorsProperty);
        }

        /// <summary>
        /// Behaviorをクローンするかどうか
        /// </summary>
        public static readonly DependencyProperty IsCloneProperty = DependencyProperty.RegisterAttached(
            "IsClone", typeof(bool), typeof(StyleBehavior), new PropertyMetadata(default(bool), IsCloneChanged));

        private static void IsCloneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            UpdateBehaviors(d);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetIsClone(DependencyObject element, bool value)
        {
            element.SetValue(IsCloneProperty, value);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool GetIsClone(DependencyObject element)
        {
            return (bool) element.GetValue(IsCloneProperty);
        }

        private static void UpdateBehaviors(DependencyObject dependencyObject)
        {
            var styleBehaviors = GetBehaviors(dependencyObject);

            if (styleBehaviors is null)
                return;

            var behaviors = Interaction.GetBehaviors(dependencyObject);

            if (behaviors.SequenceEqual(styleBehaviors))
                return;

            behaviors.Clear();
            var isClone = GetIsClone(dependencyObject);
            foreach (var value in styleBehaviors)
            {
                var behavior = value;

                if (isClone)
                    behavior = (Microsoft.Xaml.Behaviors.Behavior)behavior.Clone();

                behaviors.Add(behavior);
            }
        }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new StyleBehavior();
        }
    }
}