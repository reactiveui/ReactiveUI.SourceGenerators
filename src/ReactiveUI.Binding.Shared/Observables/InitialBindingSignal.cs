// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Wires both binding directions before emitting the initial view model signal.</summary>
/// <param name="source">The combined direction signals.</param>
internal sealed class InitialBindingSignal(IObservable<bool> source) : IObservable<bool>
{
    /// <inheritdoc/>
    public IDisposable Subscribe(IObserver<bool> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);
        return new Subscription(observer).Start(source);
    }

    /// <summary>Suppresses attachment snapshots and serializes updates behind the initial signal.</summary>
    /// <param name="observer">The observer receiving direction signals.</param>
    private sealed class Subscription(IObserver<bool> observer) : IObserver<bool>, IDisposable
    {
        /// <summary>The downstream observer.</summary>
        private readonly IObserver<bool> _observer = observer;

        /// <summary>Owns the combined notification subscription.</summary>
        private readonly SwapDisposable _upstream = new();

        /// <summary>The mutable delivery state, called in place.</summary>
        private SerializedDelivery<bool> _delivery = new();

        /// <summary>Whether both directions have been attached.</summary>
        private bool _ready;

        /// <inheritdoc/>
        public void Dispose()
        {
            _delivery.Stop();
            _upstream.Dispose();
        }

        /// <inheritdoc/>
        public void OnNext(bool value)
        {
            if (Volatile.Read(ref _ready))
            {
                _delivery.OnNext(_observer, value, new Drain(this));
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => _delivery.OnError(error, new Drain(this));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => _delivery.OnCompleted(new Drain(this));

        /// <summary>Reserves the initial delivery while attaching the source.</summary>
        /// <param name="source">The notifications to attach.</param>
        /// <returns>This subscription.</returns>
        internal Subscription Start(IObservable<bool> source)
        {
            _ = _delivery.TryClaim();
            try
            {
                _upstream.Disposable = source.Subscribe(this);
                Volatile.Write(ref _ready, true);
                _delivery.DeliverClaimed(_observer, true, new Drain(this));
                return this;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>Drains pending directions and releases a terminated source.</summary>
        /// <param name="Owner">The subscription holding delivery state.</param>
        private readonly record struct Drain(Subscription Owner) : IDrainTarget
        {
            /// <inheritdoc/>
            void IDrainTarget.Drain()
            {
                if (Owner._delivery.DrainTo(Owner._observer))
                {
                    Owner._upstream.Dispose();
                }
            }
        }
    }
}
