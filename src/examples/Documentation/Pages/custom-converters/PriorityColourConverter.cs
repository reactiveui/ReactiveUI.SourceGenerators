// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Converts a to-do priority to a colour name. It implements <see cref="IBindingTypeConverter"/> directly.</summary>
[System.Diagnostics.DebuggerDisplay("PriorityColourConverter: TodoPriority -> colour")]
public sealed class PriorityColourConverter : IBindingTypeConverter
{
    /// <summary>The affinity of the converter for its type pair.</summary>
    private const int ColourAffinity = 20;

    /// <inheritdoc/>
    public Type FromType => typeof(TodoPriority);

    /// <inheritdoc/>
    public Type ToType => typeof(string);

    /// <inheritdoc/>
    public int GetAffinityForObjects() => ColourAffinity;

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, [NotNullWhen(true)] out object? result)
    {
        result = from switch
        {
            TodoPriority.Low => "Green",
            TodoPriority.Normal => "Amber",
            TodoPriority.High => "Crimson",
            _ => null,
        };

        return result is not null;
    }
}
