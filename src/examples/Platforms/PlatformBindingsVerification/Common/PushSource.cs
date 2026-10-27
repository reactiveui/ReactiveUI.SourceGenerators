// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace PlatformBindingsVerification.Common;

/// <summary>A minimal multicast <see cref="IObservable{T}"/> a scenario can push values into from any thread.</summary>
/// <typeparam name="T">The type of value pushed.</typeparam>
[System.Diagnostics.DebuggerDisplay("PushSource")]
public sealed class PushSource<T> : IObservable<T>
{
    /// <summary>The lock guarding <see cref="_observers"/>.</summary>
    private readonly Lock _gate = new();

    /// <summary>The observers currently subscribed.</summary>
    private List<IObserver<T>> _observers = [];

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        lock (_gate)
        {
            _observers = [.. _observers, observer];
        }

        return new Unsubscriber(this, observer);
    }

    /// <summary>Pushes a value to every current subscriber.</summary>
    /// <param name="value">The value to push.</param>
    public void OnNext(T value)
    {
        IObserver<T>[] snapshot;
        lock (_gate)
        {
            snapshot = [.. _observers];
        }

        foreach (var observer in snapshot)
        {
            observer.OnNext(value);
        }
    }

    /// <summary>Removes an observer from <see cref="_observers"/> when it disposes its subscription.</summary>
    /// <param name="observer">The observer to remove.</param>
    private void Remove(IObserver<T> observer)
    {
        lock (_gate)
        {
            _observers = [.. _observers.Where(o => !ReferenceEquals(o, observer))];
        }
    }

    /// <summary>Disposes a single subscription created by <see cref="Subscribe"/>.</summary>
    /// <param name="owner">The <see cref="PushSource{T}"/> the subscription belongs to.</param>
    /// <param name="observer">The observer to remove from <paramref name="owner"/> on disposal.</param>
    [System.Diagnostics.DebuggerDisplay("Unsubscriber")]
    private sealed class Unsubscriber(PushSource<T> owner, IObserver<T> observer) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => owner.Remove(observer);
    }
}
