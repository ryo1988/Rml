using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using Prism.Events;

namespace Rml.Wpf.Behavior
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class EventAggregatorBehavior<T> : Behavior<T> where T : DependencyObject
    {
        /// <summary>
        /// 
        /// </summary>
        public static readonly DependencyProperty EventAggregatorProperty = DependencyProperty.Register(
            "EventAggregator", typeof(IEventAggregator), typeof(EventAggregatorBehavior<T>), new PropertyMetadata(default(IEventAggregator), EventAggregatorChanged));

        private static void EventAggregatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (EventAggregatorBehavior<T>)d;
            self.EventAggregatorChanged();
        }

        private void EventAggregatorChanged()
        {
            _subscribeDisposable?.Dispose();
            _subscribeDisposable = Subscribe();
        }

        /// <summary>
        /// 
        /// </summary>
        public IEventAggregator EventAggregator
        {
            get { return (IEventAggregator) GetValue(EventAggregatorProperty); }
            set { SetValue(EventAggregatorProperty, value); }
        }

        private IDisposable _subscribeDisposable;

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            _subscribeDisposable?.Dispose();

            if (EventAggregator is null)
                return;

            _subscribeDisposable = Subscribe();
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            _subscribeDisposable?.Dispose();
            _subscribeDisposable = null;

            base.OnDetaching();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected abstract IDisposable Subscribe();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    public abstract class PubSubEventBehavior<T, TEvent> : EventAggregatorBehavior<T>
    where TEvent : PubSubEvent, new() where T : DependencyObject
    {
        private readonly ThreadOption _threadOption;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadOption"></param>
        protected PubSubEventBehavior(ThreadOption threadOption = ThreadOption.PublisherThread)
        {
            _threadOption = threadOption;
        }

        /// <inheritdoc />
        protected override IDisposable Subscribe()
        {
            return EventAggregator?
                .GetEvent<TEvent>()
                .Subscribe(Raised, _threadOption);
        }

        /// <summary>
        /// 
        /// </summary>
        protected abstract void Raised();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TPayLoad"></typeparam>
    public abstract class PubSubEventBehavior<T, TEvent, TPayLoad> : EventAggregatorBehavior<T>
        where TEvent : PubSubEvent<TPayLoad>, new() where T : DependencyObject
    {
        private readonly ThreadOption _threadOption;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadOption"></param>
        protected PubSubEventBehavior(ThreadOption threadOption = ThreadOption.PublisherThread)
        {
            _threadOption = threadOption;
        }

        /// <inheritdoc />
        protected override IDisposable Subscribe()
        {
            return EventAggregator?
                .GetEvent<TEvent>()
                .Subscribe(Raised, _threadOption);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payLoad"></param>
        protected abstract void Raised(TPayLoad payLoad);
    }
}