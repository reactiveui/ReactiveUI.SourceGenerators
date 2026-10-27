// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Mixins;
using Splat;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>Shows how views are registered by hand: <c>Map</c>, <c>Unmap</c>, <c>ViewMappingBuilder</c> and the order in which the locator looks.</summary>
public static class ViewMappingExamples
{
    /// <summary>The title of the to-do item the examples show.</summary>
    private const string ItemTitle = "Renew car registration";

    /// <summary>The balance of the account the examples show.</summary>
    private const decimal AccountBalance = 2450.75M;

    /// <summary>The contract of the preview layout.</summary>
    private const string PreviewContract = "preview";

    /// <summary>The contract of the detail layout.</summary>
    private const string DetailContract = "detail";

    /// <summary>The contract of the compact layout.</summary>
    private const string CompactContract = "compact";

    /// <summary>Registers a preview screen for a to-do item with its parameterless constructor.</summary>
    public static void MapView()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();

        locator.Map<TodoItem, TodoItemPreviewView>();

        IViewFor? view = locator.ResolveView(item);

        Console.WriteLine(view?.GetType().Name);
        Console.WriteLine(ReferenceEquals(view?.ViewModel, item));

        // Output:
        // TodoItemPreviewView
        // True
    }

    /// <summary>Registers a preview screen under a contract, so it answers only to that contract.</summary>
    public static void MapViewWithContract()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();

        locator.Map<TodoItem, TodoItemPreviewView>(PreviewContract);

        IViewFor? preview = locator.ResolveView(item, PreviewContract);
        IViewFor? plain = locator.ResolveView(item, null);

        Console.WriteLine(preview?.GetType().Name);
        Console.WriteLine(plain is null);

        // Output:
        // TodoItemPreviewView
        // True
    }

    /// <summary>Registers a factory, for a screen that needs setup before it is shown.</summary>
    public static void MapViewWithFactory()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();

        locator.Map<TodoItem>(static () => new TodoItemPreviewView { IsCompact = true });

        TodoItemPreviewView? view = (TodoItemPreviewView?)locator.ResolveView(item);

        Console.WriteLine(view?.IsCompact);
        Console.WriteLine(ReferenceEquals(view?.ViewModel, item));

        // Output:
        // True
        // True
    }

    /// <summary>Registers a factory under a contract.</summary>
    public static void MapViewWithFactoryAndContract()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();

        locator.Map<TodoItem>(static () => new TodoItemPreviewView { IsCompact = true }, CompactContract);

        TodoItemPreviewView? view = (TodoItemPreviewView?)locator.ResolveView(item, CompactContract);

        Console.WriteLine(view?.IsCompact);
        Console.WriteLine(locator.ResolveView(item, null) is null);

        // Output:
        // True
        // True
    }

    /// <summary>Removes a registration, with and without a contract; the result says whether one existed.</summary>
    public static void UnmapView()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();
        locator.Map<TodoItem, TodoItemPreviewView>();
        locator.Map<TodoItem, TodoItemPreviewView>(PreviewContract);

        bool removedDefault = locator.Unmap<TodoItem>();
        bool removedAgain = locator.Unmap<TodoItem>();
        bool removedContract = locator.Unmap<TodoItem>(PreviewContract);

        Console.WriteLine(removedDefault);
        Console.WriteLine(removedAgain);
        Console.WriteLine(removedContract);
        Console.WriteLine(locator.ResolveView(item) is null);

        // Output:
        // True
        // False
        // True
        // True
    }

    /// <summary>Registers views in a chain with the fluent builder of a locator.</summary>
    public static void MapViewsWithFluentBuilder()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();

        _ = locator.CreateMappingBuilder()
            .Map<TodoItem, TodoItemPreviewView>()
            .Map<TodoItem, TodoItemDetailView>(DetailContract)
            .Map<TodoItem>(static () => new TodoItemPreviewView { IsCompact = true }, CompactContract);

        IViewFor? standard = locator.ResolveView(item, null);
        IViewFor? detail = locator.ResolveView(item, DetailContract);
        TodoItemPreviewView? compact = (TodoItemPreviewView?)locator.ResolveView(item, CompactContract);

        Console.WriteLine(standard?.GetType().Name);
        Console.WriteLine(detail?.GetType().Name);
        Console.WriteLine(compact?.IsCompact);

        // Output:
        // TodoItemPreviewView
        // TodoItemDetailView
        // True
    }

    /// <summary>Registers a factory for the default contract in a chain.</summary>
    public static void MapFactoryWithFluentBuilder()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();

        _ = locator.CreateMappingBuilder().Map<TodoItem>(static () => new TodoItemPreviewView { IsCompact = true });

        TodoItemPreviewView? view = (TodoItemPreviewView?)locator.ResolveView(item);

        Console.WriteLine(view?.IsCompact);

        // Output:
        // True
    }

    /// <summary>Resolves a screen from the view model type alone, before there is a view model to show.</summary>
    public static void ResolveViewByType()
    {
        DefaultViewLocator locator = new();
        locator.Map<TodoItem, TodoItemPreviewView>();
        locator.Map<TodoItem, TodoItemDetailView>(DetailContract);

        IViewFor<TodoItem>? preview = locator.ResolveView<TodoItem>();
        IViewFor<TodoItem>? detail = locator.ResolveView<TodoItem>(DetailContract);

        Console.WriteLine(preview?.GetType().Name);
        Console.WriteLine(detail?.GetType().Name);
        Console.WriteLine(preview?.ViewModel is null);

        // Output:
        // TodoItemPreviewView
        // TodoItemDetailView
        // True
    }

    /// <summary>Maps a view model to a screen the service locator builds, so the container decides how it is created.</summary>
    public static void MapViewFromServiceLocator()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();
        AppLocator.CurrentMutable.Register(static () => new TodoItemDetailView());

        try
        {
            _ = locator.CreateMappingBuilder().MapFromServiceLocator<TodoItem, TodoItemDetailView>();

            IViewFor? view = locator.ResolveView(item);

            Console.WriteLine(view?.GetType().Name);
            Console.WriteLine(ReferenceEquals(view?.ViewModel, item));
        }
        finally
        {
            AppLocator.CurrentMutable.UnregisterAll<TodoItemDetailView>();
        }

        // Output:
        // TodoItemDetailView
        // True
    }

    /// <summary>
    /// Maps a contract to a screen the service locator registers under the same contract. Both screens are registered as
    /// <c>IViewFor&lt;TodoItem&gt;</c>, so the service contract is what tells them apart.
    /// </summary>
    public static void MapContractedViewFromServiceLocator()
    {
        TodoItem item = CreateItem();
        DefaultViewLocator locator = new();
        AppLocator.CurrentMutable.Register<IViewFor<TodoItem>>(static () => new TodoItemDetailView());
        AppLocator.CurrentMutable.Register<IViewFor<TodoItem>>(static () => new TodoItemPreviewView(), PreviewContract);

        try
        {
            _ = locator.CreateMappingBuilder()
                .MapFromServiceLocator<TodoItem, IViewFor<TodoItem>>()
                .MapFromServiceLocator<TodoItem, IViewFor<TodoItem>>(PreviewContract, serviceContract: PreviewContract);

            IViewFor? detail = locator.ResolveView(item);
            IViewFor? preview = locator.ResolveView(item, PreviewContract);

            Console.WriteLine(detail?.GetType().Name);
            Console.WriteLine(preview?.GetType().Name);
        }
        finally
        {
            AppLocator.CurrentMutable.UnregisterAll<IViewFor<TodoItem>>();
            AppLocator.CurrentMutable.UnregisterAll<IViewFor<TodoItem>>(PreviewContract);
        }

        // Output:
        // TodoItemDetailView
        // TodoItemPreviewView
    }

    /// <summary>Registers the mappings while the application starts, before the locator is first used.</summary>
    public static void ConfigureViewLocatorInBuilder()
    {
        TodoItem item = CreateItem();
        IReactiveUIBindingBuilder builder = (IReactiveUIBindingBuilder)RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder
            .WithCoreServices()
            .ConfigureViewLocator(static mappings => mappings
                .Map<TodoItem, TodoItemPreviewView>()
                .Map<TodoItem, TodoItemDetailView>(DetailContract))
            .BuildApp();

        IViewLocator locator = ViewLocator.GetCurrent();

        IViewFor? standard = locator.ResolveView(item);
        IViewFor? detail = locator.ResolveView(item, DetailContract);

        Console.WriteLine(standard?.GetType().Name);
        Console.WriteLine(detail?.GetType().Name);

        // Output:
        // TodoItemPreviewView
        // TodoItemDetailView
    }

    /// <summary>Registers the mappings in a chain that goes through the Splat builder, where <c>ReactiveUI.Binding.Mixins</c> supplies the call.</summary>
    public static void ConfigureViewLocatorFromAppBuilder()
    {
        TodoItem item = CreateItem();

        _ = RxBindingBuilder.CreateReactiveUIBindingBuilder()
            .WithCoreServices()
            .ConfigureViewLocator(static mappings => mappings.Map<TodoItem, TodoItemPreviewView>())
            .BuildApp();

        IViewFor? view = ViewLocator.GetCurrent().ResolveView(item);

        Console.WriteLine(view?.GetType().Name);

        // Output:
        // TodoItemPreviewView
    }

    /// <summary>Resolves with the locator's three sources registered: the generated lookup comes first, then <c>Map</c>, then the service locator.</summary>
    public static void ResolveInOrder()
    {
        TodoItem item = CreateItem();
        Account account = CreateAccount();
        DefaultViewLocator locator = new();
        locator.Map<Account, AccountStatementView>();
        locator.Map<TodoItem, TodoItemPreviewView>();
        AppLocator.CurrentMutable.Register<IViewFor<TodoItem>>(static () => new TodoItemDetailView());

        try
        {
            IViewFor? generated = locator.ResolveView(account);
            IViewFor? mapped = locator.ResolveView(item);
            _ = locator.Unmap<TodoItem>();
            IViewFor? registered = locator.ResolveView(item);

            Console.WriteLine(generated?.GetType().Name);
            Console.WriteLine(mapped?.GetType().Name);
            Console.WriteLine(registered?.GetType().Name);
        }
        finally
        {
            AppLocator.CurrentMutable.UnregisterAll<IViewFor<TodoItem>>();
        }

        // Output:
        // AccountSummaryView
        // TodoItemPreviewView
        // TodoItemDetailView
    }

    /// <summary>Creates the to-do item the examples show.</summary>
    /// <returns>A new item.</returns>
    private static TodoItem CreateItem() => new() { Title = ItemTitle };

    /// <summary>Creates the account the examples show.</summary>
    /// <returns>A new account.</returns>
    private static Account CreateAccount() => new() { Id = "ACC-1001", Name = "Everyday Account", Kind = AccountKind.Everyday, Currency = "AUD", Balance = AccountBalance };
}
