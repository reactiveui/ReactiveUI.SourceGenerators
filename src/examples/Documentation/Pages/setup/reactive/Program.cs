// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Concurrency;
using System.Reactive.Linq;
using ReactiveUI.Binding.Documentation.Setup.Aot;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Setup.Reactive;

/// <summary>Edits a task through a generated two-way binding in a System.Reactive application.</summary>
internal static class Program
{
    /// <summary>Tracks the initial title, a user edit and disposal of the binding.</summary>
    /// <exception cref="InvalidOperationException">The row and task did not agree with the observed edits.</exception>
    internal static void Main()
    {
        const string Original = "Renew registration";
        const string Edited = "Renew registration online";
        TodoItem item = new() { Title = Original };
        TodoRowView row = new();
        List<string> titles = [];
        using IDisposable observation = item.WhenChanged(static task => task.Title)
            .ObserveOn(ImmediateScheduler.Instance)
            .Subscribe(titles.Add);
        using (item.BindTwoWay(row, static task => task.Title, static view => view.TitleText))
        {
            if (row.TitleText != Original)
            {
                throw new InvalidOperationException("The row should show the task title before the first edit.");
            }

            row.TitleText = Edited;
        }

        row.TitleText = "Detached row";
        if (item.Title != Edited || titles is not [Original, Edited])
        {
            throw new InvalidOperationException("Only the edit made while the binding was active should reach the task.");
        }
    }
}
