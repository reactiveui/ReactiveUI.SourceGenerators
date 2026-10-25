// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Emits a property's value on subscribe, then its new value after each change the source announces.</summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// A <see cref="INotifyPropertyChanged.PropertyChanged"/> notification with a null or empty property name applies
/// to every property. When distinct filtering is on, a value equal to the last emitted one is dropped.
/// </remarks>
[DebuggerDisplay("PropertyObservable: Property = {_propertyName}, Source = {_source}, DistinctUntilChanged = {_distinctUntilChanged}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PropertyObservable<T> : IObservable<T>
{
    /// <summary>The source object implementing <see cref="INotifyPropertyChanged"/>.</summary>
    private readonly INotifyPropertyChanged _source;

    /// <summary>The name of the property to observe.</summary>
    private readonly string _propertyName;

    /// <summary>A delegate that reads the current property value from the source. The value may be <see langword="null"/> for reference-typed properties.</summary>
    private readonly Func<INotifyPropertyChanged, T?> _getter;

    /// <summary>Whether to suppress duplicate consecutive values.</summary>
    private readonly bool _distinctUntilChanged;

    /// <summary>Initializes a new instance of the <see cref="PropertyObservable{T}"/> class.</summary>
    /// <param name="source">The object implementing <see cref="INotifyPropertyChanged"/>.</param>
    /// <param name="propertyName">The property name to observe.</param>
    /// <param name="getter">A delegate that reads the property value from the source.</param>
    /// <param name="distinctUntilChanged">Whether to suppress duplicate consecutive values.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="propertyName"/> or <paramref name="getter"/> is <see langword="null"/>.</exception>
    public PropertyObservable(
        INotifyPropertyChanged source,
        string propertyName,
        Func<INotifyPropertyChanged, T?> getter,
        bool distinctUntilChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(propertyName);
        ArgumentExceptionHelper.ThrowIfNull(getter);
        _source = source;
        _propertyName = propertyName;
        _getter = getter;
        _distinctUntilChanged = distinctUntilChanged;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer).Start();
    }

    /// <summary>Manages the event subscription for a single observer, with optional distinct-until-changed filtering.</summary>
    internal sealed class Subscription : IDisposable
    {
        /// <summary>The parent observable that owns the source and property metadata.</summary>
        private readonly PropertyObservable<T> _parent;

        /// <summary>The downstream observer.</summary>
        private readonly IObserver<T> _observer;

        /// <summary>Serializes reads and delivery without holding a lock across the observer.</summary>
        private CurrentValueDelivery<T> _delivery;

        /// <summary>Whether this subscription has been disposed.</summary>
        private int _disposed;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class.</summary>
        /// <param name="parent">The parent observable.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(PropertyObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _delivery = new(parent._distinctUntilChanged);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _delivery.Stop();
            _parent._source.PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>Attaches notifications before reading the initial value inside the delivery gate.</summary>
        /// <returns>The subscription owning the event handler.</returns>
        internal IDisposable Start()
        {
            try
            {
                _parent._source.PropertyChanged += OnPropertyChanged;
                _delivery.Start(new Drain(this));
                return this;
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Handles the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// and forwards the current property value to the observer if it passes the distinct-until-changed filter.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments containing the property name.</param>
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != _parent._propertyName
                && !string.IsNullOrEmpty(e.PropertyName))
            {
                return;
            }

            _delivery.Changed(new Drain(this));
        }

        /// <summary>Reads the current property value inside the delivery gate.</summary>
        /// <param name="Owner">The subscription owning the getter.</param>
        private readonly record struct Reader(Subscription Owner) : ICurrentValueReader<T>
        {
            /// <inheritdoc/>
            public T Read() => Owner._parent._getter(Owner._parent._source)!;
        }

        /// <summary>Drains pending changes into the observer.</summary>
        /// <param name="Owner">The subscription owning delivery state.</param>
        private readonly record struct Drain(Subscription Owner) : IDrainTarget
        {
            /// <inheritdoc/>
            void IDrainTarget.Drain() => _ = Owner._delivery.DrainTo(Owner._observer, new Reader(Owner));
        }
    }
}
