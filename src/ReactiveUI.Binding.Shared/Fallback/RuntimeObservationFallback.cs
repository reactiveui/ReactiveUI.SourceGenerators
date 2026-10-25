// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>
/// Observes properties of WhenChanged, WhenChanging and WhenAnyValue calls the source generator did not handle,
/// by walking the property expression at runtime and resolving each link through the registered
/// <see cref="ICreatesObservableForProperty"/> implementations.
/// </summary>
/// <remarks>
/// Every observable emits the current value on subscribe and then again after each change, skipping a value equal to
/// the one before it. When the observed object is null, or a property partway along a path is null, nothing is
/// emitted until the path resolves. The multi-property overloads emit once every property has a value.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
[RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
public static class RuntimeObservationFallback
{
    /// <summary>Observes a property, emitting its value after each change.</summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property">The property expression.</param>
    /// <returns>An observable that emits property values after they change.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
    public static IObservable<TValue> WhenChanged<TObj, TValue>(
        TObj obj,
        Expression<Func<TObj, TValue>> property)
        where TObj : class
    {
        ArgumentExceptionHelper.ThrowIfNull(property);

        return new MapSignal<IObservedChange<TObj, TValue>, TValue>(
            obj.SubscribeToExpressionChain<TObj, TValue>(property.Body, skipInitial: false),
            static x => x.Value);
    }

    /// <summary>Observes two properties, emitting both values whenever either changes.</summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <returns>An observable that emits a tuple of property values when any changes.</returns>
    public static IObservable<PropertyValues<T1, T2>> WhenChanged<TObj, T1, T2>(
        TObj obj,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2)
        where TObj : class
    {
        var o1 = WhenChanged(obj, property1);
        var o2 = WhenChanged(obj, property2);
        return CombineLatestObservable.Create(o1, o2, static (v1, v2) => new PropertyValues<T1, T2>(v1, v2));
    }

    /// <summary>Observes three properties, emitting all their values whenever any changes.</summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <typeparam name="T3">The type of the third property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <param name="property3">The third property expression.</param>
    /// <returns>An observable that emits a tuple of property values when any changes.</returns>
    public static IObservable<PropertyValues<T1, T2, T3>> WhenChanged<TObj, T1, T2, T3>(
        TObj obj,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3)
        where TObj : class
    {
        var o1 = WhenChanged(obj, property1);
        var o2 = WhenChanged(obj, property2);
        var o3 = WhenChanged(obj, property3);
        return CombineLatestObservable.Create(o1, o2, o3, static (v1, v2, v3) => new PropertyValues<T1, T2, T3>(v1, v2, v3));
    }

    /// <summary>Observes a property, emitting its value just before each change.</summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property">The property expression.</param>
    /// <returns>An observable that emits property values before they change.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
    public static IObservable<TValue> WhenChanging<TObj, TValue>(
        TObj obj,
        Expression<Func<TObj, TValue>> property)
        where TObj : class
    {
        ArgumentExceptionHelper.ThrowIfNull(property);

        return new MapSignal<IObservedChange<TObj, TValue>, TValue>(
            obj.SubscribeToExpressionChain<TObj, TValue>(property.Body, true, false, true),
            static x => x.Value);
    }

    /// <summary>Observes two properties, emitting both values whenever either is about to change.</summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <returns>An observable that emits a tuple of property values before any changes.</returns>
    public static IObservable<PropertyValues<T1, T2>> WhenChanging<TObj, T1, T2>(
        TObj obj,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2)
        where TObj : class
    {
        var o1 = WhenChanging(obj, property1);
        var o2 = WhenChanging(obj, property2);
        return CombineLatestObservable.Create(o1, o2, static (v1, v2) => new PropertyValues<T1, T2>(v1, v2));
    }

    /// <summary>Observes three properties, emitting all their values whenever any is about to change.</summary>
    /// <typeparam name="TObj">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <typeparam name="T3">The type of the third property value.</typeparam>
    /// <param name="obj">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <param name="property3">The third property expression.</param>
    /// <returns>An observable that emits a tuple of property values before any changes.</returns>
    public static IObservable<PropertyValues<T1, T2, T3>> WhenChanging<TObj, T1, T2, T3>(
        TObj obj,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3)
        where TObj : class
    {
        var o1 = WhenChanging(obj, property1);
        var o2 = WhenChanging(obj, property2);
        var o3 = WhenChanging(obj, property3);
        return CombineLatestObservable.Create(o1, o2, o3, static (v1, v2, v3) => new PropertyValues<T1, T2, T3>(v1, v2, v3));
    }

    /// <summary>Observes a property, emitting its value after each change.</summary>
    /// <typeparam name="TSender">The type of object being observed.</typeparam>
    /// <typeparam name="TValue">The type of the property value.</typeparam>
    /// <param name="sender">The object to observe.</param>
    /// <param name="property">The property expression.</param>
    /// <returns>An observable that emits property values after they change.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="property"/> is <see langword="null"/>.</exception>
    public static IObservable<TValue> WhenAnyValue<TSender, TValue>(
        TSender sender,
        Expression<Func<TSender, TValue>> property)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(property);

        return new MapSignal<IObservedChange<TSender, TValue>, TValue>(
            sender.SubscribeToExpressionChain<TSender, TValue>(property.Body, skipInitial: false),
            static x => x.Value);
    }

    /// <summary>Observes two properties, emitting both values whenever either changes.</summary>
    /// <typeparam name="TSender">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <param name="sender">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <returns>An observable that emits a tuple of property values when any changes.</returns>
    public static IObservable<PropertyValues<T1, T2>> WhenAnyValue<TSender, T1, T2>(
        TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2)
        where TSender : class
    {
        var o1 = WhenAnyValue(sender, property1);
        var o2 = WhenAnyValue(sender, property2);
        return CombineLatestObservable.Create(o1, o2, static (v1, v2) => new PropertyValues<T1, T2>(v1, v2));
    }

    /// <summary>Observes three properties, emitting all their values whenever any changes.</summary>
    /// <typeparam name="TSender">The type of object being observed.</typeparam>
    /// <typeparam name="T1">The type of the first property value.</typeparam>
    /// <typeparam name="T2">The type of the second property value.</typeparam>
    /// <typeparam name="T3">The type of the third property value.</typeparam>
    /// <param name="sender">The object to observe.</param>
    /// <param name="property1">The first property expression.</param>
    /// <param name="property2">The second property expression.</param>
    /// <param name="property3">The third property expression.</param>
    /// <returns>An observable that emits a tuple of property values when any changes.</returns>
    public static IObservable<PropertyValues<T1, T2, T3>> WhenAnyValue<TSender, T1, T2, T3>(
        TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3)
        where TSender : class
    {
        var o1 = WhenAnyValue(sender, property1);
        var o2 = WhenAnyValue(sender, property2);
        var o3 = WhenAnyValue(sender, property3);
        return CombineLatestObservable.Create(o1, o2, o3, static (v1, v2, v3) => new PropertyValues<T1, T2, T3>(v1, v2, v3));
    }
}
