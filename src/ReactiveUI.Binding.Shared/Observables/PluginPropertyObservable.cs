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
/// Observes one property through a registered <see cref="ICreatesObservableForProperty"/> that outranks the
/// mechanism the generator selected, reading the value through a generated accessor.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// The plugin says when the property changed; the value is read with the supplied getter, never from the
/// notification. The current value is emitted on subscribe and again after each notification. An error or
/// completion from the plugin is passed on after any value still waiting to be read.
/// </remarks>
[DebuggerDisplay("PluginPropertyObservable: Property = {_propertyName}, Source = {_source}, BeforeChange = {_beforeChange}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PluginPropertyObservable<T> : IObservable<T>
{
    /// <summary>The registration that outranked the generated mechanism.</summary>
    private readonly ICreatesObservableForProperty _plugin;

    /// <summary>The object the property is read from.</summary>
    private readonly object _source;

    /// <summary>The property as the call site named it, handed to the plugin unchanged.</summary>
    private readonly Expression _expression;

    /// <summary>The name of the property being observed.</summary>
    private readonly string _propertyName;

    /// <summary>Reads the current property value from the source.</summary>
    private readonly Func<object, T?> _getter;

    /// <summary>Whether before-change notifications are being observed.</summary>
    private readonly bool _beforeChange;

    /// <summary>Whether to suppress duplicate consecutive values.</summary>
    private readonly bool _distinctUntilChanged;

    /// <summary>Initializes a new instance of the <see cref="PluginPropertyObservable{T}"/> class.</summary>
    /// <param name="plugin">The registration that outranked the generated mechanism.</param>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="expression">The property as the call site named it.</param>
    /// <param name="propertyName">The name of the property being observed.</param>
    /// <param name="getter">Reads the current property value from the source.</param>
    /// <param name="beforeChange">Whether before-change notifications are being observed.</param>
    /// <param name="distinctUntilChanged">Whether to suppress duplicate consecutive values.</param>
    /// <exception cref="ArgumentNullException">Any reference-type argument is <see langword="null"/>.</exception>
    public PluginPropertyObservable(
        ICreatesObservableForProperty plugin,
        object source,
        Expression expression,
        string propertyName,
        Func<object, T?> getter,
        bool beforeChange,
        bool distinctUntilChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(plugin);
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        _plugin = plugin;
        _source = source;
        _expression = expression;
        _propertyName = propertyName;
        _getter = getter;
        _beforeChange = beforeChange;
        _distinctUntilChanged = distinctUntilChanged;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer).Start();
    }

    /// <summary>Relays one observer's subscription to the plugin, with the distinct gate applied inline.</summary>
    internal sealed class Subscription : IDisposable, IObserver<IObservedChange<object, object?>>
    {
        /// <summary>The parent observable that owns the source and property metadata.</summary>
        private readonly PluginPropertyObservable<T> _parent;

        /// <summary>The downstream observer.</summary>
        private IObserver<T> _observer;

        /// <summary>The plugin's own subscription, dropped when this one is.</summary>
        private IDisposable _inner = EmptyDisposable.Instance;

        /// <summary>Serializes current-value reads and terminal notifications.</summary>
        private CurrentValueDelivery<T> _delivery;

        /// <summary>Whether this subscription has been disposed.</summary>
        private int _disposed;

        /// <summary>Whether a provider notification is waiting to be read.</summary>
        private int _pendingChange;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class.</summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(PluginPropertyObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _delivery = new(parent._distinctUntilChanged);
        }

        /// <inheritdoc/>
        public void OnNext(IObservedChange<object, object?> value)
        {
            if (_delivery.IsTerminated)
            {
                return;
            }

            Volatile.Write(ref _pendingChange, 1);
            _delivery.Changed(new Drain(this));
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => _delivery.Fault(error, new Drain(this));

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => _delivery.Complete(new Drain(this));

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _delivery.Stop();
            _inner.Dispose();
        }

        /// <summary>Attaches the provider before delivering the initial value.</summary>
        /// <returns>The subscription owning the provider registration.</returns>
        internal IDisposable Start()
        {
            _observer = new DeliveryObserver(this, _observer);
            try
            {
                _inner = _parent._plugin.GetNotificationForProperty(
                    _parent._source,
                    _parent._expression,
                    _parent._propertyName,
                    _parent._beforeChange,
                    false).Subscribe(this);
                _delivery.Start(new Drain(this));
                return this;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>Reads the current property value inside the delivery gate.</summary>
        /// <param name="Owner">The subscription owning the getter.</param>
        private readonly record struct Reader(Subscription Owner) : ICurrentValueReader<T>
        {
            /// <inheritdoc/>
            public T Read()
            {
                _ = Interlocked.Exchange(ref Owner._pendingChange, 0);
                return Owner._parent._getter(Owner._parent._source)!;
            }
        }

        /// <summary>Drains pending changes and the terminal notification into the observer.</summary>
        /// <param name="Owner">The subscription owning delivery state.</param>
        private readonly record struct Drain(Subscription Owner) : IDrainTarget
        {
            /// <inheritdoc/>
            void IDrainTarget.Drain() => _ = Owner._delivery.DrainTo(Owner._observer, new Reader(Owner));
        }

        /// <summary>Reads a change raised inside the current delivery before passing on its terminal notification.</summary>
        /// <param name="owner">The subscription holding the pending read.</param>
        /// <param name="observer">The downstream observer.</param>
        private sealed class DeliveryObserver(Subscription owner, IObserver<T> observer) : IObserver<T>
        {
            /// <summary>The last value emitted by the delivery gate.</summary>
            private T? _lastValue;

            /// <inheritdoc/>
            public void OnNext(T value)
            {
                _lastValue = value;
                observer.OnNext(value);
            }

            /// <inheritdoc/>
            public void OnError(Exception error)
            {
                DeliverPendingValue();
                if (Volatile.Read(ref owner._disposed) == 0)
                {
                    observer.OnError(error);
                }
            }

            /// <inheritdoc/>
            public void OnCompleted()
            {
                DeliverPendingValue();
                if (Volatile.Read(ref owner._disposed) == 0)
                {
                    observer.OnCompleted();
                }
            }

            /// <summary>Delivers a pending current value while the terminal notification owns the delivery gate.</summary>
            private void DeliverPendingValue()
            {
                if (Interlocked.Exchange(ref owner._pendingChange, 0) == 0)
                {
                    return;
                }

                var value = owner._parent._getter(owner._parent._source)!;
                if (Volatile.Read(ref owner._disposed) != 0)
                {
                    return;
                }

                if (!owner._parent._distinctUntilChanged || !EqualityComparer<T>.Default.Equals(value, _lastValue!))
                {
                    observer.OnNext(value);
                }
            }
        }
    }
}
