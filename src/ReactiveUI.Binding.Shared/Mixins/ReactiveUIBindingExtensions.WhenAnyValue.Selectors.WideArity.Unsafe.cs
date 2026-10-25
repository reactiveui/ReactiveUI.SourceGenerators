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

/// <summary>Resolves a WhenAnyValue expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>WhenAnyValue</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>
    /// Observes a property by reflection and emits the selector's result for its value, on subscription and after each change. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="selector">A function that converts the observed property value to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when the observed property changes.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Func<T1, TRet> selector)
        where TSender : class
        => new MapSignal<T1, TRet>(sender.WhenAnyValueUnsafe(property1), selector);

    /// <summary>
    /// Observes 2 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Func<T1, T2, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2>, TRet>(sender.WhenAnyValueUnsafe(property1, property2), t => selector(t.Property1, t.Property2));

    /// <summary>
    /// Observes 3 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Func<T1, T2, T3, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3),
            t => selector(t.Property1, t.Property2, t.Property3));

    /// <summary>
    /// Observes 4 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Func<T1, T2, T3, T4, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4),
            t => selector(t.Property1, t.Property2, t.Property3, t.Property4));

    /// <summary>
    /// Observes 5 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Func<T1, T2, T3, T4, T5, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5),
            t => selector(t.Property1, t.Property2, t.Property3, t.Property4, t.Property5));

    /// <summary>
    /// Observes 6 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Func<T1, T2, T3, T4, T5, T6, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6),
            t => selector(t.Property1, t.Property2, t.Property3, t.Property4, t.Property5, t.Property6));

    /// <summary>
    /// Observes 7 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Func<T1, T2, T3, T4, T5, T6, T7, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7));

    /// <summary>
    /// Observes 8 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8));

    /// <summary>
    /// Observes 9 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8,
                t.Property9));

    /// <summary>
    /// Observes 10 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9,
            property10),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8,
                t.Property9,
                t.Property10));

    /// <summary>
    /// Observes 11 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9,
            property10,
            property11),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8,
                t.Property9,
                t.Property10,
                t.Property11));

    /// <summary>
    /// Observes 12 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9,
            property10,
            property11,
            property12),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8,
                t.Property9,
                t.Property10,
                t.Property11,
                t.Property12));

    /// <summary>
    /// Observes 13 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Expression<Func<TSender, T13>> property13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9,
            property10,
            property11,
            property12,
            property13),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8,
                t.Property9,
                t.Property10,
                t.Property11,
                t.Property12,
                t.Property13));

    /// <summary>
    /// Observes 14 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<
        TSender,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Expression<Func<TSender, T13>> property13,
        Expression<Func<TSender, T14>> property14,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9,
            property10,
            property11,
            property12,
            property13,
            property14),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8,
                t.Property9,
                t.Property10,
                t.Property11,
                t.Property12,
                t.Property13,
                t.Property14));

    /// <summary>
    /// Observes 15 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="property15">An expression that selects the fifteenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<
        TSender,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15,
        TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Expression<Func<TSender, T13>> property13,
        Expression<Func<TSender, T14>> property14,
        Expression<Func<TSender, T15>> property15,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9,
            property10,
            property11,
            property12,
            property13,
            property14,
            property15),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8,
                t.Property9,
                t.Property10,
                t.Property11,
                t.Property12,
                t.Property13,
                t.Property14,
                t.Property15));

    /// <summary>
    /// Observes 16 properties by reflection and emits the selector's result for their values, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="T10">The type of the tenth observed property value.</typeparam>
    /// <typeparam name="T11">The type of the eleventh observed property value.</typeparam>
    /// <typeparam name="T12">The type of the twelfth observed property value.</typeparam>
    /// <typeparam name="T13">The type of the thirteenth observed property value.</typeparam>
    /// <typeparam name="T14">The type of the fourteenth observed property value.</typeparam>
    /// <typeparam name="T15">The type of the fifteenth observed property value.</typeparam>
    /// <typeparam name="T16">The type of the sixteenth observed property value.</typeparam>
    /// <typeparam name="TRet">The return type of the selector function.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="property10">An expression that selects the tenth property to observe.</param>
    /// <param name="property11">An expression that selects the eleventh property to observe.</param>
    /// <param name="property12">An expression that selects the twelfth property to observe.</param>
    /// <param name="property13">An expression that selects the thirteenth property to observe.</param>
    /// <param name="property14">An expression that selects the fourteenth property to observe.</param>
    /// <param name="property15">An expression that selects the fifteenth property to observe.</param>
    /// <param name="property16">An expression that selects the sixteenth property to observe.</param>
    /// <param name="selector">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the selector result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TRet> WhenAnyValueUnsafe<
        TSender,
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15,
        T16,
        TRet>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9,
        Expression<Func<TSender, T10>> property10,
        Expression<Func<TSender, T11>> property11,
        Expression<Func<TSender, T12>> property12,
        Expression<Func<TSender, T13>> property13,
        Expression<Func<TSender, T14>> property14,
        Expression<Func<TSender, T15>> property15,
        Expression<Func<TSender, T16>> property16,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TRet> selector)
        where TSender : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TRet>(
            sender.WhenAnyValueUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9,
            property10,
            property11,
            property12,
            property13,
            property14,
            property15,
            property16),
            t => selector(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8,
                t.Property9,
                t.Property10,
                t.Property11,
                t.Property12,
                t.Property13,
                t.Property14,
                t.Property15,
                t.Property16));
}
