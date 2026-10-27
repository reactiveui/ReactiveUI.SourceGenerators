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
/// Emits the current value of a property on subscribe, then its value as it stands before each change that an
/// <see cref="INotifyPropertyChanging"/> source announces.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// A <see cref="INotifyPropertyChanging.PropertyChanging"/> notification with a null or empty property name applies
/// to every property. Consecutive equal values are not filtered, because the value has not changed when the event
/// fires.
/// </remarks>
[DebuggerDisplay("PropertyChangingObservable: Property = {_propertyName}, Source = {_source}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PropertyChangingObservable<T> : IObservable<T>
{
    /// <summary>The source object implementing <see cref="INotifyPropertyChanging"/>.</summary>
    private readonly INotifyPropertyChanging _source;

    /// <summary>The name of the property to observe.</summary>
    private readonly string _propertyName;

    /// <summary>A delegate that reads the current property value from the source.</summary>
    private readonly Func<INotifyPropertyChanging, T?> _getter;

    /// <summary>Initializes a new instance of the <see cref="PropertyChangingObservable{T}"/> class.</summary>
    /// <param name="source">The object implementing <see cref="INotifyPropertyChanging"/>.</param>
    /// <param name="propertyName">The property name to observe.</param>
    /// <param name="getter">A delegate that reads the property value from the source.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="propertyName"/> or <paramref name="getter"/> is <see langword="null"/>.</exception>
    public PropertyChangingObservable(
        INotifyPropertyChanging source,
        string propertyName,
        Func<INotifyPropertyChanging, T?> getter)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        _source = source;
        _propertyName = propertyName;
        _getter = getter;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer).Start();
    }

    /// <summary>Manages the event subscription for a single observer.</summary>
    internal sealed class Subscription : IDisposable
    {
        /// <summary>The parent observable that owns the source and property metadata.</summary>
        private readonly PropertyChangingObservable<T> _parent;

        /// <summary>The downstream observer.</summary>
        private readonly IObserver<T> _observer;

        /// <summary>Serializes captured values and permits nested delivery before a property write.</summary>
        private SerializedDelivery<T> _delivery;

        /// <summary>Whether the subscription has been disposed.</summary>
        private int _disposed;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class.</summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(PropertyChangingObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _delivery = new(reentrant: true);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _delivery.Stop();
            _parent._source.PropertyChanging -= OnPropertyChanging;
        }

        /// <summary>Attaches notifications and reads the initial value inside the delivery gate.</summary>
        /// <returns>The subscription owning the event handler.</returns>
        internal IDisposable Start()
        {
            try
            {
                _parent._source.PropertyChanging += OnPropertyChanging;
                _delivery.Start(_observer, new Reader(this), new Drain(this));
                return this;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>Handles the <see cref="INotifyPropertyChanging.PropertyChanging"/> event and forwards the current property value to the observer.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments containing the property name.</param>
        private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName != _parent._propertyName
                && !string.IsNullOrEmpty(e.PropertyName))
            {
                return;
            }

            if (Volatile.Read(ref _disposed) == 0)
            {
                _delivery.OnNext(_observer, _parent._getter(_parent._source)!, new Drain(this));
            }
        }

        /// <summary>Reads the initial value inside the delivery gate.</summary>
        /// <param name="Owner">The subscription owning the getter.</param>
        private readonly record struct Reader(Subscription Owner) : ICurrentValueReader<T>
        {
            /// <inheritdoc/>
            public T Read() => Owner._parent._getter(Owner._parent._source)!;
        }

        /// <summary>Drains captured values into the observer.</summary>
        /// <param name="Owner">The subscription owning delivery state.</param>
        private readonly record struct Drain(Subscription Owner) : IDrainTarget
        {
            /// <inheritdoc/>
            void IDrainTarget.Drain() => _ = Owner._delivery.DrainTo(Owner._observer);
        }
    }
}
