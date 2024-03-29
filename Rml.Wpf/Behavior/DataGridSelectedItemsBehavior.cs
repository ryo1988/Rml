﻿using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// 
    /// </summary>
    public class DataGridSelectedItemsBehavior : Behavior<DataGrid>
    {
        /// <summary>
        ///DataGrid
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems", typeof(IList), typeof(DataGridSelectedItemsBehavior), new FrameworkPropertyMetadata(default(Array), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemsPropertyChanged));

        private static void SelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DataGridSelectedItemsBehavior)d;
            self.SelectedItemsChanged();
        }

        private void SelectedItemsChanged()
        {
            if (AssociatedObject == null)
            {
                return;
            }

            if (_internalUpdating)
            {
                return;
            }

            if (AssociatedObject.SelectedItems.OfType<object>()
                .SequenceEqual(SelectedItems?.OfType<object>() ?? Enumerable.Empty<object>()))
            {
                return;
            }

            _internalUpdating = true;
            AssociatedObject.SelectedItems.Clear();
            if (SelectedItems != null)
            {
                foreach (var item in SelectedItems)
                {
                    AssociatedObject.SelectedItems.Add(item);
                }
            }
            _internalUpdating = false;
        }

        /// <summary>
        /// 
        /// </summary>
        public IList SelectedItems
        {
            get { return (Array)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty ElementTypeProperty = DependencyProperty.Register(
            "ElementType", typeof(Type), typeof(DataGridSelectedItemsBehavior), new PropertyMetadata(default(Type), ElementPropertyChanged));

        private static void ElementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (DataGridSelectedItemsBehavior)d;
            self.UpdateSelectedItems();
        }

        /// <summary>
        /// 
        /// </summary>
        public Type ElementType
        {
            get { return (Type)GetValue(ElementTypeProperty); }
            set { SetValue(ElementTypeProperty, value); }
        }

        private bool _internalUpdating;

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += AssociatedObjectOnSelectionChanged;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= AssociatedObjectOnSelectionChanged;
        }

        private void AssociatedObjectOnSelectionChanged(object s, SelectionChangedEventArgs e)
        {
            UpdateSelectedItems();
        }
        
        private void UpdateSelectedItems()
        {
            if (AssociatedObject is null)
                return;
            
            if (ElementType is null)
                return;
            
            if (_internalUpdating)
            {
                return;
            }

            if (AssociatedObject.SelectedItems.OfType<object>()
                .SequenceEqual(SelectedItems?.OfType<object>() ?? Enumerable.Empty<object>()))
            {
                return;
            }

            var selectedItems = AssociatedObject.SelectedItems;
            var count = selectedItems.Count;
            var array = Array.CreateInstance(ElementType ?? throw new InvalidOperationException(), count);
            var index = 0;
            foreach (var item in selectedItems)
            {
                array.SetValue(item, index++);
            }

            _internalUpdating = true;
            SelectedItems = array;
            _internalUpdating = false;
        }
    }
}