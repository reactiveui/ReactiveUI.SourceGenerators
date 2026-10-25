// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Shared helpers for the per-observable plumbing in the <c>WhenAnyObservable</c> overloads. Extracting the
/// repeated chain into one method keeps each arity overload's cyclomatic complexity low by moving the
/// null-coalescing decision out of the (otherwise straight-line) overload body.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class ObservableChainHelpers
{
    /// <summary>
    /// Observes an observable-valued property chain and switches to its latest inner observable,
    /// substituting an empty sequence whenever the property is currently null.
    /// </summary>
    /// <typeparam name="TSender">The type of the observed object.</typeparam>
    /// <typeparam name="T">The element type of the inner observable.</typeparam>
    /// <param name="sender">The object whose observable-valued property is observed.</param>
    /// <param name="observable">An expression selecting the observable-valued property.</param>
    /// <returns>An observable that switches to the latest non-null inner observable.</returns>
    [RequiresUnreferencedCode("Runtime observation fallback uses reflection-based expression analysis.")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static IObservable<T> SwitchLatest<TSender, T>(
        TSender sender,
        Expression<Func<TSender, IObservable<T>?>> observable)
        where TSender : class =>
        new SwitchMapSignal<IObservedChange<TSender, IObservable<T>?>, T>(
            sender.SubscribeToExpressionChain<TSender, IObservable<T>?>(observable.Body, skipInitial: false),
            static x => x.Value ?? ImmutableEmptySignal<T>.Instance);
}
