// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>Turns the tags of a to-do item into one line of text for a label.</summary>
public sealed class TagListToStringConverter : BindingTypeConverter<IReadOnlyList<string>, string>
{
    /// <summary>The text placed between two tags.</summary>
    private const string Separator = ", ";

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => 1;

    /// <inheritdoc/>
    public override bool TryConvert(IReadOnlyList<string>? from, object? conversionHint, out string? result)
    {
        result = string.Join(Separator, from ?? []);
        return true;
    }
}
