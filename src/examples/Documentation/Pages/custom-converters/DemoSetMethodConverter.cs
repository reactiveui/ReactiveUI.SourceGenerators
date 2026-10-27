// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>A demo set-method converter for specialized property-setting operations.</summary>
public sealed class DemoSetMethodConverter : ISetMethodBindingConverter
{
    /// <summary>The affinity score for set-method conversions.</summary>
    private const int SetMethodAffinity = 1;

    /// <inheritdoc/>
    public int GetAffinityForObjects(Type? fromType, Type? toType) => SetMethodAffinity;

    /// <inheritdoc/>
    public object? PerformSet(object? toTarget, object? newValue, object?[]? arguments) => newValue;
}
