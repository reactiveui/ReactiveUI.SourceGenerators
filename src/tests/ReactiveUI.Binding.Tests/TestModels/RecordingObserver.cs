// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>Records what an observer receives, optionally reacting to each value.</summary>
/// <typeparam name="T">The type of the observed values.</typeparam>
/// <param name="onNext">Runs after each value is recorded, or null for nothing.</param>
internal sealed class RecordingObserver<T>(Action<T>? onNext = null) : IObserver<T>
{
    /// <summary>Gets the values received, in order.</summary>
    public List<T> Values { get; } = [];

    /// <summary>Gets the error received, or null.</summary>
    public Exception? Error { get; private set; }

    /// <summary>Gets a value indicating whether completion was received.</summary>
    public bool IsCompleted { get; private set; }

    /// <inheritdoc/>
    public void OnCompleted() => IsCompleted = true;

    /// <inheritdoc/>
    public void OnError(Exception error) => Error = error;

    /// <inheritdoc/>
    public void OnNext(T value)
    {
        Values.Add(value);
        onNext?.Invoke(value);
    }
}
