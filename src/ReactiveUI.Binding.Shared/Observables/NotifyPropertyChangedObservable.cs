// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Observables;
#else
namespace ReactiveUI.Binding.Observables;
#endif

/// <summary>Emits an observed change each time the sender raises a notification for one property.</summary>
/// <remarks>
/// It observes <see cref="INotifyPropertyChanging"/> when before-change is requested and the sender implements it,
/// and <see cref="INotifyPropertyChanged"/> otherwise. A notification with a null or empty property name matches
/// every property. Nothing is emitted on subscribe, and the emitted change carries no value: the sender and
/// expression identify the property to read. The sequence never completes.
/// </remarks>
[DebuggerDisplay("NotifyPropertyChangedObservable: Property = {_observedPropertyName}, Sender = {_sender}, BeforeChanged = {_beforeChanged}")]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class NotifyPropertyChangedObservable : IObservable<IObservedChange<object, object?>>
{
    /// <summary>The source object raising the notifications.</summary>
    private readonly object _sender;

    /// <summary>The expression surfaced on the emitted observed change.</summary>
    private readonly Expression _expression;

    /// <summary>The property name to match, ending in <c>[]</c> for an indexer.</summary>
    private readonly string _observedPropertyName;

    /// <summary>Whether to observe before-change (<see cref="INotifyPropertyChanging"/>) notifications.</summary>
    private readonly bool _beforeChanged;

    /// <summary>Initializes a new instance of the <see cref="NotifyPropertyChangedObservable"/> class.</summary>
    /// <param name="sender">The source object raising the notifications.</param>
    /// <param name="expression">The expression surfaced on the emitted observed change.</param>
    /// <param name="expectedName">The property name to match, ending in <c>[]</c> for an indexer.</param>
    /// <param name="beforeChanged">Whether to observe before-change notifications.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sender"/>, <paramref name="expression"/> or <paramref name="expectedName"/> is <see langword="null"/>.</exception>
    public NotifyPropertyChangedObservable(object sender, Expression expression, string expectedName, bool beforeChanged)
    {
        ArgumentExceptionHelper.ThrowIfNull(sender);
        ArgumentExceptionHelper.ThrowIfNull(expression);
        ArgumentExceptionHelper.ThrowIfNull(expectedName);
        _sender = sender;
        _expression = expression;
        _observedPropertyName = expectedName;
        _beforeChanged = beforeChanged;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <see langword="null"/>.</exception>
    public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer)
    {
        ArgumentExceptionHelper.ThrowIfNull(observer);

        return new Subscription(_sender, _expression, _observedPropertyName, _beforeChanged, observer);
    }

    /// <summary>
    /// Manages the event subscription for a single observer: wires the appropriate change event,
    /// filters notifications by property name, and forwards a single prebuilt observed change.
    /// </summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>The before-change source, or <see langword="null"/> when observing after-change.</summary>
        private readonly INotifyPropertyChanging? _changing;

        /// <summary>The after-change source, or <see langword="null"/> when observing before-change.</summary>
        private readonly INotifyPropertyChanged? _changed;

        /// <summary>The observed property name (an empty notified name means "all properties").</summary>
        private readonly string _observedPropertyName;

        /// <summary>
        /// The change forwarded on every matching notification. It is constant for the subscription
        /// (fixed sender and expression, lazily-read value), so it is built once and reused.
        /// </summary>
        private readonly IObservedChange<object, object?> _change;

        /// <summary>The downstream observer. Set to <see langword="null"/> on disposal.</summary>
        private IObserver<IObservedChange<object, object?>>? _observer;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class and wires the change event.</summary>
        /// <param name="sender">The source object raising the notifications.</param>
        /// <param name="expression">The expression surfaced on the emitted observed change.</param>
        /// <param name="expectedName">The observed property name.</param>
        /// <param name="beforeChanged">Whether to observe before-change notifications.</param>
        /// <param name="observer">The downstream observer.</param>
        public Subscription(
            object sender,
            Expression expression,
            string expectedName,
            bool beforeChanged,
            IObserver<IObservedChange<object, object?>> observer)
        {
            _observedPropertyName = expectedName;
            _observer = observer;
            _change = new ObservedChange<object, object?>(sender, expression, default);

            if (beforeChanged && sender is INotifyPropertyChanging changing)
            {
                _changing = changing;
                changing.PropertyChanging += OnPropertyChanging;
            }
            else if (sender is INotifyPropertyChanged changed)
            {
                _changed = changed;
                changed.PropertyChanged += OnPropertyChanged;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _observer, null) is null)
            {
                return;
            }

            if (_changing is not null)
            {
                _changing.PropertyChanging -= OnPropertyChanging;
            }

            if (_changed is null)
            {
                return;
            }

            _changed.PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>
        /// Determines whether a notified property name matches the observed property
        /// (an empty notified name means "all properties").
        /// </summary>
        /// <param name="notifiedName">The property name carried by the notification.</param>
        /// <returns><see langword="true"/> if the notification applies to the observed property.</returns>
        private bool Matches(string? notifiedName) =>
            string.IsNullOrEmpty(notifiedName)
            || string.Equals(notifiedName, _observedPropertyName, StringComparison.InvariantCulture);

        /// <summary>Handles <see cref="INotifyPropertyChanged.PropertyChanged"/>, forwarding the change when the name matches.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The property-changed event arguments.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) => Forward(e.PropertyName);

        /// <summary>Handles <see cref="INotifyPropertyChanging.PropertyChanging"/>, forwarding the change when the name matches.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The property-changing event arguments.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnPropertyChanging(object? sender, PropertyChangingEventArgs e) => Forward(e.PropertyName);

        /// <summary>Forwards the change to the observer when the notified name matches the observed property.</summary>
        /// <param name="notifiedName">The property name carried by the notification.</param>
        private void Forward(string? notifiedName)
        {
            if (!Matches(notifiedName))
            {
                return;
            }

            Volatile.Read(ref _observer)?.OnNext(_change);
        }
    }
}
