// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Emits a value on subscribe, then again each time an <see cref="EventHandler"/> event is raised.</summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// The value comes from the getter, which is read on subscribe and on every raise, so the event's arguments are
/// ignored. The value read on subscribe is always emitted. When distinct filtering is on, a later value equal to
/// the last emitted one is dropped. A getter that throws on subscribe makes <c>Subscribe</c> throw after the handler is
/// detached, and one that throws while the event is raised propagates to whoever raised it. The sequence never
/// completes.
/// </remarks>
[DebuggerDisplay("EventObservable: Getter = {_getter}, DistinctUntilChanged = {_distinctUntilChanged}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class EventObservable<T> : IObservable<T>
{
    /// <summary>Delegate that subscribes the handler to the event source.</summary>
    private readonly Action<EventHandler> _addHandler;

    /// <summary>Delegate that unsubscribes the handler from the event source.</summary>
    private readonly Action<EventHandler> _removeHandler;

    /// <summary>Delegate that reads the current property value from the source.</summary>
    private readonly Func<T> _getter;

    /// <summary>Whether to suppress duplicate consecutive values.</summary>
    private readonly bool _distinctUntilChanged;

    /// <summary>Initializes a new instance of the <see cref="EventObservable{T}"/> class.</summary>
    /// <param name="addHandler">A delegate that subscribes an <see cref="EventHandler"/> to the property change event.</param>
    /// <param name="removeHandler">A delegate that unsubscribes an <see cref="EventHandler"/> from the property change event.</param>
    /// <param name="getter">A delegate that reads the current property value.</param>
    /// <param name="distinctUntilChanged">Whether to suppress duplicate consecutive values.</param>
    /// <exception cref="ArgumentNullException"><paramref name="addHandler"/>, <paramref name="removeHandler"/> or <paramref name="getter"/> is <see langword="null"/>.</exception>
    public EventObservable(
        Action<EventHandler> addHandler,
        Action<EventHandler> removeHandler,
        Func<T> getter,
        bool distinctUntilChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(addHandler);
        ArgumentExceptionHelper.ThrowIfNull(removeHandler);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        _addHandler = addHandler;
        _removeHandler = removeHandler;
        _getter = getter;
        _distinctUntilChanged = distinctUntilChanged;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer);
    }

    /// <summary>Manages the event subscription for a single observer, with optional distinct-until-changed filtering.</summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>The parent observable that owns the add/remove handlers and getter.</summary>
        private readonly EventObservable<T> _parent;

        /// <summary>The equality comparer used for distinct-until-changed filtering.</summary>
        private readonly EqualityComparer<T> _comparer;

        /// <summary>The downstream observer. Set to <see langword="null"/> on disposal.</summary>
        private IObserver<T>? _observer;

        /// <summary>The most recently emitted value, used for distinct-until-changed comparison.</summary>
        private T _lastValue;

        /// <summary>Whether at least one value has been emitted.</summary>
        private bool _hasValue;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class. Subscribes to the event source and emits the initial property value.</summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        /// <remarks>A throw from the initial emit detaches the handler before propagating, as the caller never receives a disposable.</remarks>
        public Subscription(EventObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _comparer = EqualityComparer<T>.Default;

            parent._addHandler(OnValueChanged);

            try
            {
                // Emit initial (StartWith) value
                var initial = parent._getter();
                _lastValue = initial;
                _hasValue = true;
                observer.OnNext(initial);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!TrySetDisposed())
            {
                return;
            }

            _parent._removeHandler(OnValueChanged);
        }

        /// <summary>Atomically nulls the observer, returning whether it was previously non-null.</summary>
        /// <returns><see langword="true"/> if this is the first disposal; otherwise <see langword="false"/>.</returns>
        [ExcludeFromCodeCoverage]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TrySetDisposed() => Interlocked.Exchange(ref _observer, null) is not null;

        /// <summary>
        /// Handles the event and forwards the current property value to the observer
        /// if it passes the distinct-until-changed filter.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnValueChanged(object? sender, EventArgs e)
        {
            var observer = Volatile.Read(ref _observer);
            if (observer is null)
            {
                return;
            }

            var value = _parent._getter();

            if (_parent._distinctUntilChanged && _hasValue && _comparer.Equals(value, _lastValue))
            {
                return;
            }

            _lastValue = value;
            _hasValue = true;
            observer.OnNext(value);
        }
    }
}
