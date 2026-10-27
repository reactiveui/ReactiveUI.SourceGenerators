// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.SourceGenerators;

/// <summary>
/// Shows bindings over the members ReactiveUI.SourceGenerators writes. That generator's output is not visible to this
/// one, so the binding generator reads its attributes and follows its rules for what it writes.
/// </summary>
public static class SourceGeneratorsExamples
{
    /// <summary>Observes the property <c>[Reactive]</c> writes from a field.</summary>
    public static void ObserveAPropertyWrittenFromAField()
    {
        ProfileViewModel profile = new ProfileViewModel();

        using IDisposable subscription = profile.WhenAnyValue(x => x.DisplayName).Subscribe(Console.WriteLine);

        profile.DisplayName = "Grace";

        // Output:
        // Ada
        // Grace
    }

    /// <summary>Observes a class marked <c>[IReactiveObject]</c>, which raises its notifications through the generated interface.</summary>
    public static void ObserveAClassMarkedIReactiveObject()
    {
        ProfileCard card = new ProfileCard();

        using IDisposable subscription = card.WhenAnyValue(x => x.Title).Subscribe(Console.WriteLine);

        card.Title = "Lead engineer";

        // Output:
        // Engineer
        // Lead engineer
    }

    /// <summary>Binds a screen's name box to the property <c>[Reactive]</c> writes, in both directions.</summary>
    public static void BindAPropertyWrittenFromAField()
    {
        ProfileViewModel profile = new ProfileViewModel();
        ProfileView view = new ProfileView { ViewModel = profile };

        using IDisposable binding = view.Bind(profile, x => x.DisplayName, v => v.NameText);

        Console.WriteLine(view.NameText);

        view.NameText = "Linus";

        Console.WriteLine(profile.DisplayName);

        // Output:
        // Ada
        // Linus
    }

    /// <summary>
    /// Observes a property the way code another source generator writes does: with <see cref="ObservedProperty"/>,
    /// which needs no generated binding. Hand-written code calls <c>WhenAnyValue</c> instead.
    /// </summary>
    public static void ObserveFromGeneratedCode()
    {
        ProfileViewModel profile = new ProfileViewModel();

        using IDisposable subscription = ObservedProperty
            .Create(profile, static x => x.DisplayName, static x => x.DisplayName)
            .Subscribe(Console.WriteLine);

        profile.DisplayName = "Grace";

        // Output:
        // Ada
        // Grace
    }

    /// <summary>Binds a button to the command <c>[ReactiveCommand]</c> writes from a method.</summary>
    public static void BindAButtonToAGeneratedCommand()
    {
        ProfileViewModel profile = new ProfileViewModel();
        ProfileView view = new ProfileView { ViewModel = profile };

        using IDisposable binding = view.BindCommand(profile, x => x.SaveCommand, v => v.Save);

        view.Save.Press();

        Console.WriteLine(profile.Saves);

        // Output:
        // 1
    }
}
