using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Rml.AppCenter
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TrackEventAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TrackEventPropertyAttribute : Attribute
    {

    }

    public class AnalyticsCrashesService
    {
        private static readonly Type UtilType = typeof(AnalyticsCrashesService);
        private static readonly MethodInfo SubscribeMethodInfo = UtilType.GetMethod(nameof(Subscribe), BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly Type TrackEventAttributeType = typeof(TrackEventAttribute);
        private static readonly Type TrackEventPropertyAttributeType = typeof(TrackEventPropertyAttribute);
        private static readonly ConcurrentDictionary<Type, FieldInfo[]> TrackEventFieldInfos = new ConcurrentDictionary<Type, FieldInfo[]>();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TrackEventPropertyInfos = new ConcurrentDictionary<Type, PropertyInfo[]>();

        static AnalyticsCrashesService()
        {
            if (SubscribeMethodInfo is null)
                throw new InvalidOperationException();
        }

        public void TrackEvent(string name, params (string key, string value)[] properties)
        {
            Analytics.TrackEvent(name, properties.ToDictionary(o => o.key, o => o.value));
        }

        public void TrackError(Exception exception, params (string key, string value)[] properties)
        {
            Crashes.TrackError(exception, properties.ToDictionary(o => o.key, o => o.value));
        }

        public void TrackEvent(string name, string subName, object param, Type type)
        {
            var fieldInfos = TrackEventFieldInfos.GetOrAdd(type, o => o
                .GetFields()
                .Where(oo => Attribute.IsDefined(oo, TrackEventPropertyAttributeType))
                .ToArray());
            var propertyInfos = TrackEventPropertyInfos.GetOrAdd(type, o => o
                .GetProperties()
                .Where(oo => Attribute.IsDefined(oo, TrackEventPropertyAttributeType))
                .ToArray());
            
            var fields = fieldInfos
                .Select(o => (name:o.Name, value:o.GetValue(param)));
            var properties = propertyInfos
                .Select(o => (name:o.Name, value:o.GetValue(param)));
            var eventProperties = (subName is null
                    ? Enumerable.Empty<(string name, object value)>()
                    : new[]
                    {
                        (name: "SubName", value: (object) subName),
                    })
                .Concat(fields)
                .Concat(properties)
                .OrderBy(o => o.name)
                .ToDictionary(o => o.name, o => $"{o.value}");
            Analytics.TrackEvent(name, eventProperties);
        }

        public void TrackEvent<TParam>(string name, string subName, TParam param)
        {
            var type = typeof(TParam);
            TrackEvent(name, subName, param, type);
        }

        private IDisposable Subscribe<T>(IObservable<T> source, string name, string subName)
        {
            return source
                .Subscribe(o => TrackEvent(name, subName, o));
        }

        public IDisposable SetupTrackEvent<TSource>(TSource source)
        {
            var type = typeof(TSource);
            var fieldInfos = TrackEventFieldInfos.GetOrAdd(type, o => o
                .GetFields()
                .Where(oo => Attribute.IsDefined(oo, TrackEventAttributeType))
                .ToArray());
            var propertyInfos = TrackEventPropertyInfos.GetOrAdd(type, o => o
                .GetProperties()
                .Where(oo => Attribute.IsDefined(oo, TrackEventAttributeType))
                .ToArray());

            var fields = fieldInfos
                .Select(o => (name:o.Name, value:o.GetValue(source)));
            var properties = propertyInfos
                .Select(o => (name:o.Name, value:o.GetValue(source)));

            var cd = new CompositeDisposable();

            foreach (var (name, value) in fields
                .Concat(properties)
                .OrderBy(o => o.name))
            {
                var valueType = value.GetType();
                valueType
                    .GetInterfaces()
                    .Where(o => o.IsConstructedGenericType)
                    .SelectMany(o =>
                    {
                        if (o.GetGenericTypeDefinition() == typeof(IObservable<>))
                            return new []{(genericTypeArguments: o.GenericTypeArguments, observable: value, name, subName: default(string))};

                        if (o.GetGenericTypeDefinition() != typeof(IObserveCallAndResponseCommand<,>))
                            return Array.Empty<(Type[] genericTypeArguments, object observable, string name, string subName)>();

                        var genericTypeArgumentsCall = o.GenericTypeArguments.AsSpan(0, 1).ToArray();
                        var genericTypeArgumentResponse = o.GenericTypeArguments.AsSpan(1, 1).ToArray();

                        var observeCallMethod =
                            o.GetMethod(nameof(IObserveCallAndResponseCommand<object, object>.ObserveCall)) ??
                            throw new InvalidOperationException();
                        var observeResponseMethod =
                            o.GetMethod(nameof(IObserveCallAndResponseCommand<object, object>.ObserveResponse)) ??
                            throw new InvalidOperationException();

                        var observableCall = observeCallMethod.Invoke(value, null);
                        var observableResponse = observeResponseMethod.Invoke(value, null);

                        return new[]
                        {
                            (genericTypeArguments: genericTypeArgumentsCall, observable: observableCall, name, subName: "Call"),
                            (genericTypeArguments: genericTypeArgumentResponse, observable: observableResponse, name, subName: "Response"),
                        };
                    })
                    .Select(o => (methodInfo: SubscribeMethodInfo
                        .MakeGenericMethod(o.genericTypeArguments), o.observable, o.name, o.subName))
                    .Select(o => (IDisposable) o.methodInfo
                        .Invoke(this, new[] {o.observable, o.name, o.subName}))
                    .ForEach(o => cd.Add(o));
            }

            return cd;
        }
    }
}