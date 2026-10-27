// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>Demonstrates <c>ToPropertyUnsafe</c>, which finds the property's raise member by reflection.</summary>
public static class UnsafeToPropertyExamples
{
    /// <summary>The title of the first seeded item.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title an example gives an item.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>Backs a property on a type that is not partial and inherits only a protected raise method.</summary>
    public static void BackAPropertyOnATypeThatIsNotPartial()
    {
        BehaviorSignal<string> titles = new(OriginalTitle);
        TodoHeadlineViewModel viewModel = new TodoHeadlineViewModel(titles);
        List<string> raised = [];
        viewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName ?? string.Empty);

        titles.OnNext(RenamedTitle);

        Console.WriteLine(viewModel.Headline);
        Console.WriteLine(string.Join(", ", raised));

        // Output:
        // Renew car registration online
        // Headline
    }
}
