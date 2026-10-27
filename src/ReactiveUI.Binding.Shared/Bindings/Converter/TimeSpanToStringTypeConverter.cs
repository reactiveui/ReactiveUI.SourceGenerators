// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Converts a <see cref="TimeSpan"/> to a <see cref="string"/> in the culture-invariant constant format (<c>c</c>).</summary>
public sealed class TimeSpanToStringTypeConverter : BindingTypeConverter<TimeSpan, string>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(TimeSpan from, object? conversionHint, [NotNullWhen(true)] out string? result)
    {
        result = from.ToString();
        return true;
    }
}
