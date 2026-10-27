// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Backs a read-only property with an observable, resolving the property and its notifications by reflection.</summary>
/// <remarks>
/// The opt-in half of <c>ToProperty</c>, for a call site the generator could not read: a property named by a string
/// that is not a constant, or a source type whose notifications generated code cannot raise. These read the property
/// name and find the members that raise its notifications at run time, and are annotated so a consumer publishing
/// trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Backs the selected read-only property with the observable's latest value, resolving the property and its notifications by reflection.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), default(TRet), false, null);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with optionally deferred
    /// subscription, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        bool deferSubscription)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), default(TRet), deferSubscription, null);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with a scheduler for its change
    /// notifications, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), default(TRet), false, scheduler);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with optionally deferred
    /// subscription and a scheduler for its change notifications, resolving the property and its notifications by
    /// reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), default(TRet), deferSubscription, scheduler);

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value, resolving the property and its notifications by reflection.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        TRet initialValue)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), initialValue, false, null);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value and a
    /// scheduler for its change notifications, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        TRet initialValue,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), initialValue, false, scheduler);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value and optionally
    /// deferred subscription, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        TRet initialValue,
        bool deferSubscription)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), initialValue, deferSubscription, null);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value, optionally
    /// deferred subscription and a scheduler for its change notifications, resolving the property and its
    /// notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        TRet initialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), initialValue, deferSubscription, scheduler);

    /// <summary>Backs the selected read-only property with the observable's latest value, with an initial value factory, resolving the property and its notifications by reflection.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        Func<TRet> getInitialValue)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), getInitialValue, false, null);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory and a
    /// scheduler for its change notifications, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        Func<TRet> getInitialValue,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), getInitialValue, false, scheduler);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory and
    /// optionally deferred subscription, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        Func<TRet> getInitialValue,
        bool deferSubscription)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), getInitialValue, deferSubscription, null);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory,
    /// optionally deferred subscription and a scheduler for its change notifications, resolving the property and its
    /// notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), getInitialValue, deferSubscription, scheduler);

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value and also returns it through an out
    /// parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), default(TRet), false, null);
        return result;
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with optionally deferred
    /// subscription and also returns it through an out parameter, resolving the property and its notifications by
    /// reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result,
        bool deferSubscription)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), default(TRet), deferSubscription, null);
        return result;
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with optionally deferred
    /// subscription and a scheduler for its change notifications and also returns it through an out parameter,
    /// resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), default(TRet), deferSubscription, scheduler);
        return result;
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value and also
    /// returns it through an out parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result,
        TRet initialValue)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), initialValue, false, null);
        return result;
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value and optionally
    /// deferred subscription and also returns it through an out parameter, resolving the property and its
    /// notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result,
        TRet initialValue,
        bool deferSubscription)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), initialValue, deferSubscription, null);
        return result;
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value, optionally
    /// deferred subscription and a scheduler for its change notifications and also returns it through an out
    /// parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result,
        TRet initialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), initialValue, deferSubscription, scheduler);
        return result;
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory and
    /// also returns it through an out parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), getInitialValue, false, null);
        return result;
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory and
    /// optionally deferred subscription and also returns it through an out parameter, resolving the property and its
    /// notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        bool deferSubscription)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), getInitialValue, deferSubscription, null);
        return result;
    }

    /// <summary>
    /// Backs the selected read-only property with the observable's latest value, with an initial value factory,
    /// optionally deferred subscription and a scheduler for its change notifications and also returns it through an
    /// out parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">A selector of the form <c>x =&gt; x.Property</c> that names the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        Expression<Func<TObj, TRet>> property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, ReadUnsafePropertyName(property), getInitialValue, deferSubscription, scheduler);
        return result;
    }

    /// <summary>Backs the named read-only property with the observable's latest value, with an initial value, resolving the property and its notifications by reflection.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        TRet initialValue)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, initialValue, false, null);

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value and a scheduler
    /// for its change notifications, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        TRet initialValue,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, initialValue, false, scheduler);

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value and optionally
    /// deferred subscription, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        TRet initialValue,
        bool deferSubscription)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, initialValue, deferSubscription, null);

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value, optionally
    /// deferred subscription and a scheduler for its change notifications, resolving the property and its
    /// notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        TRet initialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, initialValue, deferSubscription, scheduler);

    /// <summary>Backs the named read-only property with the observable's latest value, resolving the property and its notifications by reflection.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, default(TRet), false, null);

    /// <summary>Backs the named read-only property with the observable's latest value, with optionally deferred subscription, resolving the property and its notifications by reflection.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        bool deferSubscription)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, default(TRet), deferSubscription, null);

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with a scheduler for its change
    /// notifications, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, default(TRet), false, scheduler);

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with optionally deferred subscription
    /// and a scheduler for its change notifications, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, default(TRet), deferSubscription, scheduler);

    /// <summary>Backs the named read-only property with the observable's latest value, with an initial value factory, resolving the property and its notifications by reflection.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        Func<TRet> getInitialValue)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, getInitialValue, false, null);

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value factory and
    /// optionally deferred subscription, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        Func<TRet> getInitialValue,
        bool deferSubscription)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, getInitialValue, deferSubscription, null);

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value factory,
    /// optionally deferred subscription and a scheduler for its change notifications, resolving the property and its
    /// notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
        => CreateUnsafeHelper(target, source, property, getInitialValue, deferSubscription, scheduler);

    /// <summary>
    /// Backs the named read-only property with the observable's latest value and also returns it through an out
    /// parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, property, default(TRet), false, null);
        return result;
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with optionally deferred subscription
    /// and also returns it through an out parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        bool deferSubscription)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, property, default(TRet), deferSubscription, null);
        return result;
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with optionally deferred subscription
    /// and a scheduler for its change notifications and also returns it through an out parameter, resolving the
    /// property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, property, default(TRet), deferSubscription, scheduler);
        return result;
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value factory and also
    /// returns it through an out parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, property, getInitialValue, false, null);
        return result;
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value factory and
    /// optionally deferred subscription and also returns it through an out parameter, resolving the property and its
    /// notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        bool deferSubscription)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, property, getInitialValue, deferSubscription, null);
        return result;
    }

    /// <summary>
    /// Backs the named read-only property with the observable's latest value, with an initial value factory,
    /// optionally deferred subscription and a scheduler for its change notifications and also returns it through an
    /// out parameter, resolving the property and its notifications by reflection.
    /// </summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="property">The name of the property.</param>
    /// <param name="result">Receives the helper, so it can be assigned to a field inside an expression.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper that holds the property's current value.</returns>
    /// <exception cref="InvalidOperationException">The source's type raises no change notification reflection can reach.</exception>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    public static ObservableAsPropertyHelper<TRet> ToPropertyUnsafe<TObj, TRet>(
        this IObservable<TRet> target,
        TObj source,
        string property,
        out ObservableAsPropertyHelper<TRet> result,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        result = CreateUnsafeHelper(target, source, property, getInitialValue, deferSubscription, scheduler);
        return result;
    }

    /// <summary>Creates a helper whose notifications are raised by reflection, with a plain initial value.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="initialValue">The value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper.</returns>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    private static ObservableAsPropertyHelper<TRet> CreateUnsafeHelper<TObj, TRet>(
        IObservable<TRet> target,
        TObj source,
        string propertyName,
        TRet? initialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        var raiser = CreateUnsafeRaiser(source, propertyName);
        return ObservableAsPropertyHelper<TRet>.Create(
            target,
            raiser,
            static (owner, _) => ((RuntimePropertyRaiser)owner!).RaiseChanged(),
            raiser.RaisesChanging ? static (owner, _) => ((RuntimePropertyRaiser)owner!).RaiseChanging() : null,
            initialValue,
            deferSubscription,
            scheduler);
    }

    /// <summary>Creates a helper whose notifications are raised by reflection, with an initial value factory.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="target">The observable whose values the property takes.</param>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="propertyName">The property name.</param>
    /// <param name="getInitialValue">Returns the value the property holds before the observable produces one.</param>
    /// <param name="deferSubscription">Whether to wait for the first read of the property's value before subscribing.</param>
    /// <param name="scheduler">The scheduler change notifications are raised on, or null to raise them on the producing thread.</param>
    /// <returns>The helper.</returns>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    private static ObservableAsPropertyHelper<TRet> CreateUnsafeHelper<TObj, TRet>(
        IObservable<TRet> target,
        TObj source,
        string propertyName,
        Func<TRet> getInitialValue,
        bool deferSubscription,
        ISequencer? scheduler)
        where TObj : class
    {
        ArgumentExceptionHelper.ThrowIfNull(getInitialValue);

        var raiser = CreateUnsafeRaiser(source, propertyName);
        return ObservableAsPropertyHelper<TRet>.Create(
            target,
            raiser,
            static (owner, _) => ((RuntimePropertyRaiser)owner!).RaiseChanged(),
            raiser.RaisesChanging ? static (owner, _) => ((RuntimePropertyRaiser)owner!).RaiseChanging() : null,
            getInitialValue!,
            deferSubscription,
            scheduler);
    }

    /// <summary>Checks the arguments and finds the members that raise the property's notifications.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <param name="source">The object that declares the property and raises its change notifications.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The raiser.</returns>
    [RequiresUnreferencedCode(RuntimePropertyRaiser.RequiresUnreferencedCodeMessage)]
    [RequiresDynamicCode(RuntimePropertyRaiser.RequiresDynamicCodeMessage)]
    private static RuntimePropertyRaiser CreateUnsafeRaiser<TObj>(TObj source, string propertyName)
        where TObj : class
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNullOrWhiteSpace(propertyName);

        return RuntimePropertyRaiser.Create(source, propertyName);
    }

    /// <summary>Reads the property name from a selector of the form <c>x =&gt; x.Property</c>.</summary>
    /// <typeparam name="TObj">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TRet">The type of the property value.</typeparam>
    /// <param name="property">The selector.</param>
    /// <returns>The property name.</returns>
    /// <exception cref="ArgumentException">The selector does not read one member straight off its parameter.</exception>
    private static string ReadUnsafePropertyName<TObj, TRet>(Expression<Func<TObj, TRet>> property)
    {
        ArgumentExceptionHelper.ThrowIfNull(property);

        var body = property.Body is UnaryExpression { NodeType: ExpressionType.Convert } convert ? convert.Operand : property.Body;
        return body is MemberExpression { Expression: ParameterExpression } member
            ? member.Member.Name
            : throw new ArgumentException("Name the property with a selector of the form x => x.Property.", nameof(property));
    }
}
