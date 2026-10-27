// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>The priority badge colour converter an older to-do app registered with Splat.</summary>
[DebuggerDisplay("LegacyPriorityToColorConverter: TodoPriority -> badge colour")]
public sealed class LegacyPriorityToColorConverter : BindingTypeConverter<TodoPriority, string>
{
    /// <summary>The badge colour of a low priority item.</summary>
    internal const string LowColour = "#9E9E9E";

    /// <summary>The badge colour of a normal priority item.</summary>
    internal const string NormalColour = "#FB8C00";

    /// <summary>The badge colour of a high priority item.</summary>
    internal const string HighColour = "#D32F2F";

    /// <summary>The affinity the older app gave the converter, above every built-in converter.</summary>
    private const int LegacyAffinity = 100;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => LegacyAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(TodoPriority from, object? conversionHint, [MaybeNullWhen(true)] out string? result)
    {
        result = from switch
        {
            TodoPriority.Low => LowColour,
            TodoPriority.Normal => NormalColour,
            TodoPriority.High => HighColour,
            _ => null,
        };

        return result is not null;
    }
}
