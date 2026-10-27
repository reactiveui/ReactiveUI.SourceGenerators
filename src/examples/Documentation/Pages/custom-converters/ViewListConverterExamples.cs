// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Turns a list of view models into a list of child views with a converter that asks the view locator.</summary>
public static class ViewListConverterExamples
{
    /// <summary>Shows every account of the customer as an <see cref="AccountSummaryView"/> row in the accounts list.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task ShowAccountsAsSummaryViews()
    {
        DefaultViewLocator locator = new();
        locator.Map<Account, AccountSummaryView>();
        AccountsView view = new();
        AccountsViewModel viewModel = new(new InMemoryBankingBackend());
        view.ViewModel = viewModel;

        using IReactiveBinding<AccountsView, System.Collections.IEnumerable> binding =
            view.OneWayBind(viewModel, x => x.Accounts, v => v.AccountList.ItemsSource, new AccountsToViewsConverter(locator));
        await viewModel.LoadAccountsAsync();

        foreach (AccountSummaryView row in view.AccountList.ItemsSource.OfType<AccountSummaryView>())
        {
            Console.WriteLine(row.ViewModel!.Name);
        }

        // Output:
        // Everyday Account
        // Savings Account
    }
}
