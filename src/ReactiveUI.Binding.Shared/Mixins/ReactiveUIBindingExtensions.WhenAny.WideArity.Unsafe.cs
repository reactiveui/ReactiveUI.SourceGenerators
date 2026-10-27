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

/// <summary>Resolves a WhenAny expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>WhenAny</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>Observes a property by reflection and applies a selector to its observed change, on subscription and after each change.</summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Func<IObservedChange<TSender, T1>, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);

        return new MapSignal<IObservedChange<TSender, T1>, TRet>(
            sender.SubscribeToExpressionChain<TSender, T1>(
                property1.Body,
                skipInitial: false),
            selector);
    }

    /// <summary>
    /// Observes 2 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Func<IObservedChange<TSender, T1>, IObservedChange<TSender, T2>, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            (c1, c2) => selector(c1, c2));
    }

    /// <summary>
    /// Observes 3 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Func<IObservedChange<TSender, T1>, IObservedChange<TSender, T2>, IObservedChange<TSender, T3>, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            (c1, c2, c3) => selector(c1, c2, c3));
    }

    /// <summary>
    /// Observes 4 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            (c1, c2, c3, c4) => selector(c1, c2, c3, c4));
    }

    /// <summary>
    /// Observes 5 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4, T5>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        var o5 = sender.SubscribeToExpressionChain<TSender, T5>(
            property5.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            (c1, c2, c3, c4, c5) => selector(c1, c2, c3, c4, c5));
    }

    /// <summary>
    /// Observes 6 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(property1);
        ArgumentExceptionHelper.ThrowIfNull(property2);
        ArgumentExceptionHelper.ThrowIfNull(property3);
        ArgumentExceptionHelper.ThrowIfNull(property4);
        ArgumentExceptionHelper.ThrowIfNull(property5);
        ArgumentExceptionHelper.ThrowIfNull(property6);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        var o5 = sender.SubscribeToExpressionChain<TSender, T5>(
            property5.Body,
            skipInitial: false);
        var o6 = sender.SubscribeToExpressionChain<TSender, T6>(
            property6.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            (c1, c2, c3, c4, c5, c6) => selector(c1, c2, c3, c4, c5, c6));
    }

    /// <summary>
    /// Observes 7 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            TRet> selector)
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
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        var o5 = sender.SubscribeToExpressionChain<TSender, T5>(
            property5.Body,
            skipInitial: false);
        var o6 = sender.SubscribeToExpressionChain<TSender, T6>(
            property6.Body,
            skipInitial: false);
        var o7 = sender.SubscribeToExpressionChain<TSender, T7>(
            property7.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            o7,
            (c1, c2, c3, c4, c5, c6, c7) => selector(c1, c2, c3, c4, c5, c6, c7));
    }

    /// <summary>
    /// Observes 8 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8>(
        this TSender sender,
        Expression<Func<TSender, T1>> property1,
        Expression<Func<TSender, T2>> property2,
        Expression<Func<TSender, T3>> property3,
        Expression<Func<TSender, T4>> property4,
        Expression<Func<TSender, T5>> property5,
        Expression<Func<TSender, T6>> property6,
        Expression<Func<TSender, T7>> property7,
        Expression<Func<TSender, T8>> property8,
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            TRet> selector)
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
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        var o5 = sender.SubscribeToExpressionChain<TSender, T5>(
            property5.Body,
            skipInitial: false);
        var o6 = sender.SubscribeToExpressionChain<TSender, T6>(
            property6.Body,
            skipInitial: false);
        var o7 = sender.SubscribeToExpressionChain<TSender, T7>(
            property7.Body,
            skipInitial: false);
        var o8 = sender.SubscribeToExpressionChain<TSender, T8>(
            property8.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            o7,
            o8,
            (c1, c2, c3, c4, c5, c6, c7, c8) => selector(c1, c2, c3, c4, c5, c6, c7, c8));
    }

    /// <summary>
    /// Observes 9 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <typeparam name="T9">The type of property 9 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="property9">An expression that selects property 9 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            IObservedChange<TSender, T9>,
            TRet> selector)
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
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        var o5 = sender.SubscribeToExpressionChain<TSender, T5>(
            property5.Body,
            skipInitial: false);
        var o6 = sender.SubscribeToExpressionChain<TSender, T6>(
            property6.Body,
            skipInitial: false);
        var o7 = sender.SubscribeToExpressionChain<TSender, T7>(
            property7.Body,
            skipInitial: false);
        var o8 = sender.SubscribeToExpressionChain<TSender, T8>(
            property8.Body,
            skipInitial: false);
        var o9 = sender.SubscribeToExpressionChain<TSender, T9>(
            property9.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            o7,
            o8,
            o9,
            (c1, c2, c3, c4, c5, c6, c7, c8, c9) => selector(c1, c2, c3, c4, c5, c6, c7, c8, c9));
    }

    /// <summary>
    /// Observes 10 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <typeparam name="T9">The type of property 9 value.</typeparam>
    /// <typeparam name="T10">The type of property 10 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="property9">An expression that selects property 9 to observe.</param>
    /// <param name="property10">An expression that selects property 10 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            IObservedChange<TSender, T9>,
            IObservedChange<TSender, T10>,
            TRet> selector)
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
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        var o5 = sender.SubscribeToExpressionChain<TSender, T5>(
            property5.Body,
            skipInitial: false);
        var o6 = sender.SubscribeToExpressionChain<TSender, T6>(
            property6.Body,
            skipInitial: false);
        var o7 = sender.SubscribeToExpressionChain<TSender, T7>(
            property7.Body,
            skipInitial: false);
        var o8 = sender.SubscribeToExpressionChain<TSender, T8>(
            property8.Body,
            skipInitial: false);
        var o9 = sender.SubscribeToExpressionChain<TSender, T9>(
            property9.Body,
            skipInitial: false);
        var o10 = sender.SubscribeToExpressionChain<TSender, T10>(
            property10.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            o7,
            o8,
            o9,
            o10,
            (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10) => selector(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10));
    }

    /// <summary>
    /// Observes 11 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <typeparam name="T9">The type of property 9 value.</typeparam>
    /// <typeparam name="T10">The type of property 10 value.</typeparam>
    /// <typeparam name="T11">The type of property 11 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="property9">An expression that selects property 9 to observe.</param>
    /// <param name="property10">An expression that selects property 10 to observe.</param>
    /// <param name="property11">An expression that selects property 11 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
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
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            IObservedChange<TSender, T9>,
            IObservedChange<TSender, T10>,
            IObservedChange<TSender, T11>,
            TRet> selector)
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
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        var o5 = sender.SubscribeToExpressionChain<TSender, T5>(
            property5.Body,
            skipInitial: false);
        var o6 = sender.SubscribeToExpressionChain<TSender, T6>(
            property6.Body,
            skipInitial: false);
        var o7 = sender.SubscribeToExpressionChain<TSender, T7>(
            property7.Body,
            skipInitial: false);
        var o8 = sender.SubscribeToExpressionChain<TSender, T8>(
            property8.Body,
            skipInitial: false);
        var o9 = sender.SubscribeToExpressionChain<TSender, T9>(
            property9.Body,
            skipInitial: false);
        var o10 = sender.SubscribeToExpressionChain<TSender, T10>(
            property10.Body,
            skipInitial: false);
        var o11 = sender.SubscribeToExpressionChain<TSender, T11>(
            property11.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            o7,
            o8,
            o9,
            o10,
            o11,
            (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11) => selector(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11));
    }

    /// <summary>
    /// Observes 12 properties by reflection and applies a selector to their observed changes, first on subscription and again after any of them changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The type of property 1 value.</typeparam>
    /// <typeparam name="T2">The type of property 2 value.</typeparam>
    /// <typeparam name="T3">The type of property 3 value.</typeparam>
    /// <typeparam name="T4">The type of property 4 value.</typeparam>
    /// <typeparam name="T5">The type of property 5 value.</typeparam>
    /// <typeparam name="T6">The type of property 6 value.</typeparam>
    /// <typeparam name="T7">The type of property 7 value.</typeparam>
    /// <typeparam name="T8">The type of property 8 value.</typeparam>
    /// <typeparam name="T9">The type of property 9 value.</typeparam>
    /// <typeparam name="T10">The type of property 10 value.</typeparam>
    /// <typeparam name="T11">The type of property 11 value.</typeparam>
    /// <typeparam name="T12">The type of property 12 value.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="property1">An expression that selects property 1 to observe.</param>
    /// <param name="property2">An expression that selects property 2 to observe.</param>
    /// <param name="property3">An expression that selects property 3 to observe.</param>
    /// <param name="property4">An expression that selects property 4 to observe.</param>
    /// <param name="property5">An expression that selects property 5 to observe.</param>
    /// <param name="property6">An expression that selects property 6 to observe.</param>
    /// <param name="property7">An expression that selects property 7 to observe.</param>
    /// <param name="property8">An expression that selects property 8 to observe.</param>
    /// <param name="property9">An expression that selects property 9 to observe.</param>
    /// <param name="property10">An expression that selects property 10 to observe.</param>
    /// <param name="property11">An expression that selects property 11 to observe.</param>
    /// <param name="property12">An expression that selects property 12 to observe.</param>
    /// <param name="selector">A function that combines the observed changes into a result.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
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
        Func<
            IObservedChange<TSender, T1>,
            IObservedChange<TSender, T2>,
            IObservedChange<TSender, T3>,
            IObservedChange<TSender, T4>,
            IObservedChange<TSender, T5>,
            IObservedChange<TSender, T6>,
            IObservedChange<TSender, T7>,
            IObservedChange<TSender, T8>,
            IObservedChange<TSender, T9>,
            IObservedChange<TSender, T10>,
            IObservedChange<TSender, T11>,
            IObservedChange<TSender, T12>,
            TRet> selector)
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
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = sender.SubscribeToExpressionChain<TSender, T1>(
            property1.Body,
            skipInitial: false);
        var o2 = sender.SubscribeToExpressionChain<TSender, T2>(
            property2.Body,
            skipInitial: false);
        var o3 = sender.SubscribeToExpressionChain<TSender, T3>(
            property3.Body,
            skipInitial: false);
        var o4 = sender.SubscribeToExpressionChain<TSender, T4>(
            property4.Body,
            skipInitial: false);
        var o5 = sender.SubscribeToExpressionChain<TSender, T5>(
            property5.Body,
            skipInitial: false);
        var o6 = sender.SubscribeToExpressionChain<TSender, T6>(
            property6.Body,
            skipInitial: false);
        var o7 = sender.SubscribeToExpressionChain<TSender, T7>(
            property7.Body,
            skipInitial: false);
        var o8 = sender.SubscribeToExpressionChain<TSender, T8>(
            property8.Body,
            skipInitial: false);
        var o9 = sender.SubscribeToExpressionChain<TSender, T9>(
            property9.Body,
            skipInitial: false);
        var o10 = sender.SubscribeToExpressionChain<TSender, T10>(
            property10.Body,
            skipInitial: false);
        var o11 = sender.SubscribeToExpressionChain<TSender, T11>(
            property11.Body,
            skipInitial: false);
        var o12 = sender.SubscribeToExpressionChain<TSender, T12>(
            property12.Body,
            skipInitial: false);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            o7,
            o8,
            o9,
            o10,
            o11,
            o12,
            (c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12) =>
                selector(c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12));
    }
}
