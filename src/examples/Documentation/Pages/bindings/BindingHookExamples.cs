// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using Splat;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows how an <see cref="IPropertyBindingHook"/> sees, and can refuse, each binding as it is created.</summary>
public static class BindingHookExamples
{
    /// <summary>A name the customer types for the everyday account.</summary>
    private const string RenamedEverydayName = "Household Account";

    /// <summary>Creates a one-way binding with no hook registered: <see cref="BindingHooks.Any"/> is false, so the binding asks nobody.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindWithoutHook()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountNameView view = new();

        Console.WriteLine(BindingHooks.Any);

        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            Console.WriteLine(view.NameTextBox.Text);
        }

        // Output:
        // False
        // Everyday Account
    }

    /// <summary>Writes down every binding that is created: each of the four binding methods asks the hook once, with its direction.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task LogEveryBinding()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountNameView view = new() { ViewModel = accounts };

        AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new LoggingBindingHook("log"));
        BindingHooks.Refresh();

        Console.WriteLine(BindingHooks.Any);

        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        using (accounts.BindTwoWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        using (view.OneWayBind(accounts, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        using (view.Bind(accounts, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            Console.WriteLine("all four bound");
        }

        RemoveHooks();

        // Output:
        // True
        // log: OneWay: AccountsViewModel -> AccountNameView
        // log: TwoWay: AccountsViewModel -> AccountNameView
        // log: OneWay: AccountsViewModel -> AccountNameView
        // log: TwoWay: AccountsViewModel -> AccountNameView
        // all four bound
    }

    /// <summary>Refuses two-way bindings on the accounts screen: the account is shown, and an edit in the view never reaches it.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task VetoTwoWayBinding()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountNameView view = new();
        string everydayName = accounts.SelectedAccount!.Name;

        AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new ReadOnlyAccountHook());
        BindingHooks.Refresh();

        using (accounts.BindTwoWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            // The refused binding is an empty disposable: nothing was written to the view either.
            Console.WriteLine($"'{view.NameTextBox.Text}'");

            view.NameTextBox.Text = RenamedEverydayName;
            Console.WriteLine(accounts.SelectedAccount.Name == everydayName);
        }

        // One-way bindings pass the hook.
        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            Console.WriteLine(view.NameTextBox.Text);
        }

        RemoveHooks();

        // With the hook gone the same two-way binding writes the edit to the account.
        using (accounts.BindTwoWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            view.NameTextBox.Text = RenamedEverydayName;
            Console.WriteLine(accounts.SelectedAccount.Name);
        }

        // Output:
        // read-only: refused a two-way binding
        // ''
        // True
        // Everyday Account
        // Household Account
    }

    /// <summary>A refused view-first binding returns no binding, so the caller receives <see langword="null"/>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task VetoViewFirstBinding()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountNameView view = new() { ViewModel = accounts };

        AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new ReadOnlyAccountHook());
        BindingHooks.Refresh();

        IReactiveBinding<AccountNameView, BindingChange> refused = view.Bind(accounts, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text);
        using IReactiveBinding<AccountNameView, string> allowed = view.OneWayBind(accounts, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text);

        Console.WriteLine(refused is null);
        Console.WriteLine(allowed is null);
        Console.WriteLine(view.NameTextBox.Text);

        RemoveHooks();

        // Output:
        // read-only: refused a two-way binding
        // True
        // False
        // Everyday Account
    }

    /// <summary>Asks the registered hooks directly. One refusal is enough, and the hooks after it are not asked.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task AskHooksDirectly()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountNameView view = new();

        AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new LoggingBindingHook("first"));
        AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new ReadOnlyAccountHook());
        AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new LoggingBindingHook("last"));
        BindingHooks.Refresh();

        bool oneWay = BindingHooks.ShouldBind(accounts, view, Observed(accounts), Observed(view), BindingDirection.OneWay);
        bool twoWay = BindingHooks.ShouldBind(accounts, view, Observed(accounts), Observed(view), BindingDirection.TwoWay);

        Console.WriteLine(oneWay);
        Console.WriteLine(twoWay);

        RemoveHooks();

        // Output:
        // first: OneWay: AccountsViewModel -> AccountNameView
        // last: OneWay: AccountsViewModel -> AccountNameView
        // first: TwoWay: AccountsViewModel -> AccountNameView
        // read-only: refused a two-way binding
        // True
        // False
    }

    /// <summary>Makes a hook registered after the first binding visible: <see cref="BindingHooks"/> reads the registrations once, until <see cref="BindingHooks.Refresh"/>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task RefreshAfterLateRegistration()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountNameView view = new();

        Console.WriteLine(BindingHooks.Any);

        AppLocator.CurrentMutable.RegisterConstant<IPropertyBindingHook>(new LoggingBindingHook("late"));

        // The hook is registered, but the registrations were read before it arrived, so this binding does not ask it.
        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            Console.WriteLine("bound without the hook");
        }

        BindingHooks.Refresh();

        Console.WriteLine(BindingHooks.Any);

        using (accounts.BindOneWay(view, x => x.SelectedAccount!.Name, v => v.NameTextBox.Text))
        {
            Console.WriteLine("bound with the hook");
        }

        RemoveHooks();

        Console.WriteLine(BindingHooks.Any);

        // Output:
        // False
        // bound without the hook
        // True
        // late: OneWay: AccountsViewModel -> AccountNameView
        // bound with the hook
        // False
    }

    /// <summary>Reads what a hook is handed: the objects being bound, and a reader for the current values on each side.</summary>
    /// <param name="root">The object whose values the reader returns.</param>
    /// <returns>A function that reads the object as one observed change.</returns>
    private static Func<IObservedChange<object, object>[]> Observed(object root) =>
        () => [new ObservedChange<object, object>(root, null, root)];

    /// <summary>Removes every registered hook and tells <see cref="BindingHooks"/> to read the registrations again.</summary>
    private static void RemoveHooks()
    {
        AppLocator.CurrentMutable.UnregisterAll<IPropertyBindingHook>();
        BindingHooks.Refresh();
    }

    /// <summary>Loads the accounts of the seeded bank and selects the everyday account.</summary>
    /// <returns>A task that completes with the accounts screen, which has the everyday account selected.</returns>
    private static async Task<AccountsViewModel> OpenAccountsAsync()
    {
        AccountsViewModel accounts = new(new InMemoryBankingBackend());

        await accounts.LoadAccountsAsync();
        accounts.SelectedAccount = accounts.Accounts[0];
        return accounts;
    }
}
