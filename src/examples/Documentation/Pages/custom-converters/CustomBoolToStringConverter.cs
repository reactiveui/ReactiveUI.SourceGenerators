// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Custom bool-to-string converter that uses different labels than the default.</summary>
[DebuggerDisplay("CustomBoolToStringConverter: bool -> string (custom labels)")]
public sealed class CustomBoolToStringConverter : BindingTypeConverter<bool, string>
{
    /// <summary>The affinity score to prioritize this converter over built-in ones.</summary>
    private const int HighAffinity = 100;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => HighAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(bool from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        result = from ? "AFFIRMATIVE" : "NEGATIVE";
        return true;
    }
}
