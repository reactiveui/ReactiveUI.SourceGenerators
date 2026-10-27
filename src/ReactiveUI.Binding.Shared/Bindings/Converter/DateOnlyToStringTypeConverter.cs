// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if NET8_0_OR_GREATER
#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts a <see cref="DateOnly"/> to a <see cref="string"/> in the short date format of the current culture.</summary>
public sealed class DateOnlyToStringTypeConverter : BindingTypeConverter<DateOnly, string>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(DateOnly from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        result = from.ToString();
        return true;
    }
}
#endif
