// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Observables;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Combines two to sixteen sources with <see cref="CombineLatestObservable"/>. It delivers a result once every
/// source has produced a value, and again each time any source produces a new one.
/// </summary>
public static class CombineLatestExamples
{
    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The title the registration task is renamed to.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The prefix of a finished checklist line.</summary>
    private const string DoneMark = "[x] ";

    /// <summary>The prefix of an open checklist line.</summary>
    private const string OpenMark = "[ ] ";

    /// <summary>The value every completed field of a form contributes.</summary>
    private const int CompletedField = 1;

    /// <summary>Combines the title and the done flag of a to-do item into a checklist line.</summary>
    public static void CombineTwoSources()
    {
        TodoItem item = new() { Title = RegistrationTitle };

        using (CombineLatestObservable
            .Create(Title(item), IsDone(item), static (title, isDone) => (isDone ? DoneMark : OpenMark) + title)
            .Subscribe(Console.WriteLine))
        {
            item.IsDone = true;
            item.Title = RenamedTitle;
        }

        // Output:
        // [ ] Renew car registration
        // [x] Renew car registration
        // [x] Renew car registration online
    }

    /// <summary>Combines the title, the done flag and the priority of a to-do item into one summary.</summary>
    public static void CombineThreeSources()
    {
        TodoItem item = new() { Title = RegistrationTitle };

        using (CombineLatestObservable
            .Create(Title(item), IsDone(item), Priority(item), static (title, isDone, priority) => $"{title} {isDone} {priority}")
            .Subscribe(Console.WriteLine))
        {
            item.Priority = TodoPriority.High;
        }

        // Output:
        // Renew car registration False Normal
        // Renew car registration False High
    }

    /// <summary>Counts the completed fields of a form with four fields.</summary>
    public static void CombineFourFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4) => f1 + f2 + f3 + f4).Subscribe(Console.WriteLine);

        // Output:
        // 4
    }

    /// <summary>Counts the completed fields of a form with five fields.</summary>
    public static void CombineFiveFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5) => f1 + f2 + f3 + f4 + f5).Subscribe(Console.WriteLine);

        // Output:
        // 5
    }

    /// <summary>Counts the completed fields of a form with six fields.</summary>
    public static void CombineSixFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6) => f1 + f2 + f3 + f4 + f5 + f6).Subscribe(Console.WriteLine);

        // Output:
        // 6
    }

    /// <summary>Counts the completed fields of a form with seven fields.</summary>
    public static void CombineSevenFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7) => f1 + f2 + f3 + f4 + f5 + f6 + f7).Subscribe(Console.WriteLine);

        // Output:
        // 7
    }

    /// <summary>Counts the completed fields of a form with eight fields.</summary>
    public static void CombineEightFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8).Subscribe(Console.WriteLine);

        // Output:
        // 8
    }

    /// <summary>Counts the completed fields of a form with nine fields.</summary>
    public static void CombineNineFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8, f9) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9).Subscribe(Console.WriteLine);

        // Output:
        // 9
    }

    /// <summary>Counts the completed fields of a form with ten fields.</summary>
    public static void CombineTenFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10).Subscribe(Console.WriteLine);

        // Output:
        // 10
    }

    /// <summary>Counts the completed fields of a form with eleven fields.</summary>
    public static void CombineElevenFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11).Subscribe(Console.WriteLine);

        // Output:
        // 11
    }

    /// <summary>Counts the completed fields of a form with twelve fields.</summary>
    public static void CombineTwelveFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12) => f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12).Subscribe(Console.WriteLine);

        // Output:
        // 12
    }

    /// <summary>Counts the completed fields of a form with thirteen fields.</summary>
    public static void CombineThirteenFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13) =>
                f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13).Subscribe(Console.WriteLine);

        // Output:
        // 13
    }

    /// <summary>Counts the completed fields of a form with fourteen fields.</summary>
    public static void CombineFourteenFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14) =>
                f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14).Subscribe(Console.WriteLine);

        // Output:
        // 14
    }

    /// <summary>Counts the completed fields of a form with fifteen fields.</summary>
    public static void CombineFifteenFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15) =>
                f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14 + f15).Subscribe(Console.WriteLine);

        // Output:
        // 15
    }

    /// <summary>Counts the completed fields of a form with sixteen fields.</summary>
    public static void CombineSixteenFields()
    {
        using IDisposable counts = CombineLatestObservable.Create(
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            Field(),
            static (f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12, f13, f14, f15, f16) =>
                f1 + f2 + f3 + f4 + f5 + f6 + f7 + f8 + f9 + f10 + f11 + f12 + f13 + f14 + f15 + f16).Subscribe(Console.WriteLine);

        // Output:
        // 16
    }

    /// <summary>Creates a form field that is complete and never changes.</summary>
    /// <returns>The field.</returns>
    private static UnchangingPropertyObservable<int> Field() => new(CompletedField);

    /// <summary>Observes the title of a to-do item.</summary>
    /// <param name="item">The item to observe.</param>
    /// <returns>The title now and after each change.</returns>
    private static PropertyObservable<string> Title(TodoItem item) => new(item, nameof(TodoItem.Title), static source => ((TodoItem)source).Title, true);

    /// <summary>Observes the done flag of a to-do item.</summary>
    /// <param name="item">The item to observe.</param>
    /// <returns>The flag now and after each change.</returns>
    private static PropertyObservable<bool> IsDone(TodoItem item) => new(item, nameof(TodoItem.IsDone), static source => ((TodoItem)source).IsDone, true);

    /// <summary>Observes the priority of a to-do item.</summary>
    /// <param name="item">The item to observe.</param>
    /// <returns>The priority now and after each change.</returns>
    private static PropertyObservable<TodoPriority> Priority(TodoItem item) => new(item, nameof(TodoItem.Priority), static source => ((TodoItem)source).Priority, true);
}
