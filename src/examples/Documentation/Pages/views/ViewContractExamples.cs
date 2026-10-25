// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;
using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>Shows how <c>[ViewContract]</c> gives one view model more than one screen, and how the locator picks between them.</summary>
public static class ViewContractExamples
{
    /// <summary>The balance of the account the examples show.</summary>
    private const decimal AccountBalance = 2450.75M;

    /// <summary>A contract that no banking screen is registered under.</summary>
    private const string UnclaimedContract = "print-preview";

    /// <summary>Reads the contract a screen is registered under from its <see cref="ViewContractAttribute"/>.</summary>
    public static void ReadViewContract()
    {
        ViewContractAttribute? attribute = typeof(AccountStatementView).GetCustomAttribute<ViewContractAttribute>();

        Console.WriteLine(attribute?.Contract);

        // Output:
        // statement
    }

    /// <summary>Resolves the account screens by contract: the contract picks the statement, and no contract picks the summary.</summary>
    public static void ResolveViewByContract()
    {
        Account account = CreateAccount();
        DefaultViewLocator locator = new();

        IViewFor? statement = locator.ResolveView(account, AccountViewContracts.Statement);
        IViewFor? summary = locator.ResolveView(account, null);

        Console.WriteLine(statement?.GetType().Name);
        Console.WriteLine(summary?.GetType().Name);

        // Output:
        // AccountStatementView
        // AccountSummaryView
    }

    /// <summary>Resolves a banking screen with and without a contract: the contract picks the compact screen.</summary>
    public static void ResolveBankingViewByContract()
    {
        AccountsViewModel viewModel = new(new InMemoryBankingBackend());
        DefaultViewLocator locator = new();

        IViewFor? standard = locator.ResolveView(viewModel, null);
        IViewFor? compact = locator.ResolveView(viewModel, AccountViewContracts.Compact);

        Console.WriteLine(standard?.GetType().Name);
        Console.WriteLine(compact?.GetType().Name);
        Console.WriteLine(ReferenceEquals(compact?.ViewModel, viewModel));

        // Output:
        // AccountsView
        // CompactAccountsView
        // True
    }

    /// <summary>Resolves with a contract that no screen claims: the screen that has no contract does not answer it, so no view resolves.</summary>
    public static void ResolveUnclaimedContract()
    {
        Account account = CreateAccount();
        DefaultViewLocator locator = new();

        IViewFor? view = locator.ResolveView(account, UnclaimedContract);

        Console.WriteLine(view is null);

        // Output:
        // True
    }

    /// <summary>Creates the account the examples show.</summary>
    /// <returns>A new account.</returns>
    private static Account CreateAccount() => new() { Id = "ACC-1001", Name = "Everyday Account", Kind = AccountKind.Everyday, Currency = "AUD", Balance = AccountBalance };
}
