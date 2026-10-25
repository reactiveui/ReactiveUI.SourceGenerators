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
/// <summary>Observes property chains by reflection, for expressions the generator could not read (WhenAnyValue).</summary>
/// <remarks>
/// These are the opt-in half of <c>WhenAnyValue</c>. The unsuffixed overloads resolve at compile time and throw when no
/// generated dispatch claimed the call site; these walk the chain by reflection instead, and say so, so a
/// consumer publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>
    /// Observes a property by reflection, emitting its current value on subscription and its new value after each change. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <returns>An observable sequence that emits the property value when it changes.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<T1> WhenAnyValueUnsafe<TSender, T1>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);

        return RuntimeObservationFallback.WhenAnyValue(sender, property1);
    }

    /// <summary>
    /// Observes 2 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2>> WhenAnyValueUnsafe<TSender, T1, T2>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            static (v1, v2) => new PropertyValues<T1, T2>(v1, v2));
    }

    /// <summary>
    /// Observes 3 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3>> WhenAnyValueUnsafe<TSender, T1, T2, T3>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            static (v1, v2, v3) => new PropertyValues<T1, T2, T3>(v1, v2, v3));
    }

    /// <summary>
    /// Observes 4 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4>> WhenAnyValueUnsafe<
        TSender,
        T1,
        T2,
        T3,
        T4>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            static (v1, v2, v3, v4) => new PropertyValues<T1, T2, T3, T4>(v1, v2, v3, v4));
    }

    /// <summary>
    /// Observes 5 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5>> WhenAnyValueUnsafe<
        TSender,
        T1,
        T2,
        T3,
        T4,
        T5>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            static (v1, v2, v3, v4, v5) => new PropertyValues<T1, T2, T3, T4, T5>(v1, v2, v3, v4, v5));
    }

    /// <summary>
    /// Observes 6 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6>> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            static (v1, v2, v3, v4, v5, v6) => new PropertyValues<T1, T2, T3, T4, T5, T6>(v1, v2, v3, v4, v5, v6));
    }

    /// <summary>
    /// Observes 7 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="T1">The type of the first observed property value.</typeparam>
    /// <typeparam name="T2">The type of the second observed property value.</typeparam>
    /// <typeparam name="T3">The type of the third observed property value.</typeparam>
    /// <typeparam name="T4">The type of the fourth observed property value.</typeparam>
    /// <typeparam name="T5">The type of the fifth observed property value.</typeparam>
    /// <typeparam name="T6">The type of the sixth observed property value.</typeparam>
    /// <typeparam name="T7">The type of the seventh observed property value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7>> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            static (v1, v2, v3, v4, v5, v6, v7) => new PropertyValues<T1, T2, T3, T4, T5, T6, T7>(v1, v2, v3, v4, v5, v6, v7));
    }

    /// <summary>
    /// Observes 8 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects the first property to observe.</param>
    /// <param name="property2">An expression that selects the second property to observe.</param>
    /// <param name="property3">An expression that selects the third property to observe.</param>
    /// <param name="property4">An expression that selects the fourth property to observe.</param>
    /// <param name="property5">An expression that selects the fifth property to observe.</param>
    /// <param name="property6">An expression that selects the sixth property to observe.</param>
    /// <param name="property7">An expression that selects the seventh property to observe.</param>
    /// <param name="property8">An expression that selects the eighth property to observe.</param>
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8>> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            static (v1, v2, v3, v4, v5, v6, v7, v8) => new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8>(v1, v2, v3, v4, v5, v6, v7, v8));
    }

    /// <summary>
    /// Observes 9 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Expression<Func<TSender, T9>> property9)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);
        ArgumentExceptionHelper.ThrowIfNull(property9);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            RuntimeObservationFallback.WhenAnyValue(sender, property9),
            static (v1, v2, v3, v4, v5, v6, v7, v8, v9) =>
                new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9>(v1, v2, v3, v4, v5, v6, v7, v8, v9));
    }

    /// <summary>
    /// Observes 10 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
        Expression<Func<TSender, T10>> property10)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);
        ArgumentExceptionHelper.ThrowIfNull(property9);
        ArgumentExceptionHelper.ThrowIfNull(property10);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            RuntimeObservationFallback.WhenAnyValue(sender, property9),
            RuntimeObservationFallback.WhenAnyValue(sender, property10),
            static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10) =>
                new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10));
    }

    /// <summary>
    /// Observes 11 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>> WhenAnyValueUnsafe<
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
                T11>(
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
        Expression<Func<TSender, T11>> property11)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);
        ArgumentExceptionHelper.ThrowIfNull(property9);
        ArgumentExceptionHelper.ThrowIfNull(property10);
        ArgumentExceptionHelper.ThrowIfNull(property11);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            RuntimeObservationFallback.WhenAnyValue(sender, property9),
            RuntimeObservationFallback.WhenAnyValue(sender, property10),
            RuntimeObservationFallback.WhenAnyValue(sender, property11),
            static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11) =>
                new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11));
    }

    /// <summary>
    /// Observes 12 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>> WhenAnyValueUnsafe<
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
                T12>(
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
        Expression<Func<TSender, T12>> property12)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);
        ArgumentExceptionHelper.ThrowIfNull(property9);
        ArgumentExceptionHelper.ThrowIfNull(property10);
        ArgumentExceptionHelper.ThrowIfNull(property11);
        ArgumentExceptionHelper.ThrowIfNull(property12);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            RuntimeObservationFallback.WhenAnyValue(sender, property9),
            RuntimeObservationFallback.WhenAnyValue(sender, property10),
            RuntimeObservationFallback.WhenAnyValue(sender, property11),
            RuntimeObservationFallback.WhenAnyValue(sender, property12),
            static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12) =>
                new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12));
    }

    /// <summary>
    /// Observes 13 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>> WhenAnyValueUnsafe<
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
                T13>(
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
        Expression<Func<TSender, T13>> property13)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);
        ArgumentExceptionHelper.ThrowIfNull(property9);
        ArgumentExceptionHelper.ThrowIfNull(property10);
        ArgumentExceptionHelper.ThrowIfNull(property11);
        ArgumentExceptionHelper.ThrowIfNull(property12);
        ArgumentExceptionHelper.ThrowIfNull(property13);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            RuntimeObservationFallback.WhenAnyValue(sender, property9),
            RuntimeObservationFallback.WhenAnyValue(sender, property10),
            RuntimeObservationFallback.WhenAnyValue(sender, property11),
            RuntimeObservationFallback.WhenAnyValue(sender, property12),
            RuntimeObservationFallback.WhenAnyValue(sender, property13),
            static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13) =>

                    new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13));
    }

    /// <summary>
    /// Observes 14 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>> WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
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
        Expression<Func<TSender, T14>> property14)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);
        ArgumentExceptionHelper.ThrowIfNull(property9);
        ArgumentExceptionHelper.ThrowIfNull(property10);
        ArgumentExceptionHelper.ThrowIfNull(property11);
        ArgumentExceptionHelper.ThrowIfNull(property12);
        ArgumentExceptionHelper.ThrowIfNull(property13);
        ArgumentExceptionHelper.ThrowIfNull(property14);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            RuntimeObservationFallback.WhenAnyValue(sender, property9),
            RuntimeObservationFallback.WhenAnyValue(sender, property10),
            RuntimeObservationFallback.WhenAnyValue(sender, property11),
            RuntimeObservationFallback.WhenAnyValue(sender, property12),
            RuntimeObservationFallback.WhenAnyValue(sender, property13),
            RuntimeObservationFallback.WhenAnyValue(sender, property14),
            static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14) =>

                    new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14));
    }

    /// <summary>
    /// Observes 15 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>>
        WhenAnyValueUnsafe<TSender, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
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
        Expression<Func<TSender, T15>> property15)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);
        ArgumentExceptionHelper.ThrowIfNull(property9);
        ArgumentExceptionHelper.ThrowIfNull(property10);
        ArgumentExceptionHelper.ThrowIfNull(property11);
        ArgumentExceptionHelper.ThrowIfNull(property12);
        ArgumentExceptionHelper.ThrowIfNull(property13);
        ArgumentExceptionHelper.ThrowIfNull(property14);
        ArgumentExceptionHelper.ThrowIfNull(property15);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            RuntimeObservationFallback.WhenAnyValue(sender, property9),
            RuntimeObservationFallback.WhenAnyValue(sender, property10),
            RuntimeObservationFallback.WhenAnyValue(sender, property11),
            RuntimeObservationFallback.WhenAnyValue(sender, property12),
            RuntimeObservationFallback.WhenAnyValue(sender, property13),
            RuntimeObservationFallback.WhenAnyValue(sender, property14),
            RuntimeObservationFallback.WhenAnyValue(sender, property15),
            static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15) =>

                    new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15));
    }

    /// <summary>
    /// Observes 16 properties by reflection and emits their values as a tuple, first on subscription and again after any of them changes. This is a ReactiveUI compatibility shim.
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
    /// <returns>An observable sequence that emits a tuple of all observed property values when any of them changes.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode(DynamicChainRequiresUnreferencedCode)]
    public static IObservable<PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>> WhenAnyValueUnsafe<
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
                T16>(
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
        Expression<Func<TSender, T16>> property16)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(property7);
        ArgumentExceptionHelper.ThrowIfNull(property8);
        ArgumentExceptionHelper.ThrowIfNull(property9);
        ArgumentExceptionHelper.ThrowIfNull(property10);
        ArgumentExceptionHelper.ThrowIfNull(property11);
        ArgumentExceptionHelper.ThrowIfNull(property12);
        ArgumentExceptionHelper.ThrowIfNull(property13);
        ArgumentExceptionHelper.ThrowIfNull(property14);
        ArgumentExceptionHelper.ThrowIfNull(property15);
        ArgumentExceptionHelper.ThrowIfNull(property16);

        return CombineLatestObservable.Create(
            RuntimeObservationFallback.WhenAnyValue(sender, property1),
            RuntimeObservationFallback.WhenAnyValue(sender, property2),
            RuntimeObservationFallback.WhenAnyValue(sender, property3),
            RuntimeObservationFallback.WhenAnyValue(sender, property4),
            RuntimeObservationFallback.WhenAnyValue(sender, property5),
            RuntimeObservationFallback.WhenAnyValue(sender, property6),
            RuntimeObservationFallback.WhenAnyValue(sender, property7),
            RuntimeObservationFallback.WhenAnyValue(sender, property8),
            RuntimeObservationFallback.WhenAnyValue(sender, property9),
            RuntimeObservationFallback.WhenAnyValue(sender, property10),
            RuntimeObservationFallback.WhenAnyValue(sender, property11),
            RuntimeObservationFallback.WhenAnyValue(sender, property12),
            RuntimeObservationFallback.WhenAnyValue(sender, property13),
            RuntimeObservationFallback.WhenAnyValue(sender, property14),
            RuntimeObservationFallback.WhenAnyValue(sender, property15),
            RuntimeObservationFallback.WhenAnyValue(sender, property16),
            static (v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16) =>
                new PropertyValues<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16));
    }
}
