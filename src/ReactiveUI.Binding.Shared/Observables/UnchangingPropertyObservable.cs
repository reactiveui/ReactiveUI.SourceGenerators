// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Observes a property that has no change notification: its value, once, and then silence.</summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// The sequence never completes, so a binding or chain that observes it stays subscribed.
/// </remarks>
[DebuggerDisplay("UnchangingPropertyObservable: UnchangingProperty: {_value}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class UnchangingPropertyObservable<T> : IObservable<T>
{
    /// <summary>The only value this observation will ever carry.</summary>
    private readonly T _value;

    /// <summary>Initializes a new instance of the <see cref="UnchangingPropertyObservable{T}"/> class.</summary>
    /// <param name="value">The property's current value, which is also its final one.</param>
    public UnchangingPropertyObservable(T value) => _value = value;

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        observer.OnNext(_value);

        return new Subscription();
    }

    /// <summary>
    /// The teardown for a subscription that holds nothing. Disposing it is what a caller does with the
    /// handle it was given; there is no handler to unhook and no completion to suppress.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
