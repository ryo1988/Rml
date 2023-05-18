using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Rml.Wpf.Command;

namespace Rml.Wpf.AttachedBehavior;

public static class ScrollBarOptimizer
{
    public static readonly DependencyProperty EnableOptimizeCommandProperty = DependencyProperty.RegisterAttached(
        "EnableOptimizeCommand", typeof(bool), typeof(ScrollBarOptimizer),
        new PropertyMetadata(default(bool), EnableOptimizeCommandChanged));

    public static void SetEnableOptimizeCommand(DependencyObject element, bool value)
        => element.SetValue(EnableOptimizeCommandProperty, value);

    public static bool GetEnableOptimizeCommand(DependencyObject element)
        => (bool)element.GetValue(EnableOptimizeCommandProperty);

    private static void EnableOptimizeCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is false)
            return;
        
        if (d is not ScrollBar bar)
            throw new NotSupportedException("ScrollBarのみ対応");

        if (bar.IsLoaded)
            throw new InvalidOperationException("");

        // ScrollBarのボタンはControlTemplateに定義されているので、Loaded後に作成される
        bar.Loaded += ScrollBarLoaded;
    }

    private static void ScrollBarLoaded(object sender, RoutedEventArgs e)
    {
        var bar = (ScrollBar)sender;
        
        bar.Loaded -= ScrollBarLoaded;

        // ボタンがない場合もあるので、ApplyTemplateしておく
        bar.ApplyTemplate();

        foreach (var button in GetVisualDescendants(bar).OfType<ButtonBase>())
            // スクロール対象がない場合はスクロールバー自体無効化されるので、使っているボタンは常に実行可能にしてRoutedEventsの発生を抑制
            button.Command = new AlwaysExecutableRoutedCommand((RoutedCommand)button.Command, button);
    }

    private static IEnumerable<DependencyObject> GetVisualDescendants(DependencyObject target)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(target); i++)
        {
            var child = VisualTreeHelper.GetChild(target, i);

            yield return child;

            foreach (var descendant in GetVisualDescendants(child)) yield return descendant;
        }
    }
}