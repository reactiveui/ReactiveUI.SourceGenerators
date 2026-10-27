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
    /// Observes an observable-valued property by reflection and emits the values of its current observable, switching when the property changes; a null observable emits nothing.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <returns>An observable sequence that emits values from the latest observed observable.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);

        return ObservableChainHelpers.SwitchLatest(sender, obs1);
    }

    /// <summary>
    /// Observes 2 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 3 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 4 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 5 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 6 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(obs3);
        ArgumentExceptionHelper.ThrowIfNull(obs4);
        ArgumentExceptionHelper.ThrowIfNull(obs5);
        ArgumentExceptionHelper.ThrowIfNull(obs6);

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 7 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7)
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

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 8 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="obs3">An expression that selects observable property 3 to observe.</param>
    /// <param name="obs4">An expression that selects observable property 4 to observe.</param>
    /// <param name="obs5">An expression that selects observable property 5 to observe.</param>
    /// <param name="obs6">An expression that selects observable property 6 to observe.</param>
    /// <param name="obs7">An expression that selects observable property 7 to observe.</param>
    /// <param name="obs8">An expression that selects observable property 8 to observe.</param>
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8)
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

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 9 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
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
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9)
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

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
            sender.WhenAnyObservableUnsafe(obs9),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 10 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
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
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10)
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

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
            sender.WhenAnyObservableUnsafe(obs9),
            sender.WhenAnyObservableUnsafe(obs10),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 11 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
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
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10,
        Expression<Func<TSender, IObservable<TRet>?>> obs11)
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

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
            sender.WhenAnyObservableUnsafe(obs9),
            sender.WhenAnyObservableUnsafe(obs10),
            sender.WhenAnyObservableUnsafe(obs11),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 12 observable-valued properties by reflection and merges the values of their current observables, each switching when its property changes.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The element type of the observed observables.</typeparam>
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
    /// <returns>An observable sequence that emits values from the merged observed observables.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [SuppressMessage("Design", "SST1523", Justification = "one observation step per observed property; the length is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet>(
        this TSender sender,
        Expression<Func<TSender, IObservable<TRet>?>> obs1,
        Expression<Func<TSender, IObservable<TRet>?>> obs2,
        Expression<Func<TSender, IObservable<TRet>?>> obs3,
        Expression<Func<TSender, IObservable<TRet>?>> obs4,
        Expression<Func<TSender, IObservable<TRet>?>> obs5,
        Expression<Func<TSender, IObservable<TRet>?>> obs6,
        Expression<Func<TSender, IObservable<TRet>?>> obs7,
        Expression<Func<TSender, IObservable<TRet>?>> obs8,
        Expression<Func<TSender, IObservable<TRet>?>> obs9,
        Expression<Func<TSender, IObservable<TRet>?>> obs10,
        Expression<Func<TSender, IObservable<TRet>?>> obs11,
        Expression<Func<TSender, IObservable<TRet>?>> obs12)
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

        IObservable<TRet>[] sources =
        [
            sender.WhenAnyObservableUnsafe(obs1),
            sender.WhenAnyObservableUnsafe(obs2),
            sender.WhenAnyObservableUnsafe(obs3),
            sender.WhenAnyObservableUnsafe(obs4),
            sender.WhenAnyObservableUnsafe(obs5),
            sender.WhenAnyObservableUnsafe(obs6),
            sender.WhenAnyObservableUnsafe(obs7),
            sender.WhenAnyObservableUnsafe(obs8),
            sender.WhenAnyObservableUnsafe(obs9),
            sender.WhenAnyObservableUnsafe(obs10),
            sender.WhenAnyObservableUnsafe(obs11),
            sender.WhenAnyObservableUnsafe(obs12),
        ];

        return new EnumerableBlendSignal<TRet>(sources);
    }

    /// <summary>
    /// Observes 2 observable-valued properties of different element types by reflection and applies a selector to their latest values once every observable has produced one.
    /// </summary>
    /// <typeparam name="TSender">The type of the sender to monitor for property changes.</typeparam>
    /// <typeparam name="TRet">The return type of the selector.</typeparam>
    /// <typeparam name="T1">The element type of observable property 1.</typeparam>
    /// <typeparam name="T2">The element type of observable property 2.</typeparam>
    /// <param name="sender">The sender instance to observe for property changes.</param>
    /// <param name="obs1">An expression that selects observable property 1 to observe.</param>
    /// <param name="obs2">An expression that selects observable property 2 to observe.</param>
    /// <param name="selector">A function that combines the latest values from all observables.</param>
    /// <returns>An observable sequence of selector results.</returns>
    [SuppressMessage("Design", "SST1472", Justification = "one selector per observed property; the parameter count is the shape of this overload")]
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    public static IObservable<TRet> WhenAnyObservableUnsafe<TSender, TRet, T1, T2>(
        this TSender sender,
        Expression<Func<TSender, IObservable<T1>?>> obs1,
        Expression<Func<TSender, IObservable<T2>?>> obs2,
        Func<T1?, T2?, TRet> selector)
        where TSender : class
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(obs1);
        ArgumentExceptionHelper.ThrowIfNull(obs2);
        ArgumentExceptionHelper.ThrowIfNull(selector);

        var o1 = ObservableChainHelpers.SwitchLatest(sender, obs1);
        var o2 = ObservableChainHelpers.SwitchLatest(sender, obs2);
        return CombineLatestObservable.Create(
            o1,
            o2,
            (v1, v2) => selector(v1, v2));
    }
}
