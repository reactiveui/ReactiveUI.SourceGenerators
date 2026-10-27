// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Avalonia.Tests;

/// <summary>An observable the test pushes values and errors into, with one subscriber at a time.</summary>
public sealed class ValueSource : IObservable<object?>
{
    /// <summary>The current subscriber, or null when none is subscribed.</summary>
    private IObserver<object?>? _observer;

    /// <summary>Gets a value indicating whether a subscriber is attached.</summary>
    public bool HasObserver => _observer is not null;

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<object?> observer)
    {
        _observer = observer;
        return new Unsubscriber(this);
    }

    /// <summary>Pushes a value to the subscriber.</summary>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push(object? value) => _observer?.OnNext(value);

    /// <summary>Pushes an error to the subscriber.</summary>
    /// <param name="error">The error.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Fail(Exception error) => _observer?.OnError(error);

    /// <summary>Completes the subscriber.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Complete() => _observer?.OnCompleted();

    /// <summary>Detaches the subscriber when disposed.</summary>
    /// <param name="owner">The source to detach from.</param>
    private sealed class Unsubscriber(ValueSource owner) : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose() => owner._observer = null;
    }
}
