// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Converts a nullable <see cref="long"/> to a <see cref="string"/> using the current culture. An <see cref="int"/> hint
/// gives the minimum digit count (the <c>D</c> format) and a <see cref="string"/> hint gives the format string; a malformed
/// format throws <see cref="FormatException"/>. A null value succeeds with a null string.
/// </summary>
public sealed class NullableLongToStringTypeConverter : BindingTypeConverter<long?, string>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(long? from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        if (!from.HasValue)
        {
            result = null;
            return true;
        }

        switch (conversionHint)
        {
            case int width:
                {
                    result = from.Value.ToString($"D{width}");
                    return true;
                }

            case string format:
                {
                    result = from.Value.ToString(format);
                    return true;
                }

            default:
                {
                    result = from.Value.ToString();
                    return true;
                }
        }
    }
}
