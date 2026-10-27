// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ToPropertyVerification.Common;

/// <summary>A source that produces nothing until pushed, like a pending result, so a helper shows its initial value.</summary>
/// <typeparam name="T">The value type.</typeparam>
[System.Diagnostics.DebuggerDisplay("PendingSource")]
public sealed class PendingSource<T> : IObservable<T>
{
    /// <summary>The observers currently subscribed.</summary>
    private readonly List<IObserver<T>> _observers = [];

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        return new Subscription(this, observer);
    }

    /// <summary>Hands a value to every observer.</summary>
    /// <param name="value">The value.</param>
    public void Push(T value)
    {
        for (var i = 0; i < _observers.Count; i++)
        {
            _observers[i].OnNext(value);
        }
    }

    /// <summary>Removes one observer when disposed.</summary>
    /// <param name="parent">The source.</param>
    /// <param name="observer">The observer.</param>
    private sealed class Subscription(PendingSource<T> parent, IObserver<T> observer) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => parent._observers.Remove(observer);
    }
}
