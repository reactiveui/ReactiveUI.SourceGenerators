// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows what <see cref="IPropertyBindingHook.ExecuteHook"/> receives and returns when a hook is asked about a binding directly.</summary>
public static class PropertyBindingHookExamples
{
    /// <summary>Asks a hook whether a binding between an accounts screen and its view may be created.</summary>
    /// <param name="hook">The hook to ask.</param>
    /// <param name="accounts">The view model the binding reads.</param>
    /// <param name="view">The view the binding writes.</param>
    /// <param name="direction">Whether the binding writes one way or both ways.</param>
    /// <returns><see langword="true"/> when the hook lets the binding be created; <see langword="false"/> when it refuses.</returns>
    public static bool AskHook(IPropertyBindingHook hook, AccountsViewModel accounts, AccountNameView view, BindingDirection direction) =>
        hook.ExecuteHook(accounts, view, ReadValues(accounts), ReadValues(view), direction);

    /// <summary>Asks a logging hook and a read-only hook about the same two objects: the logging hook always agrees, and the read-only hook refuses a two-way binding.</summary>
    public static void AskHookDirectly()
    {
        AccountsViewModel accounts = new(new InMemoryBankingBackend());
        AccountNameView view = new();

        Console.WriteLine(AskHook(new LoggingBindingHook("audit"), accounts, view, BindingDirection.OneWay));
        Console.WriteLine(AskHook(new ReadOnlyAccountHook(), accounts, view, BindingDirection.OneWay));
        Console.WriteLine(AskHook(new ReadOnlyAccountHook(), accounts, view, BindingDirection.TwoWay));

        // Output:
        // audit: OneWay: AccountsViewModel -> AccountNameView
        // True
        // True
        // read-only: refused a two-way binding
        // False
    }

    /// <summary>Creates the reader of the current values that a hook receives for one side of a binding.</summary>
    /// <param name="root">The object the reader returns as one observed change.</param>
    /// <returns>A function that reads the object as one observed change.</returns>
    private static Func<IObservedChange<object, object>[]> ReadValues(object root) =>
        () => [new ObservedChange<object, object>(root, null, root)];
}
