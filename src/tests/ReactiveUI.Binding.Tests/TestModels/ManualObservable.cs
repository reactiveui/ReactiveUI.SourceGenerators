// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>
/// A test-only IObservable that stores its observer and returns a no-op disposable,
/// allowing tests to call OnNext/OnError/OnCompleted on the observer even after the
/// subscription has been "disposed" by the subscriber.
/// </summary>
/// <typeparam name="T">The element type.</typeparam>
internal sealed class ManualObservable<T> : IObservable<T>
{
    /// <summary>Gets the observer that was passed to Subscribe.</summary>
    internal IObserver<T>? Observer { get; private set; }

    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        Observer = observer;
        return new NoOpDisposable();
    }

    /// <summary>A disposable that performs no action on dispose.</summary>
    private sealed class NoOpDisposable : IDisposable
    {
        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
