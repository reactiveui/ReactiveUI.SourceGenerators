// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Converts the tags of a to-do item to one line of text, with a comma between the tags.</summary>
[System.Diagnostics.DebuggerDisplay("TagListToTextConverter: tags -> text")]
public sealed class TagListToTextConverter : BindingTypeConverter<IReadOnlyList<string>, string>
{
    /// <summary>The affinity of the converter for its type pair.</summary>
    private const int TagsAffinity = 10;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => TagsAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(IReadOnlyList<string>? from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        result = from is null ? null : string.Join(", ", from);
        return result is not null;
    }
}
