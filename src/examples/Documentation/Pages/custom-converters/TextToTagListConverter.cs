// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Converts a line of comma separated text to the tags of a to-do item, dropping blank tags.</summary>
[System.Diagnostics.DebuggerDisplay("TextToTagListConverter: text -> tags")]
public sealed class TextToTagListConverter : BindingTypeConverter<string, IReadOnlyList<string>>
{
    /// <summary>The affinity of the converter for its type pair.</summary>
    private const int TagsAffinity = 10;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => TagsAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(string? from, object? conversionHint, [MaybeNullWhen(true)] out IReadOnlyList<string>? result)
    {
        result = from?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return result is not null;
    }
}
