// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.Observables;

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Builds the public observation types with typed, cached accessors.</summary>
internal static class DeliveryObservation
{
    /// <summary>The provider whose events require no reflected property access.</summary>
    private static readonly DeliveryNotificationPlugin Plugin = new();

    /// <summary>The property expression shared across subscriptions.</summary>
    private static readonly Expression<Func<DeliveryViewModel, int>> ValueExpression = static source => source.Value;

    /// <summary>Creates the observation whose subscription owns the delivery gate.</summary>
    /// <param name="source">The source to read and observe.</param>
    /// <param name="kind">The notification contract to exercise.</param>
    /// <returns>An observable that emits its initial value when subscribed.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The observation kind is not supported.</exception>
    internal static IObservable<int> Create(DeliveryViewModel source, string kind) => kind switch
    {
        nameof(ObservationKind.Changed) => new PropertyObservable<int>(source, nameof(DeliveryViewModel.Value), static model => ((DeliveryViewModel)model).Value, false),
        nameof(ObservationKind.Changing) => new PropertyChangingObservable<int>(source, nameof(DeliveryViewModel.Value), static model => ((DeliveryViewModel)model).Value),
        nameof(ObservationKind.Plugin) => new PluginPropertyObservable<int>(
            Plugin,
            source,
            ValueExpression.Body,
            nameof(DeliveryViewModel.Value),
            static model => ((DeliveryViewModel)model).Value,
            false,
            false),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
