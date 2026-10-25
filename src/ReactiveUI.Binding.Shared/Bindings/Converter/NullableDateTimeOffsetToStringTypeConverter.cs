// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Converts a nullable <see cref="DateTimeOffset"/> to a <see cref="string"/> in the general date and time format of the
/// current culture, including the offset. A null value succeeds with a null string.
/// </summary>
public sealed class NullableDateTimeOffsetToStringTypeConverter : BindingTypeConverter<DateTimeOffset?, string>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(
        DateTimeOffset? from,
        object? conversionHint,
        [MaybeNullWhen(true)] out string? result)
    {
        if (!from.HasValue)
        {
            result = null;
            return true;
        }

        result = from.Value.ToString();
        return true;
    }
}
