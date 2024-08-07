﻿using System.Collections.Generic;
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

            var scrollViewer = d as ScrollViewer;

            if (scrollViewer is null)
            {
                scrollViewer = GetScrollViewer(d as Visual);
                if (scrollViewer is null)
                {
                    return;
                }
            }

            Do(scrollViewer);

            void Do(ScrollViewer changedScrollViewer)
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

                    SyncScroll(newGroup, changedScrollViewer, true, true);
                }
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
        
        public static readonly DependencyProperty IgnoreLoadedBeforeScrollProperty = DependencyProperty.RegisterAttached(
            "IgnoreLoadedBeforeScroll", typeof(bool), typeof(ScrollSynchronize), new PropertyMetadata(false));
        
        public static void SetIgnoreLoadedBeforeScroll(DependencyObject element, bool value)
        {
            element.SetValue(IgnoreLoadedBeforeScrollProperty, value);
        }
        
        public static bool GetIgnoreLoadedBeforeScroll(DependencyObject element)
        {
            return (bool) element.GetValue(IgnoreLoadedBeforeScrollProperty);
        }

        private static void ScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;
            var group = Groups[scrollViewer];
            var scrollViewers = ScrollViewers[group];
            // ロード前にスクロール位置を初期位置に戻そうとされる場合に対処
            if (GetIgnoreLoadedBeforeScroll(scrollViewer) && scrollViewer.IsLoaded is false)
            {
                scrollViewer = scrollViewers.FirstOrDefault(o => o != scrollViewer) ?? scrollViewer;
            }
            scrollViewers.ForEach(o => o.ScrollChanged -= ScrollViewerOnScrollChanged);
            SyncScroll(group, scrollViewer, e.HorizontalChange != 0.0, e.VerticalChange != 0.0);
            scrollViewers.ForEach(o => o.ScrollChanged += ScrollViewerOnScrollChanged);
        }

        private static void SyncScroll(string group, ScrollViewer scrollViewer, bool isHorizontal, bool isVertical)
        {
            var hOffset = scrollViewer.HorizontalOffset;
            if (scrollViewer.Tag is double taggedOffset)
            {
                // タグにセットしたオフセットを再適用して謎のズレを回避する
                hOffset = taggedOffset;
                scrollViewer.ScrollToHorizontalOffset(hOffset);
                scrollViewer.Tag = null;
            }

            foreach (var viewer in ScrollViewers[group].Where(o => o != scrollViewer))
            {
                if (isHorizontal && GetEnableHorizontal(viewer))
                {
                    viewer.ScrollToHorizontalOffset(hOffset);
                }

                if (isVertical && GetEnableVertical(viewer))
                {
                    viewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset);
                }
            }
        }

        private static ScrollViewer GetScrollViewer(Visual visual)
        {
            return VisualTreeHelperEx.FindDescendantByType<ScrollViewer>(visual);
        }
    }
}