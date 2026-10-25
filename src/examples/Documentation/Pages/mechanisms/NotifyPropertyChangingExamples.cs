// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq.Expressions;
using ReactiveUI.Binding.ObservableForProperty;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>Shows how a class that implements <c>INotifyPropertyChanging</c> lets an observer read a property before an edit.</summary>
public static class NotifyPropertyChangingExamples
{
    /// <summary>The title of the registration task.</summary>
    private const string RegistrationTitle = "Renew car registration";

    /// <summary>The title the registration task is renamed to.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The notes written to the draft while its title is observed.</summary>
    private const string RegistrationNotes = "Bring the insurance certificate";

    /// <summary>The name of the observed title property.</summary>
    private const string TitlePropertyName = nameof(DraftTodo.Title);

    /// <summary>Reads the title of a draft each time an edit is about to replace it.</summary>
    public static void ObserveTitleBeforeItChanges()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };

        using (draft.WhenChanging(x => x.Title).Subscribe(Console.WriteLine))
        {
            draft.Title = RenamedTitle;
            draft.Notes = RegistrationNotes;
        }

        // Output:
        // Renew car registration
        // Renew car registration
    }

    /// <summary>Asks the <c>PropertyChanged</c> provider for before-change notifications from a draft.</summary>
    public static void AskProviderAboutBeforeChangeNotifications()
    {
        DraftTodo draft = new() { Title = RegistrationTitle };
        INPCObservableForProperty provider = new();
        Expression<Func<DraftTodo, string>> property = x => x.Title;

        Console.WriteLine($"Affinity before a change: {provider.GetAffinityForObject(typeof(DraftTodo), TitlePropertyName, true)}");

        List<string> titlesBefore = [];
        using (provider.GetNotificationForProperty(draft, property.Body, TitlePropertyName, true).Subscribe(change => titlesBefore.Add(((DraftTodo)change.Sender).Title)))
        {
            draft.Title = RenamedTitle;
        }

        Console.WriteLine($"Titles read before the change: {string.Join(", ", titlesBefore)}");

        // Output:
        // Affinity before a change: 5
        // Titles read before the change: Renew car registration
    }
}
