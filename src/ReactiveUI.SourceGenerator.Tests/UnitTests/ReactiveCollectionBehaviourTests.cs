// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Loader;

namespace ReactiveUI.SourceGenerator.Tests;

/// <summary>Runs a generated <c>[ReactiveCollection]</c> property to check what its setter subscribes.</summary>
public class ReactiveCollectionBehaviourTests
{
    /// <summary>A view model with one generated collection property.</summary>
    private const string Source = """
        using System.Collections.ObjectModel;
        using ReactiveUI;
        using ReactiveUI.SourceGenerators;

        namespace Behaviour;

        public partial class ViewModel : ReactiveObject
        {
            [ReactiveCollection]
            private ObservableCollection<int>? _items;
        }
        """;

    /// <summary>Replacing the collection unsubscribes the one it replaced, so only the current collection notifies.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task ReplacingTheCollectionUnsubscribesTheOldOne()
    {
        var (viewModel, property, collectionType) = CreateViewModel();
        var first = (IList)Activator.CreateInstance(collectionType)!;
        var second = (IList)Activator.CreateInstance(collectionType)!;
        var notifications = 0;
        ((INotifyPropertyChanged)viewModel).PropertyChanged += (_, _) => notifications++;

        property.SetValue(viewModel, first);
        property.SetValue(viewModel, second);
        property.SetValue(viewModel, first);
        property.SetValue(viewModel, second);
        notifications = 0;

        _ = first.Add(1);
        await Assert.That(notifications).IsEqualTo(0);

        _ = second.Add(1);
        await Assert.That(notifications).IsEqualTo(1);
    }

    /// <summary>Clearing a collection property that holds nothing does not throw.</summary>
    /// <returns>A task to monitor the async.</returns>
    [Test]
    public async Task SettingNullWhenEmptyDoesNotThrow()
    {
        var (viewModel, property, _) = CreateViewModel();

        await Assert.That(() => property.SetValue(viewModel, null)).ThrowsNothing();
    }

    /// <summary>Generates, compiles and loads the view model.</summary>
    /// <returns>An instance, its generated collection property, and the collection type.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "SES1402", Justification = "The test loads the assembly it has just compiled from its own source.")]
    private static (object ViewModel, PropertyInfo Property, Type CollectionType) CreateViewModel()
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp13);
        var compilation = CSharpCompilation.Create(
            "ReactiveCollectionBehaviour",
            [CSharpSyntaxTree.ParseText(Source, parseOptions)],
            TestCompilationReferences.CreatePortableDefault(),
            new(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        _ = CSharpGeneratorDriver
            .Create([new ReactiveGenerator().AsSourceGenerator(), new ReactiveCollectionGenerator().AsSourceGenerator()], parseOptions: parseOptions)
            .RunGeneratorsAndUpdateCompilation(compilation, out var output, out _);

        using var image = new MemoryStream();
        var emit = output.Emit(image);
        if (!emit.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, emit.Diagnostics));
        }

        image.Position = 0;
        var assembly = new AssemblyLoadContext(nameof(ReactiveCollectionBehaviourTests), isCollectible: true).LoadFromStream(image);
        var type = assembly.GetType("Behaviour.ViewModel", throwOnError: true)!;
        var property = type.GetProperty("Items") ?? throw new InvalidOperationException("The Items property was not generated.");
        return (Activator.CreateInstance(type)!, property, property.PropertyType);
    }
}
