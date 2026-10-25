// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using Splat;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>
/// Shows what the generated view lookup does with each view: <c>[SingleInstanceView]</c>, <c>[ExcludeFromViewRegistration]</c>,
/// a view that cannot be built, and a view registered for a view model interface.
/// </summary>
public static class GeneratedViewDispatchExamples
{
    /// <summary>The heading the receipt screen shows.</summary>
    private const string ReceiptHeading = "Transfer receipt";

    /// <summary>The number of the everyday account.</summary>
    private const string EverydayId = "ACC-1001";

    /// <summary>The name of the everyday account.</summary>
    private const string EverydayName = "Everyday Account";

    /// <summary>The number of the savings account.</summary>
    private const string SavingsId = "ACC-2002";

    /// <summary>The name of the savings account.</summary>
    private const string SavingsName = "Savings Account";

    /// <summary>The balance of every account the examples show.</summary>
    private const decimal AccountBalance = 2450.75M;

    /// <summary>The amount of the transfer the receipt is for.</summary>
    private const decimal TransferAmount = 250M;

    /// <summary>The contract the hand-written lookup answers to.</summary>
    private const string PreviewCardContract = "preview-card";

    /// <summary>Resolves two accounts: a view marked as a single instance is built once and shared, and each resolve gives it the latest view model.</summary>
    public static void ResolveSingleInstanceView()
    {
        Account everyday = CreateAccount(EverydayId, EverydayName);
        Account savings = CreateAccount(SavingsId, SavingsName);
        DefaultViewLocator locator = new();

        IViewFor? first = locator.ResolveView(everyday);
        IViewFor? second = locator.ResolveView(savings);

        Console.WriteLine(first?.GetType().Name);
        Console.WriteLine(ReferenceEquals(first, second));
        Console.WriteLine(ReferenceEquals(first?.ViewModel, savings));

        // Output:
        // AccountSummaryView
        // True
        // True
    }

    /// <summary>Resolves a view that leaves the generated lookup out: nothing answers until the application maps it.</summary>
    public static void ResolveViewLeftOutOfRegistration()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration" };
        DefaultViewLocator locator = new();

        IViewFor? before = locator.ResolveView(item);
        locator.Map<TodoItem, TodoItemPreviewView>();
        IViewFor? after = locator.ResolveView(item);

        Console.WriteLine(before is null);
        Console.WriteLine(after?.GetType().Name);

        // Output:
        // True
        // TodoItemPreviewView
    }

    /// <summary>Resolves a view with no parameterless constructor: the generated lookup finds it only when the service locator holds it.</summary>
    public static void ResolveViewWithoutParameterlessConstructor()
    {
        TransferReceipt receipt = new("RCPT-000001", TransferAmount, AccountBalance - TransferAmount, DateTimeOffset.UnixEpoch);
        DefaultViewLocator locator = new();

        IViewFor? before = locator.ResolveView(receipt);
        AppLocator.CurrentMutable.Register<IViewFor<TransferReceipt>>(static () => new ReceiptView(ReceiptHeading));

        try
        {
            ReceiptView? after = (ReceiptView?)locator.ResolveView(receipt);

            Console.WriteLine(before is null);
            Console.WriteLine(after?.HeadingLabel.Text);
            Console.WriteLine(ReferenceEquals(after?.ViewModel, receipt));
        }
        finally
        {
            AppLocator.CurrentMutable.UnregisterAll<IViewFor<TransferReceipt>>();
        }

        // Output:
        // True
        // Transfer receipt
        // True
    }

    /// <summary>Resolves two view models that implement one interface: a view registered for the interface serves every implementation.</summary>
    public static void ResolveViewRegisteredForInterface()
    {
        AccountListViewModel current = new([CreateAccount(EverydayId, EverydayName)]);
        ClosedAccountListViewModel closed = new([]);
        DefaultViewLocator locator = new();

        IViewFor? currentView = locator.ResolveView(current);
        IViewFor? closedView = locator.ResolveView(closed);

        Console.WriteLine(currentView?.GetType().Name);
        Console.WriteLine(closedView?.GetType().Name);
        Console.WriteLine(ReferenceEquals(closedView?.ViewModel, closed));

        // Output:
        // AccountListView
        // AccountListView
        // True
    }

    /// <summary>
    /// Resolves a view model that has a view for its interface and a view for its own class. The generated lookup tests the
    /// registrations in source-file order and returns the first match, so the interface view comes first here and the view
    /// for the class is never returned.
    /// </summary>
    public static void ResolveInterfaceViewBeforeClassView()
    {
        AccountListViewModel current = new([CreateAccount(EverydayId, EverydayName)]);
        DefaultViewLocator locator = new();

        IViewFor? view = locator.ResolveView(current);

        Console.WriteLine(view?.GetType().Name);
        Console.WriteLine(view is DetailedAccountListView);

        // Output:
        // AccountListView
        // False
    }

    /// <summary>Registers a lookup of your own, in the form the generator registers for each assembly: it answers for the view models and contracts it knows and returns null for the rest.</summary>
    public static void RegisterHandWrittenDispatch()
    {
        TodoItem item = new TodoItem { Title = "Renew car registration" };
        DefaultViewLocator locator = new();

        IViewFor? before = locator.ResolveView(item, PreviewCardContract);

        DefaultViewLocator.SetGeneratedViewDispatch(static (viewModel, contract) =>
            viewModel is TodoItem && contract == PreviewCardContract ? new TodoItemPreviewView() : null);

        IViewFor? after = locator.ResolveView(item, PreviewCardContract);
        IViewFor? otherContract = locator.ResolveView(item, null);

        Console.WriteLine(before is null);
        Console.WriteLine(after?.GetType().Name);
        Console.WriteLine(ReferenceEquals(after?.ViewModel, item));
        Console.WriteLine(otherContract is null);

        // Output:
        // True
        // TodoItemPreviewView
        // True
        // True
    }

    /// <summary>Creates an account for the examples.</summary>
    /// <param name="id">The number of the account.</param>
    /// <param name="name">The name of the account.</param>
    /// <returns>A new account.</returns>
    private static Account CreateAccount(string id, string name) => new() { Id = id, Name = name, Kind = AccountKind.Everyday, Currency = "AUD", Balance = AccountBalance };
}
