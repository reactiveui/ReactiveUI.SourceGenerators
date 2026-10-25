// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;
using Splat;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Shows how affinity decides which mechanism observes a property: the highest score wins, and a provider
/// registered at run time outranks the generated mechanism only with a strictly higher score.
/// </summary>
public static class AffinityExamples
{
    /// <summary>The size given to a stored object while it is observed.</summary>
    private const long ResizedBytes = 1_024;

    /// <summary>The notes written to an item while they are observed.</summary>
    private const string EditedNotes = "Bring the insurance certificate and the old plate.";

    /// <summary>The title written to an item while it is observed.</summary>
    private const string EditedTitle = "Renew car registration online";

    /// <summary>The name of the property that carries the notes.</summary>
    private const string NotesPropertyName = nameof(TodoItem.Notes);

    /// <summary>The name of the property that carries the title.</summary>
    private const string TitlePropertyName = nameof(TodoItem.Title);

    /// <summary>Registers two providers that bid for properties of <see cref="TodoItem"/> next to the core providers.</summary>
    /// <param name="tied">The provider whose bid equals the generated mechanism's affinity.</param>
    /// <param name="outranking">The provider whose bid is one above the generated mechanism's affinity.</param>
    public static void RegisterProviders(TodoPropertyObservableForProperty tied, TodoPropertyObservableForProperty outranking)
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder
            .WithCoreServices()
            .WithRegistration(resolver => resolver.RegisterConstant<ICreatesObservableForProperty>(tied))
            .WithRegistration(resolver => resolver.RegisterConstant<ICreatesObservableForProperty>(outranking))
            .BuildApp();

        ObservationAffinityChecker.Refresh();
    }

    /// <summary>Observes a property of a class that raises <c>PropertyChanged</c>; the generated mechanism delivers each change.</summary>
    /// <param name="item">The item to observe.</param>
    public static void ObserveNotifyingClass(TodoItem item)
    {
        using (item.WhenChanged(x => x.IsDone).Subscribe(Console.WriteLine))
        {
            item.IsDone = true;
        }

        // Output:
        // False
        // True
    }

    /// <summary>Observes a property of a plain class; only the value at subscription arrives, and later changes do not.</summary>
    /// <param name="stored">The stored object to observe.</param>
    public static void ObservePlainClass(StorageObject stored)
    {
        using (stored.WhenChanged(x => x.Size).Subscribe(Console.WriteLine))
        {
            stored.Size = ResizedBytes;
        }

        // Output:
        // 2411724
    }

    /// <summary>Scores three properties with the built-in providers and names the provider that wins each.</summary>
    public static void PickWinningMechanism()
    {
        Console.WriteLine($"PropertyChanged provider, to-do item: {new INPCObservableForProperty().GetAffinityForObject(typeof(TodoItem), nameof(TodoItem.IsDone))}");
        Console.WriteLine($"PropertyChanged provider, stored object: {new INPCObservableForProperty().GetAffinityForObject(typeof(StorageObject), nameof(StorageObject.Size))}");
        Console.WriteLine($"Fallback provider, stored object: {new POCOObservableForProperty().GetAffinityForObject(typeof(StorageObject), nameof(StorageObject.Size))}");

        Console.WriteLine($"IsDone is observed by {SelectHighestAffinity(typeof(TodoItem), nameof(TodoItem.IsDone))?.GetType().Name}");
        Console.WriteLine($"Size is observed by {SelectHighestAffinity(typeof(StorageObject), nameof(StorageObject.Size))?.GetType().Name}");
        Console.WriteLine($"Title is observed by {SelectHighestAffinity(typeof(TodoItem), TitlePropertyName)?.GetType().Name}");

        // Output:
        // PropertyChanged provider, to-do item: 5
        // PropertyChanged provider, stored object: 0
        // Fallback provider, stored object: 1
        // IsDone is observed by INPCObservableForProperty
        // Size is observed by POCOObservableForProperty
        // Title is observed by TodoPropertyObservableForProperty
    }

    /// <summary>
    /// Compares every affinity score with a provider that bids <see cref="BindingAffinity.Explicit"/>: the
    /// provider outranks the scores below its bid, and loses to its own score and every score above it.
    /// </summary>
    public static void CompareEveryAffinityScore()
    {
        (string Name, int Score)[] scores =
        [
            (nameof(BindingAffinity.Fallback), BindingAffinity.Fallback),
            (nameof(BindingAffinity.DefaultInternalTypeConverter), BindingAffinity.DefaultInternalTypeConverter),
            (nameof(BindingAffinity.DefaultEvent), BindingAffinity.DefaultEvent),
            (nameof(BindingAffinity.WpfDependencyObject), BindingAffinity.WpfDependencyObject),
            (nameof(BindingAffinity.EventEnabledControl), BindingAffinity.EventEnabledControl),
            (nameof(BindingAffinity.Explicit), BindingAffinity.Explicit),
            (nameof(BindingAffinity.WinUiDependencyObject), BindingAffinity.WinUiDependencyObject),
            (nameof(BindingAffinity.WinFormsEvent), BindingAffinity.WinFormsEvent),
            (nameof(BindingAffinity.ExactType), BindingAffinity.ExactType),
            (nameof(BindingAffinity.Kvo), BindingAffinity.Kvo),
        ];

        foreach (var (name, score) in scores)
        {
            bool outranked = ObservationAffinityChecker.HasHigherAffinityPlugin(typeof(TodoItem), NotesPropertyName, score, false);

            Console.WriteLine($"{name} ({score}): provider outranks it = {outranked}");
        }

        // Output:
        // Fallback (1): provider outranks it = True
        // DefaultInternalTypeConverter (2): provider outranks it = True
        // DefaultEvent (3): provider outranks it = True
        // WpfDependencyObject (4): provider outranks it = True
        // EventEnabledControl (4): provider outranks it = True
        // Explicit (5): provider outranks it = False
        // WinUiDependencyObject (6): provider outranks it = False
        // WinFormsEvent (8): provider outranks it = False
        // ExactType (10): provider outranks it = False
        // Kvo (15): provider outranks it = False
    }

    /// <summary>Observes two properties of the same item; the provider that ties the generated mechanism is never used, and the one that outranks it is.</summary>
    /// <param name="item">The item to observe.</param>
    public static void ObserveWithTiedAndOutrankingProviders(TodoItem item)
    {
        using (item.WhenChanged(x => x.Notes).Subscribe(Console.WriteLine))
        using (item.WhenChanged(x => x.Title).Subscribe(Console.WriteLine))
        {
            item.Notes = EditedNotes;
            item.Title = EditedTitle;
        }

        // Output:
        // Bring the old plate as well.
        // Title is observed by the provider that bids 6
        // Renew car registration by post
        // Bring the insurance certificate and the old plate.
        // Renew car registration online
    }

    /// <summary>Finds the registered provider with the highest affinity for a property, keeping the first on a tie.</summary>
    /// <param name="type">The type being observed.</param>
    /// <param name="propertyName">The property being observed.</param>
    /// <returns>The winning provider, or <see langword="null"/> when none supports the property.</returns>
    private static ICreatesObservableForProperty? SelectHighestAffinity(Type type, string propertyName)
    {
        ICreatesObservableForProperty? winner = null;
        int highest = 0;

        foreach (ICreatesObservableForProperty provider in AppLocator.Current.GetServices<ICreatesObservableForProperty>())
        {
            int affinity = provider.GetAffinityForObject(type, propertyName);
            if (affinity <= highest)
            {
                continue;
            }

            highest = affinity;
            winner = provider;
        }

        return winner;
    }
}
