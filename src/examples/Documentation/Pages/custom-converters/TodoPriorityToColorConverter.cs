// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Custom converter from TodoPriority to color name string.</summary>
public sealed class TodoPriorityToColorConverter : BindingTypeConverter<TodoPriority, string>
{
    /// <summary>The affinity score to prioritize this converter over built-in ones.</summary>
    private const int ConvertorAffinity = 10;

    /// <summary>Gets the affinity score for this converter.</summary>
    /// <returns>A value higher than default converters to prioritize this one.</returns>
    public override int GetAffinityForObjects() => ConvertorAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(TodoPriority from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        result = from switch
        {
            TodoPriority.Low => "Gray",
            TodoPriority.Normal => "Orange",
            TodoPriority.High => "Red",
            _ => null,
        };

        return result is not null;
    }
}
