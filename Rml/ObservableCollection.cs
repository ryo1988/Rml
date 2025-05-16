using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Rml
{
    /// <summary>
    /// AddRangeを追加したもの
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {
        private bool _suppressNotification;

        /// <inheritdoc />
        public ObservableCollection()
        {
        }

        /// <inheritdoc />
        public ObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        /// <inheritdoc />
        public ObservableCollection(List<T> collection)
            : base(collection)
        {
        }

        /// <inheritdoc />
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_suppressNotification)
                base.OnCollectionChanged(e);
        }

        /// <summary>
        /// 全ての要素を追加後にNotifyCollectionChangedAction.Resetを行います
        /// </summary>
        /// <param name="list"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddRange(IEnumerable<T> list)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));
            
            var changed = false;

            _suppressNotification = true;

            foreach (var item in list)
            {
                Add(item);
                changed = true;
            }

            _suppressNotification = false;

            if (changed)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
        
        /// <summary>
        /// 全ての要素を削除後にNotifyCollectionChangedAction.Resetを行います
        /// </summary>
        /// <param name="list"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void RemoveRange(IEnumerable<T> list)
        {
            if (list is null)
                throw new ArgumentNullException(nameof(list));
            
            var changed = false;

            _suppressNotification = true;

            foreach (var item in list)
            {
                Remove(item);
                changed = true;
            }

            _suppressNotification = false;

            if (changed)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}