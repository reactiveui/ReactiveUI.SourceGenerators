// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using ReactiveUI.SourceGenerators.WinForms;

namespace ReactiveUI.SourceGenerators.Benchmarks.Support;

/// <summary>Names the generators the package ships and the mock corpus each one reads.</summary>
internal static class GeneratorCatalog
{
    /// <summary>Gets the corpus names, one per generator, each matching a file in the mocks folder.</summary>
    internal static IEnumerable<string> Corpora =>
    [
        "Reactive",
        "ReactiveCommand",
        "ReactiveObject",
        "BindableDerivedList",
        "ReactiveCollection",
        "ViewModelControlHost",
        "RoutedControlHost",
        GeneratorHarness.AllCorpus,
    ];

    /// <summary>Creates the generators a corpus exercises: its own generator, or every generator for the whole corpus.</summary>
    /// <param name="corpus">The corpus name.</param>
    /// <returns>The generators.</returns>
    /// <remarks>
    /// <see cref="ReactiveGenerator"/> always runs, because it injects the modifier enums the other attributes take.
    /// No other corpus marks a member <c>[Reactive]</c>, so it adds only its fixed cost.
    /// </remarks>
    internal static ISourceGenerator[] Create(string corpus) =>
        corpus switch
        {
            "Reactive" => [new ReactiveGenerator().AsSourceGenerator()],
            "ReactiveCommand" => [new ReactiveGenerator().AsSourceGenerator(), new ReactiveCommandGenerator().AsSourceGenerator()],
            "ReactiveObject" => [new ReactiveGenerator().AsSourceGenerator(), new ReactiveObjectGenerator().AsSourceGenerator()],
            "BindableDerivedList" => [new ReactiveGenerator().AsSourceGenerator(), new BindableDerivedListGenerator().AsSourceGenerator()],
            "ReactiveCollection" => [new ReactiveGenerator().AsSourceGenerator(), new ReactiveCollectionGenerator().AsSourceGenerator()],
            "ViewModelControlHost" => [new ReactiveGenerator().AsSourceGenerator(), new ViewModelControlHostGenerator().AsSourceGenerator()],
            "RoutedControlHost" => [new ReactiveGenerator().AsSourceGenerator(), new RoutedControlHostGenerator().AsSourceGenerator()],
            _ => Create(),
        };

    /// <summary>Creates every generator the package ships.</summary>
    /// <returns>The generators.</returns>
    internal static ISourceGenerator[] Create() =>
    [
        new ReactiveGenerator().AsSourceGenerator(),
        new ReactiveCommandGenerator().AsSourceGenerator(),
        new ReactiveObjectGenerator().AsSourceGenerator(),
        new BindableDerivedListGenerator().AsSourceGenerator(),
        new ReactiveCollectionGenerator().AsSourceGenerator(),
        new ViewModelControlHostGenerator().AsSourceGenerator(),
        new RoutedControlHostGenerator().AsSourceGenerator(),
    ];
}
