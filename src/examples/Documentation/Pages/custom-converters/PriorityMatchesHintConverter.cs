// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Converts a priority to <see langword="true"/> when it equals the priority passed as the conversion hint.</summary>
[System.Diagnostics.DebuggerDisplay("PriorityMatchesHintConverter: TodoPriority -> bool (equals the hint)")]
public sealed class PriorityMatchesHintConverter : BindingTypeConverter<TodoPriority, bool>
{
    /// <summary>The affinity of the converter for its type pair.</summary>
    private const int MatchAffinity = 10;

    /// <inheritdoc/>
    public override int GetAffinityForObjects() => MatchAffinity;

    /// <inheritdoc/>
    public override bool TryConvert(TodoPriority from, object? conversionHint, out bool result)
    {
        result = conversionHint is TodoPriority expected && from == expected;
        return true;
    }
}
