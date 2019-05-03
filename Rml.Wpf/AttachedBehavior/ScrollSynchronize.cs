using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Rml.Wpf.AttachedBehavior
{
    /// <summary>
    /// スクロール同期
    /// </summary>
    public class ScrollSynchronize
    {
        private static readonly Dictionary<ScrollViewer, string> Groups = new Dictionary<ScrollViewer, string>();
        private static readonly Dictionary<string, List<ScrollViewer>> ScrollViewers = new Dictionary<string, List<ScrollViewer>>();

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty ScrollGroupProperty = DependencyProperty.RegisterAttached(
            "ScrollGroup", typeof(string), typeof(ScrollSynchronize), new PropertyMetadata(default(string), ScrollGroupChanged));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetScrollGroup(DependencyObject element, string value)
        {
            element.SetValue(ScrollGroupProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetScrollGroup(DependencyObject element)
        {
            return (string) element.GetValue(ScrollGroupProperty);
        }

        private static void ScrollGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldGroup = e.OldValue as string;
            var newGroup = e.NewValue as string;

            var scrollViewer = d as ScrollViewer ?? VisualTreeHelperEx.FindDescendantByType<ScrollViewer>((Visual)d);

            if (scrollViewer is null)
            {
                return;
            }

            ScrollGroupChanged(scrollViewer);

            void ScrollGroupChanged(ScrollViewer changedScrollViewer)
            {
                if (string.IsNullOrEmpty(oldGroup) == false)
                {
                    ScrollViewers[oldGroup].Remove(changedScrollViewer);
                    Groups.Remove(changedScrollViewer);
                    changedScrollViewer.ScrollChanged -= ScrollViewerOnScrollChanged;
                }

                if (string.IsNullOrEmpty(newGroup) == false)
                {
                    changedScrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
                    if (ScrollViewers.TryGetValue(newGroup, out var scrollViewers) == false)
                    {
                        scrollViewers = new List<ScrollViewer>();
                        ScrollViewers.Add(newGroup, scrollViewers);
                    }
                    scrollViewers.Add(changedScrollViewer);
                    Groups.Add(changedScrollViewer, newGroup);
                }

                SyncScroll(newGroup, changedScrollViewer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty EnableVerticalProperty = DependencyProperty.RegisterAttached(
            "EnableVertical", typeof(bool), typeof(ScrollSynchronize), new PropertyMetadata(true));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetEnableVertical(DependencyObject element, bool value)
        {
            element.SetValue(EnableVerticalProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool GetEnableVertical(DependencyObject element)
        {
            return (bool) element.GetValue(EnableVerticalProperty);
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty EnableHorizontalProperty = DependencyProperty.RegisterAttached(
            "EnableHorizontal", typeof(bool), typeof(ScrollSynchronize), new PropertyMetadata(true));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetEnableHorizontal(DependencyObject element, bool value)
        {
            element.SetValue(EnableHorizontalProperty, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool GetEnableHorizontal(DependencyObject element)
        {
            return (bool) element.GetValue(EnableHorizontalProperty);
        }

        private static void ScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            var group = Groups[scrollViewer];
            SyncScroll(group, scrollViewer);
        }

        private static void SyncScroll(string group, ScrollViewer scrollViewer)
        {
            foreach (var viewer in ScrollViewers[group].Where(o => o != scrollViewer))
            {
                if (GetEnableHorizontal(viewer))
                {
                    viewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset);
                }

                if (GetEnableVertical(viewer))
                {
                    viewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
                }
            }
        }
    }
}