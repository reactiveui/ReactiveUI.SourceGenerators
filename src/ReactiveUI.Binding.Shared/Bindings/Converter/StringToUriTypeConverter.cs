// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>
/// Converts a <see cref="string"/> to a <see cref="Uri"/> with <see cref="Uri.TryCreate(string?, UriKind, out Uri?)"/>,
/// accepting relative and absolute URIs; a null string or text that cannot form a URI fails.
/// </summary>
public sealed class StringToUriTypeConverter : BindingTypeConverter<string, Uri>
{
    /// <summary>The affinity returned by <see cref="GetAffinityForObjects"/> indicating a strong match.</summary>
    private static readonly int Affinity = BindingAffinity.DefaultInternalTypeConverter;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => Affinity;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [NotNullWhen(true)] out Uri? result)
    {
        if (from is null)
        {
            result = null;
            return false;
        }

        return Uri.TryCreate(from, UriKind.RelativeOrAbsolute, out result);
    }
}
