// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>The changes a two-way binding actually wrote, handed to whoever subscribes to the binding.</summary>
/// <remarks>
/// A value the binding weighed and refused to write is not reported. Each change is delivered synchronously on
/// the reporting thread to the observers subscribed when delivery starts. The sequence never completes.
/// </remarks>
[DebuggerDisplay("AppliedChangeObservable: {_observers.Length} observer(s)")]
public sealed class AppliedChangeObservable : IObservable<BindingChange>
{
    /// <summary>The observers a change is delivered to, replaced whenever the set changes.</summary>
    private IObserver<BindingChange>[] _observers = [];

    /// <summary>Gets a value indicating whether at least one observer is subscribed.</summary>
    public bool HasObservers => Volatile.Read(ref _observers).Length != 0;

    /// <summary>Reports a change the binding has written.</summary>
    /// <param name="value">The change that was written.</param>
    public void OnNext(BindingChange value)
    {
        var observers = Volatile.Read(ref _observers);
        for (var i = 0; i < observers.Length; i++)
        {
            observers[i].OnNext(value);
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is null.</exception>
    public IDisposable Subscribe(IObserver<BindingChange> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        CopyOnWriteArray.Add(ref _observers, observer);
        return new Subscription(this, observer);
    }

    /// <summary>Drops one observer without disturbing a change already being delivered.</summary>
    /// <param name="observer">The observer to drop. An observer that is not there is left alone.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Remove(IObserver<BindingChange> observer) => CopyOnWriteArray.Remove(ref _observers, observer);

    /// <summary>Releases one observer's place in the change stream.</summary>
    /// <param name="parent">The stream subscribed to.</param>
    /// <param name="observer">The observer to drop on disposal.</param>
    private sealed class Subscription(AppliedChangeObservable parent, IObserver<BindingChange> observer) : IDisposable
    {
        /// <summary>The observer to drop, cleared once dropped so disposing twice drops one place.</summary>
        private IObserver<BindingChange>? _observer = observer;

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) is not { } dropped)
            {
                return;
            }

            parent.Remove(dropped);
        }
    }
}
