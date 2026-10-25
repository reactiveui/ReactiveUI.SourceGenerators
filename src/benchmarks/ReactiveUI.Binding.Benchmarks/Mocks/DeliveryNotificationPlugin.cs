// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Forwards cached notification tokens so plugin delivery measures typed reads without per-change boxing.</summary>
internal sealed class DeliveryNotificationPlugin : ICreatesObservableForProperty
{
    /// <summary>The score for the fixture's notification mechanism.</summary>
    private const int Affinity = 100;

    /// <inheritdoc/>
    public int GetAffinityForObject(Type type, string propertyName, bool beforeChanged) =>
        type == typeof(DeliveryViewModel) && propertyName == nameof(DeliveryViewModel.Value) && !beforeChanged ? Affinity : 0;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObservable<IObservedChange<object, object?>> GetNotificationForProperty(
        object sender,
        Expression expression,
        string propertyName,
        bool beforeChanged,
        bool suppressWarnings) => new Notifications((DeliveryViewModel)sender, expression);

    /// <summary>Owns one immutable property notification token.</summary>
    /// <param name="source">The property source.</param>
    /// <param name="expression">The property expression passed to the provider.</param>
    private sealed class Notifications(DeliveryViewModel source, Expression expression) : IObservable<IObservedChange<object, object?>>
    {
        /// <summary>The provider's signal-only token; the binding reads the typed value itself.</summary>
        private readonly ObservedChange<object, object?> _change = new(source, expression, null);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IDisposable Subscribe(IObserver<IObservedChange<object, object?>> observer) => new Subscription(source, _change, observer);
    }

    /// <summary>Attaches one event handler for one provider subscription.</summary>
    private sealed class Subscription : IDisposable
    {
        /// <summary>The notification source.</summary>
        private readonly DeliveryViewModel _source;

        /// <summary>The immutable signal token reused for each notification.</summary>
        private readonly IObservedChange<object, object?> _change;

        /// <summary>The subscriber receiving notifications.</summary>
        private readonly IObserver<IObservedChange<object, object?>> _observer;

        /// <summary>Initializes a new instance of the <see cref="Subscription"/> class.</summary>
        /// <param name="source">The event source.</param>
        /// <param name="change">The token reused for each event.</param>
        /// <param name="observer">The downstream notification consumer.</param>
        internal Subscription(DeliveryViewModel source, IObservedChange<object, object?> change, IObserver<IObservedChange<object, object?>> observer)
        {
            _source = source;
            _change = change;
            _observer = observer;
            _source.PropertyChanged += OnChanged;
        }

        /// <inheritdoc/>
        public void Dispose() => _source.PropertyChanged -= OnChanged;

        /// <summary>Forwards a notification without reading or boxing the property.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The cached event arguments.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnChanged(object? sender, PropertyChangedEventArgs args) => _observer.OnNext(_change);
    }
}
