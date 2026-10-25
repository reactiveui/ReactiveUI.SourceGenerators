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

/// <summary>Resolves a WhenChanged expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>WhenChanged</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>
    /// Observes 2 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Func<T1, T2, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
            property1,
            property2),
            t => conversionFunc(t.Property1, t.Property2));

    /// <summary>
    /// Observes 3 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Func<T1, T2, T3, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
            property1,
            property2,
            property3),
            t => conversionFunc(t.Property1, t.Property2, t.Property3));

    /// <summary>
    /// Observes 4 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Func<T1, T2, T3, T4, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
            property1,
            property2,
            property3,
            property4),
            t => conversionFunc(t.Property1, t.Property2, t.Property3, t.Property4));

    /// <summary>
    /// Observes 5 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, T5, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Func<T1, T2, T3, T4, T5, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5),
            t => conversionFunc(t.Property1, t.Property2, t.Property3, t.Property4, t.Property5));

    /// <summary>
    /// Observes 6 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, T5, T6, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Func<T1, T2, T3, T4, T5, T6, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6),
            t => conversionFunc(t.Property1, t.Property2, t.Property3, t.Property4, t.Property5, t.Property6));

    /// <summary>
    /// Observes 7 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, T5, T6, T7, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Func<T1, T2, T3, T4, T5, T6, T7, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7),
            t => conversionFunc(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7));

    /// <summary>
    /// Observes 8 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8),
            t => conversionFunc(
                t.Property1,
                t.Property2,
                t.Property3,
                t.Property4,
                t.Property5,
                t.Property6,
                t.Property7,
                t.Property8));

    /// <summary>
    /// Observes 9 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <typeparam name="T8">The type of the eighth observed property value.</typeparam>
    /// <typeparam name="T9">The type of the ninth observed property value.</typeparam>
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <param name="property9">An expression that selects the ninth property to observe.</param>
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
            property1,
            property2,
            property3,
            property4,
            property5,
            property6,
            property7,
            property8,
            property9),
            t => conversionFunc(
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
    /// Observes 10 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
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
            t => conversionFunc(
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
    /// Observes 11 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
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
            t => conversionFunc(
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
    /// Observes 12 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<TObj, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
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
            t => conversionFunc(
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
    /// Observes 13 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<
        TObj,
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
        TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
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
            t => conversionFunc(
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
    /// Observes 14 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<
        TObj,
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
        TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
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
            t => conversionFunc(
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
    /// Observes 15 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<
        TObj,
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
        TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Expression<Func<TObj, T15>> property15,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
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
            t => conversionFunc(
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
    /// Observes 16 properties by reflection and emits the conversion function's result for their values, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TObj">The type of the object to monitor for property changes.</typeparam>
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
    /// <typeparam name="TReturn">The return type of the conversion function.</typeparam>
    /// <param name="objectToMonitor">The object instance to observe for property changes.</param>
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
    /// <param name="conversionFunc">A function that converts the observed property values to the return type.</param>
    /// <returns>An observable sequence that emits the converted result when any of the observed properties changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<TReturn> WhenChangedUnsafe<
        TObj,
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
        TReturn>(
        this TObj objectToMonitor,
        Expression<Func<TObj, T1>> property1,
        Expression<Func<TObj, T2>> property2,
        Expression<Func<TObj, T3>> property3,
        Expression<Func<TObj, T4>> property4,
        Expression<Func<TObj, T5>> property5,
        Expression<Func<TObj, T6>> property6,
        Expression<Func<TObj, T7>> property7,
        Expression<Func<TObj, T8>> property8,
        Expression<Func<TObj, T9>> property9,
        Expression<Func<TObj, T10>> property10,
        Expression<Func<TObj, T11>> property11,
        Expression<Func<TObj, T12>> property12,
        Expression<Func<TObj, T13>> property13,
        Expression<Func<TObj, T14>> property14,
        Expression<Func<TObj, T15>> property15,
        Expression<Func<TObj, T16>> property16,
        Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn> conversionFunc)
        where TObj : class
        => new MapSignal<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, TReturn>(
            objectToMonitor.WhenChangedUnsafe(
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
            t => conversionFunc(
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
