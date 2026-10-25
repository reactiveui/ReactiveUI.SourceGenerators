// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.InteropServices;
using Foundation;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Observes an Apple object's property through key-value observing.</summary>
/// <typeparam name="T">The type of the property value.</typeparam>
/// <remarks>
/// Each subscription registers its own observer for the key path and delivers the current value straight away. The
/// sequence never completes. Disposing a subscription removes the observer once, however many times it is disposed.
/// </remarks>
[DebuggerDisplay("KvoPropertyObservable: KvoProperty: {_keyPath}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class KvoPropertyObservable<T> : IObservable<T>
{
    /// <summary>The object whose property is observed.</summary>
    private readonly NSObject _source;

    /// <summary>The key path of the observed property.</summary>
    private readonly NSString _keyPath;

    /// <summary>Reads the property from the source.</summary>
    private readonly Func<NSObject, T> _getter;

    /// <summary>Whether a value equal to the last one delivered is suppressed.</summary>
    private readonly bool _distinct;

    /// <summary>Which value the observer registration asks KVO to report.</summary>
    private readonly NSKeyValueObservingOptions _options;

    /// <summary>Initializes a new instance of the <see cref="KvoPropertyObservable{T}"/> class.</summary>
    /// <param name="source">The object whose property is observed.</param>
    /// <param name="keyPath">The key path of the observed property.</param>
    /// <param name="getter">Reads the property from the source.</param>
    /// <param name="distinct">Whether a value equal to the last one delivered is suppressed.</param>
    /// <param name="beforeChange">Whether the observation reports the value before a change rather than after it.</param>
    /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="keyPath"/> or <paramref name="getter"/> is <see langword="null"/>.</exception>
    public KvoPropertyObservable(NSObject source, string keyPath, Func<NSObject, T> getter, bool distinct, bool beforeChange)
    {
        ArgumentExceptionHelper.ThrowIfNull(source);
        ArgumentExceptionHelper.ThrowIfNull(keyPath);
        ArgumentExceptionHelper.ThrowIfNull(getter);

        _source = source;
        _keyPath = (NSString)keyPath;
        _getter = getter;
        _distinct = distinct;
        _options = beforeChange ? NSKeyValueObservingOptions.Old : NSKeyValueObservingOptions.New;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer);
    }

    /// <summary>One subscriber's key-value observer registration.</summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>The observation this subscription belongs to.</summary>
        private readonly KvoPropertyObservable<T> _parent;

        /// <summary>The native observer registered for the key path.</summary>
        private readonly KvoCallbackObserver _kvoObserver;

        /// <summary>Keeps the native observer alive while it is registered.</summary>
        private readonly GCHandle _handle;

        /// <summary>The subscriber, or null once disposed.</summary>
        private IObserver<T>? _observer;

        /// <summary>The last value delivered.</summary>
        private T _lastValue;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class, registering and delivering the current value.</summary>
        /// <param name="parent">The observation this subscription belongs to.</param>
        /// <param name="observer">The subscriber.</param>
        public Subscription(KvoPropertyObservable<T> parent, IObserver<T> observer)
        {
            _parent = parent;
            _observer = observer;
            _kvoObserver = new(OnValueChanged);
            _handle = GCHandle.Alloc(_kvoObserver);

            parent._source.AddObserver(_kvoObserver, parent._keyPath, parent._options, IntPtr.Zero);

            _lastValue = parent._getter(parent._source);
            observer.OnNext(_lastValue);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) is null)
            {
                return;
            }

            _parent._source.RemoveObserver(_kvoObserver, _parent._keyPath);
            _handle.Free();
            _kvoObserver.Dispose();
        }

        /// <summary>Reads the property and delivers it, unless disposed or unchanged under distinct delivery.</summary>
        private void OnValueChanged()
        {
            var observer = Volatile.Read(ref _observer);
            if (observer is null)
            {
                return;
            }

            var value = _parent._getter(_parent._source);
            if (_parent._distinct && EqualityComparer<T>.Default.Equals(value, _lastValue))
            {
                return;
            }

            _lastValue = value;
            observer.OnNext(value);
        }
    }
}
