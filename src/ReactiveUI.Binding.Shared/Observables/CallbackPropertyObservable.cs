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
/// Observes a property through a platform's own change callback: the value on subscription, then again each time
/// the callback fires.
/// </summary>
/// <typeparam name="TSource">The type that declares the property.</typeparam>
/// <typeparam name="TValue">The type of the property value.</typeparam>
/// <remarks>
/// Generated observation of a native mechanism - a UIKit control event, a WinForms companion event, a WinUI or Uno
/// dependency property, an Android listener - passes the statements that attach to the notification as
/// <c>subscribe</c>. The sequence never completes. Disposing a subscription detaches it once, however many times it
/// is disposed.
/// </remarks>
[DebuggerDisplay("CallbackPropertyObservable: CallbackProperty: {_source}, Distinct = {_distinct}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CallbackPropertyObservable<TSource, TValue> : IObservable<TValue>
{
    /// <summary>The object the property is read from.</summary>
    private readonly TSource _source;

    /// <summary>Attaches a callback to the platform's notification and returns what detaches it.</summary>
    private readonly Func<TSource, Action, IDisposable> _subscribe;

    /// <summary>Reads the property from the source.</summary>
    private readonly Func<TSource, TValue> _getter;

    /// <summary>Whether a value equal to the last one delivered is suppressed.</summary>
    private readonly bool _distinct;

    /// <summary>Initializes a new instance of the <see cref="CallbackPropertyObservable{TSource, TValue}"/> class.</summary>
    /// <param name="source">The object the property is read from.</param>
    /// <param name="subscribe">Attaches a callback to the platform's notification and returns what detaches it.</param>
    /// <param name="getter">Reads the property from the source.</param>
    /// <param name="distinct">Whether a value equal to the last one delivered is suppressed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="subscribe"/> or <paramref name="getter"/> is <see langword="null"/>.</exception>
    public CallbackPropertyObservable(
        TSource source,
        Func<TSource, Action, IDisposable> subscribe,
        Func<TSource, TValue> getter,
        bool distinct)
    {
        ArgumentExceptionHelper.ThrowIfNull(subscribe);
        ArgumentExceptionHelper.ThrowIfNull(getter);

        _source = source;
        _subscribe = subscribe;
        _getter = getter;
        _distinct = distinct;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<TValue> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(this, observer);
    }

    /// <summary>One subscriber's attachment to the platform notification.</summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>The observation this subscription belongs to.</summary>
        private readonly CallbackPropertyObservable<TSource, TValue> _parent;

        /// <summary>Detaches the callback from the platform notification.</summary>
        private readonly IDisposable _attachment;

        /// <summary>The subscriber, or null once disposed.</summary>
        private IObserver<TValue>? _observer;

        /// <summary>The last value delivered.</summary>
        private TValue _lastValue = default!;

        /// <summary>Whether a value has been delivered.</summary>
        private bool _hasValue;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class, attaching and delivering the current value.</summary>
        /// <param name="parent">The observation this subscription belongs to.</param>
        /// <param name="observer">The subscriber.</param>
        public Subscription(CallbackPropertyObservable<TSource, TValue> parent, IObserver<TValue> observer)
        {
            _parent = parent;
            _observer = observer;
            _attachment = parent._subscribe(parent._source, Publish);
            try
            {
                Publish();
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
            if (Interlocked.Exchange(ref _observer, null) is not null)
            {
                _attachment.Dispose();
            }
        }

        /// <summary>Reads the property and delivers it, unless disposed or unchanged under distinct delivery.</summary>
        private void Publish()
        {
            var observer = Volatile.Read(ref _observer);
            if (observer is null)
            {
                return;
            }

            var value = _parent._getter(_parent._source);
            if (_parent._distinct && _hasValue && EqualityComparer<TValue>.Default.Equals(_lastValue, value))
            {
                return;
            }

            _lastValue = value;
            _hasValue = true;
            observer.OnNext(value);
        }
    }
}
