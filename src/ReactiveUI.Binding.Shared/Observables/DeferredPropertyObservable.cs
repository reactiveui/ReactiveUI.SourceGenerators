// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>
/// Observes a property that has no change notification by reading it when a subscriber arrives: its value at that
/// moment, once, and then silence.
/// </summary>
/// <typeparam name="TSource">The type that declares the property.</typeparam>
/// <typeparam name="TValue">The type of the property value.</typeparam>
/// <remarks>
/// Unlike <see cref="UnchangingPropertyObservable{T}"/>, which carries the value it was built with, each
/// subscription reads the property afresh. The sequence never completes, so a binding or chain that observes it
/// stays subscribed.
/// </remarks>
[DebuggerDisplay("DeferredPropertyObservable: DeferredProperty: {_source}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DeferredPropertyObservable<TSource, TValue> : IObservable<TValue>
{
    /// <summary>The object the property is read from.</summary>
    private readonly TSource _source;

    /// <summary>Reads the property from the source.</summary>
    private readonly Func<TSource, TValue> _getter;

    /// <summary>Initializes a new instance of the <see cref="DeferredPropertyObservable{TSource, TValue}"/> class.</summary>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="getter">Reads the property from the source.</param>
    /// <exception cref="ArgumentNullException"><paramref name="getter"/> is <see langword="null"/>.</exception>
    public DeferredPropertyObservable(TSource source, Func<TSource, TValue> getter)
    {
        ArgumentExceptionHelper.ThrowIfNull(getter);

        _source = source;
        _getter = getter;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<TValue> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        observer.OnNext(_getter(_source));

        return EmptyDisposable.Instance;
    }
}
