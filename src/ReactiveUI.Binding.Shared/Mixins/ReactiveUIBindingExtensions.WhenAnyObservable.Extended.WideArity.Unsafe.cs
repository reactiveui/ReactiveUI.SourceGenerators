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

/// <summary>Resolves a WhenAnyObservable expression by reflection, for a call site the generator could not read.</summary>
/// <remarks>
/// The opt-in half of <c>WhenAnyObservable</c>. The unsuffixed overloads resolve at compile time and throw when no generated
/// dispatch claimed the call site; these walk the chain by reflection instead, and are annotated so a consumer
/// publishing trimmed or ahead-of-time sees the requirement at their own call site.
/// </remarks>
public static partial class ReactiveUIBindingExtensions
{
    /// <summary>
    /// Observes 3 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Func<T1?, T2?, T3?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            (v1, v2, v3) => selector(v1, v2, v3));
    }

    /// <summary>
    /// Observes 4 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Func<T1?, T2?, T3?, T4?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            (v1, v2, v3, v4) => selector(v1, v2, v3, v4));
    }

    /// <summary>
    /// Observes 5 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4, T5>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Func<T1?, T2?, T3?, T4?, T5?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        var o5 = ObservableChainHelpers.SwitchLatest(sender, obs5);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            (v1, v2, v3, v4, v5) => selector(v1, v2, v3, v4, v5));
    }

    /// <summary>
    /// Observes 6 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        var o5 = ObservableChainHelpers.SwitchLatest(sender, obs5);
        var o6 = ObservableChainHelpers.SwitchLatest(sender, obs6);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            (v1, v2, v3, v4, v5, v6) => selector(v1, v2, v3, v4, v5, v6));
    }

    /// <summary>
    /// Observes 7 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        var o5 = ObservableChainHelpers.SwitchLatest(sender, obs5);
        var o6 = ObservableChainHelpers.SwitchLatest(sender, obs6);
        var o7 = ObservableChainHelpers.SwitchLatest(sender, obs7);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            o7,
            (v1, v2, v3, v4, v5, v6, v7) => selector(v1, v2, v3, v4, v5, v6, v7));
    }

    /// <summary>
    /// Observes 8 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        var o5 = ObservableChainHelpers.SwitchLatest(sender, obs5);
        var o6 = ObservableChainHelpers.SwitchLatest(sender, obs6);
        var o7 = ObservableChainHelpers.SwitchLatest(sender, obs7);
        var o8 = ObservableChainHelpers.SwitchLatest(sender, obs8);
        return CombineLatestObservable.Create(
            o1,
            o2,
            o3,
            o4,
            o5,
            o6,
            o7,
            o8,
            (v1, v2, v3, v4, v5, v6, v7, v8) => selector(v1, v2, v3, v4, v5, v6, v7, v8));
    }

    /// <summary>
    /// Observes 9 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <typeparam name="T9">The element type of observable property 9.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Expression<Func<TSender, IObservable<T9>?>> obs9,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(obs9);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        var o5 = ObservableChainHelpers.SwitchLatest(sender, obs5);
        var o6 = ObservableChainHelpers.SwitchLatest(sender, obs6);
        var o7 = ObservableChainHelpers.SwitchLatest(sender, obs7);
        var o8 = ObservableChainHelpers.SwitchLatest(sender, obs8);
        var o9 = ObservableChainHelpers.SwitchLatest(sender, obs9);
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
            (v1, v2, v3, v4, v5, v6, v7, v8, v9) => selector(v1, v2, v3, v4, v5, v6, v7, v8, v9));
    }

    /// <summary>
    /// Observes 10 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <typeparam name="T9">The element type of observable property 9.</typeparam>
    /// <typeparam name="T10">The element type of observable property 10.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Expression<Func<TSender, IObservable<T9>?>> obs9,
        Expression<Func<TSender, IObservable<T10>?>> obs10,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(obs9);
        ArgumentExceptionHelper.ThrowIfNull(obs10);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        var o5 = ObservableChainHelpers.SwitchLatest(sender, obs5);
        var o6 = ObservableChainHelpers.SwitchLatest(sender, obs6);
        var o7 = ObservableChainHelpers.SwitchLatest(sender, obs7);
        var o8 = ObservableChainHelpers.SwitchLatest(sender, obs8);
        var o9 = ObservableChainHelpers.SwitchLatest(sender, obs9);
        var o10 = ObservableChainHelpers.SwitchLatest(sender, obs10);
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
            selector);
    }

    /// <summary>
    /// Observes 11 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <typeparam name="T9">The element type of observable property 9.</typeparam>
    /// <typeparam name="T10">The element type of observable property 10.</typeparam>
    /// <typeparam name="T11">The element type of observable property 11.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <param name="obs11">An expression that selects observable property 11 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Expression<Func<TSender, IObservable<T9>?>> obs9,
        Expression<Func<TSender, IObservable<T10>?>> obs10,
        Expression<Func<TSender, IObservable<T11>?>> obs11,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(obs9);
        ArgumentExceptionHelper.ThrowIfNull(obs10);
        ArgumentExceptionHelper.ThrowIfNull(obs11);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        var o5 = ObservableChainHelpers.SwitchLatest(sender, obs5);
        var o6 = ObservableChainHelpers.SwitchLatest(sender, obs6);
        var o7 = ObservableChainHelpers.SwitchLatest(sender, obs7);
        var o8 = ObservableChainHelpers.SwitchLatest(sender, obs8);
        var o9 = ObservableChainHelpers.SwitchLatest(sender, obs9);
        var o10 = ObservableChainHelpers.SwitchLatest(sender, obs10);
        var o11 = ObservableChainHelpers.SwitchLatest(sender, obs11);
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
            selector);
    }

    /// <summary>
    /// Observes 12 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <typeparam name="T3">The element type of observable property 3.</typeparam>
    /// <typeparam name="T4">The element type of observable property 4.</typeparam>
    /// <typeparam name="T5">The element type of observable property 5.</typeparam>
    /// <typeparam name="T6">The element type of observable property 6.</typeparam>
    /// <typeparam name="T7">The element type of observable property 7.</typeparam>
    /// <typeparam name="T8">The element type of observable property 8.</typeparam>
    /// <typeparam name="T9">The element type of observable property 9.</typeparam>
    /// <typeparam name="T10">The element type of observable property 10.</typeparam>
    /// <typeparam name="T11">The element type of observable property 11.</typeparam>
    /// <typeparam name="T12">The element type of observable property 12.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <param name="obs9">An expression that selects observable property 9 to observe.</param>
    /// <param name="obs10">An expression that selects observable property 10 to observe.</param>
    /// <param name="obs11">An expression that selects observable property 11 to observe.</param>
    /// <param name="obs12">An expression that selects observable property 12 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Expression<Func<TSender, IObservable<T3>?>> obs3,
        Expression<Func<TSender, IObservable<T4>?>> obs4,
        Expression<Func<TSender, IObservable<T5>?>> obs5,
        Expression<Func<TSender, IObservable<T6>?>> obs6,
        Expression<Func<TSender, IObservable<T7>?>> obs7,
        Expression<Func<TSender, IObservable<T8>?>> obs8,
        Expression<Func<TSender, IObservable<T9>?>> obs9,
        Expression<Func<TSender, IObservable<T10>?>> obs10,
        Expression<Func<TSender, IObservable<T11>?>> obs11,
        Expression<Func<TSender, IObservable<T12>?>> obs12,
        Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, T11?, T12?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);
        ArgumentExceptionHelper.ThrowIfNull(obs7);
        ArgumentExceptionHelper.ThrowIfNull(obs8);
        ArgumentExceptionHelper.ThrowIfNull(obs9);
        ArgumentExceptionHelper.ThrowIfNull(obs10);
        ArgumentExceptionHelper.ThrowIfNull(obs11);
        ArgumentExceptionHelper.ThrowIfNull(obs12);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        var o3 = ObservableChainHelpers.SwitchLatest(sender, obs3);
        var o4 = ObservableChainHelpers.SwitchLatest(sender, obs4);
        var o5 = ObservableChainHelpers.SwitchLatest(sender, obs5);
        var o6 = ObservableChainHelpers.SwitchLatest(sender, obs6);
        var o7 = ObservableChainHelpers.SwitchLatest(sender, obs7);
        var o8 = ObservableChainHelpers.SwitchLatest(sender, obs8);
        var o9 = ObservableChainHelpers.SwitchLatest(sender, obs9);
        var o10 = ObservableChainHelpers.SwitchLatest(sender, obs10);
        var o11 = ObservableChainHelpers.SwitchLatest(sender, obs11);
        var o12 = ObservableChainHelpers.SwitchLatest(sender, obs12);
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
            selector);
    }
}
