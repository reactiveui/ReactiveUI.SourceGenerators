// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>Shows a partial property declared with <see cref="ObservableAsPropertyAttribute"/>.</summary>
public static class ObservableAsPropertyAttributeExamples
{
    /// <summary>Constructs <see cref="SummaryViewModel"/> and reads both of its generated properties.</summary>
    public static void DeclareAPropertyWithTheAttribute()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration" };
        SummaryViewModel summary = new SummaryViewModel(item);

        Console.WriteLine(summary.Title);
        Console.WriteLine(summary.IsDone);

        item.IsDone = true;

        Console.WriteLine(summary.IsDone);
    }

    /// <summary>Reads a property before and after its helper is assigned: until then it returns its initial value.</summary>
    public static void StartFromAnInitialValue()
    {
        StatusViewModel waiting = new StatusViewModel(null);
        StatusViewModel ready = new StatusViewModel(Signal.Return("Ready"));

        Console.WriteLine(waiting.Status);
        Console.WriteLine(ready.Status);

        // Output:
        // Loading
        // Ready
    }
}
