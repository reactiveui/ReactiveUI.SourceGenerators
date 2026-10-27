// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>Emits a property's value on subscribe, unless skipped, and again on each notification.</summary>
/// <typeparam name="TSender">The type of the observed object surfaced on the emitted change.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
/// <remarks>
/// The value is read through the supplied delegate on every notification, and the notification's own value is
/// ignored. A read that throws is forwarded to the observer as an error. Errors and completion of the
/// notification source pass through to the observer.
/// </remarks>
[DebuggerDisplay("ObservableForPropertySink: {_expression}, Sender = {_sender}, SkipInitial = {_skipInitial}, Distinct = {_isDistinct}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class ObservableForPropertySink<TSender, TValue> : IObservable<IObservedChange<TSender, TValue>>
{
    /// <summary>The observed object surfaced on the emitted change.</summary>
    private readonly TSender _sender;

    /// <summary>The expression surfaced on the emitted change.</summary>
    private readonly Expression _expression;

    /// <summary>The underlying change-notification source (a notification re-reads the value).</summary>
    private readonly IObservable<IObservedChange<object, object?>> _notifications;

    /// <summary>Reads the current property value from the sender.</summary>
    private readonly Func<TValue> _readValue;

    /// <summary>When <see langword="true"/>, the current value is not emitted on subscribe.</summary>
    private readonly bool _skipInitial;

    /// <summary>When <see langword="true"/>, consecutive equal values are suppressed.</summary>
    private readonly bool _isDistinct;

    /// <summary>Initializes a new instance of the <see cref="ObservableForPropertySink{TSender, TValue}"/> class.</summary>
    /// <param name="sender">The observed object surfaced on the emitted change.</param>
    /// <param name="expression">The expression surfaced on the emitted change.</param>
    /// <param name="notifications">The underlying change-notification source.</param>
    /// <param name="readValue">Reads the current property value from the sender.</param>
    /// <param name="skipInitial">When <see langword="true"/>, the current value is not emitted on subscribe.</param>
    /// <param name="isDistinct">When <see langword="true"/>, consecutive equal values are suppressed.</param>
    /// <exception cref="ArgumentNullException"><paramref name="expression"/>, <paramref name="notifications"/> or <paramref name="readValue"/> is <see langword="null"/>.</exception>
    public ObservableForPropertySink(
        TSender sender,
        Expression expression,
        IObservable<IObservedChange<object, object?>> notifications,
        Func<TValue> readValue,
        bool skipInitial,
        bool isDistinct)
    {
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(notifications);
        ArgumentExceptionHelper.ThrowIfNull(readValue);
        _sender = sender;
        _expression = expression;
        _notifications = notifications;
        _readValue = readValue;
        _skipInitial = skipInitial;
        _isDistinct = isDistinct;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<IObservedChange<TSender, TValue>> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        var sink = new Sink(observer, _sender, _expression, _readValue, _isDistinct);
        return sink.Run(_notifications, _skipInitial);
    }

    /// <summary>Reads the property value on each notification and forwards it as an observed change.</summary>
    /// <param name="downstream">The observer receiving observed changes.</param>
    /// <param name="sender">The observed object.</param>
    /// <param name="expression">The expression surfaced on the observed change.</param>
    /// <param name="readValue">Reads the current property value.</param>
    /// <param name="isDistinct">Whether consecutive equal values are suppressed.</param>
    private sealed class Sink(
            IObserver<IObservedChange<TSender, TValue>> downstream,
            TSender sender,
            Expression expression,
            Func<TValue> readValue,
            bool isDistinct) : IObserver<IObservedChange<object, object?>>
    {
        /// <summary>The observer receiving observed changes.</summary>
        private readonly IObserver<IObservedChange<TSender, TValue>> _downstream = downstream;

        /// <summary>The observed object.</summary>
        private readonly TSender _sender = sender;

        /// <summary>The expression surfaced on the observed change.</summary>
        private readonly Expression _expression = expression;

        /// <summary>Reads the current property value from the sender.</summary>
        private readonly Func<TValue> _readValue = readValue;

        /// <summary>Whether consecutive equal values are suppressed.</summary>
        private readonly bool _isDistinct = isDistinct;

        /// <summary>The last emitted value, used by the distinct gate.</summary>
        private TValue _last = default!;

        /// <summary>Whether <see cref="_last"/> holds a value yet.</summary>
        private bool _hasLast;

        /// <summary>Optionally emits the current value, then subscribes to the notification source.</summary>
        /// <param name="notifications">The change-notification source.</param>
        /// <param name="skipInitial">When <see langword="true"/>, the current value is not emitted on subscribe.</param>
        /// <returns>The notification-source subscription.</returns>
        public IDisposable Run(IObservable<IObservedChange<object, object?>> notifications, bool skipInitial)
        {
            if (!skipInitial)
            {
                Emit();
            }

            return notifications.Subscribe(this);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNext(IObservedChange<object, object?> value) => Emit();

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnError(Exception error) => _downstream.OnError(error);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnCompleted() => _downstream.OnCompleted();

        /// <summary>Reads the current property value and forwards it as an observed change, honoring the distinct gate.</summary>
        private void Emit()
        {
            TValue current;
            try
            {
                current = _readValue();
            }
            catch (Exception ex)
            {
                _downstream.OnError(ex);
                return;
            }

            if (_isDistinct && _hasLast && EqualityComparer<TValue>.Default.Equals(current, _last))
            {
                return;
            }

            _last = current;
            _hasLast = true;
            _downstream.OnNext(new ObservedChange<TSender, TValue>(_sender, _expression, current));
        }
    }
}
