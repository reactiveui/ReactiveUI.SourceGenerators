// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Examples of <c>TwoWayConverterPair</c>, the two conversions a two-way binding needs.</summary>
public static class TwoWayConverterPairExamples
{
    /// <summary>Pairs the conversions between a to-do priority and its text, and takes the pair apart again.</summary>
    public static void DeconstructPriorityConverterPair()
    {
        Func<TodoPriority, string> toText = static priority => priority.ToString();
        Func<string, TodoPriority> toPriority = Enum.Parse<TodoPriority>;
        TwoWayConverterPair<TodoPriority, string> pair = new(toText, toPriority);

        var (forward, reverse) = pair;

        Console.WriteLine(forward(TodoPriority.High));
        Console.WriteLine(reverse("Low"));

        // Output:
        // High
        // Low
    }

    /// <summary>Compares priority converter pairs; a pair is equal to another when it holds the same two conversions.</summary>
    public static void ComparePriorityConverterPairs()
    {
        Func<TodoPriority, string> toText = static priority => priority.ToString();
        Func<string, TodoPriority> toPriority = Enum.Parse<TodoPriority>;
        TwoWayConverterPair<TodoPriority, string> pair = new(toText, toPriority);
        TwoWayConverterPair<TodoPriority, string> same = new(toText, toPriority);
        TwoWayConverterPair<TodoPriority, string> different = new(toText, static _ => TodoPriority.Low);
        object boxedSame = same;

        Console.WriteLine(pair == same);
        Console.WriteLine(pair != different);
        Console.WriteLine(pair.Equals(boxedSame));
        Console.WriteLine(pair.GetHashCode() == same.GetHashCode());
        Console.WriteLine(pair.ToString().StartsWith(nameof(TwoWayConverterPair<,>), StringComparison.Ordinal));

        // Output:
        // True
        // True
        // True
        // True
        // True
    }
}
