// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Views;

/// <summary>Shows how a screen declares the view model it displays: <c>IViewFor</c>, <c>IViewFor&lt;T&gt;</c> and <c>IActivatableView</c>.</summary>
public static class ViewForExamples
{
    /// <summary>The title the user types into the new item box.</summary>
    private const string NewItemTitle = "Book vet appointment";

    /// <summary>The text shown while the screen has no view model.</summary>
    private const string NoViewModelText = "no view model yet";

    /// <summary>Gives a screen its view model through the typed interface, as a navigation service does.</summary>
    /// <typeparam name="TViewModel">The view model type the screen shows.</typeparam>
    /// <param name="view">The screen.</param>
    /// <param name="viewModel">The view model to show.</param>
    /// <returns>The view model the screen now shows.</returns>
    public static TViewModel? Present<TViewModel>(IViewFor<TViewModel> view, TViewModel viewModel)
        where TViewModel : class
    {
        view.ViewModel = viewModel;
        return view.ViewModel;
    }

    /// <summary>Counts the objects a host may activate, whatever else they are.</summary>
    /// <param name="candidates">The objects the host holds.</param>
    /// <returns>The number of objects that implement <see cref="IActivatableView"/>.</returns>
    public static int CountActivatable(IEnumerable<object> candidates) => candidates.OfType<IActivatableView>().Count();

    /// <summary>Assigns a to-do list to the to-do screen through <see cref="IViewFor{T}"/>, where the view model is typed.</summary>
    public static void AssignTypedViewModel()
    {
        TodoView view = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());

        Console.WriteLine(view.ViewModel is null);

        TodoListViewModel? shown = Present(view, viewModel);

        Console.WriteLine(ReferenceEquals(shown, viewModel));
        Console.WriteLine(ReferenceEquals(view.ViewModel, viewModel));

        // Output:
        // True
        // True
        // True
    }

    /// <summary>Observes the view model of the screen, which a view reports like any other property.</summary>
    public static void ObserveViewModelOfView()
    {
        TodoView view = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        List<TodoListViewModel?> shown = [];

        using (view.WhenChanged(x => x.ViewModel!).Subscribe(shown.Add))
        {
            view.ViewModel = viewModel;
        }

        Console.WriteLine(shown.Count);
        Console.WriteLine(shown[0] is null);
        Console.WriteLine(ReferenceEquals(shown[1], viewModel));

        // Output:
        // 2
        // True
        // True
    }

    /// <summary>Assigns an issue board through the non-generic <see cref="IViewFor"/>, where the view model is an object.</summary>
    public static void AssignThroughNonGenericInterface()
    {
        IssueBoardView view = new();
        IssueBoardViewModel viewModel = new(InMemoryGitHubServer.CreateSeeded());
        IViewFor screen = (IViewFor)view;

        screen.ViewModel = viewModel;

        Console.WriteLine(ReferenceEquals(screen.ViewModel, viewModel));
        Console.WriteLine(ReferenceEquals(view.ViewModel, viewModel));

        // Output:
        // True
        // True
    }

    /// <summary>Shows that a screen refuses a view model of the wrong type when it is assigned through <see cref="IViewFor"/>.</summary>
    public static void RejectWrongViewModelType()
    {
        IViewFor screen = (IViewFor)new IssueBoardView();
        TodoListViewModel wrongViewModel = new(InMemoryTodoStore.CreateSeeded());
        bool refused = false;

        try
        {
            screen.ViewModel = wrongViewModel;
        }
        catch (InvalidCastException)
        {
            refused = true;
        }

        Console.WriteLine(refused);
        Console.WriteLine(screen.ViewModel is null);

        // Output:
        // True
        // True
    }

    /// <summary>Recognizes the screens a host may activate: a view is activatable, a view model is not.</summary>
    public static void RecognizeActivatableViews()
    {
        List<object> candidates =
        [
            new TodoView(),
            new TodoListViewModel(InMemoryTodoStore.CreateSeeded()),
            new AccountsView(),
        ];

        Console.WriteLine(CountActivatable(candidates));

        // Output:
        // 2
    }

    /// <summary>Binds the to-do screen to its view model; the bindings need a view that implements <see cref="IViewFor"/>.</summary>
    /// <returns>A task that completes when the items are loaded and shown.</returns>
    public static async Task BindTodoView()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        TodoView view = new();
        await viewModel.LoadAsync();

        // The bindings follow view.ViewModel, so the screen shows the view model they read.
        _ = Present(view, viewModel);

        using (view.OneWayBind(viewModel, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        using (view.Bind(viewModel, x => x.NewTitle, v => v.NewTitleTextBox.Text))
        {
            Console.WriteLine(view.RemainingLabel.Text);

            view.NewTitleTextBox.Text = NewItemTitle;

            Console.WriteLine(viewModel.NewTitle);
        }

        // Output:
        // 3
        // Book vet appointment
    }

    /// <summary>Binds before the screen has a view model; the binding waits for one and moves to a replacement.</summary>
    /// <returns>A task that completes when both lists are loaded and shown.</returns>
    public static async Task BindBeforeViewModelArrives()
    {
        TodoListViewModel household = new(InMemoryTodoStore.CreateSeeded());
        TodoListViewModel shared = new(InMemoryTodoStore.CreateSeeded());
        await household.LoadAsync();
        await shared.LoadAsync();
        shared.SelectedItem = shared.Items[0];
        await shared.CompleteAsync();
        TodoView view = new();

        // The binding follows view.ViewModel, which is still empty, so the label is left alone.
        using (view.OneWayBind(household, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            Console.WriteLine(view.RemainingLabel.Text ?? NoViewModelText);

            view.ViewModel = household;
            Console.WriteLine(view.RemainingLabel.Text);

            // Replacing the view model moves the binding to the new one.
            view.ViewModel = shared;
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // no view model yet
        // 3
        // 2
    }

    /// <summary>Reads the labels of a banking screen from its view model through <see cref="IViewFor{T}"/>.</summary>
    /// <returns>A task that completes when the accounts are loaded and shown.</returns>
    public static async Task BindAccountsView()
    {
        InMemoryBankingBackend backend = new();
        AccountsViewModel viewModel = new(backend);
        AccountsView view = new();
        _ = Present(view, viewModel);

        using (view.OneWayBind(viewModel, x => x.TotalBalance, v => v.TotalBalanceLabel.Text, static total => total.ToString("N2", CultureInfo.InvariantCulture)))
        {
            await viewModel.LoadAccountsAsync();

            Console.WriteLine(view.TotalBalanceLabel.Text);
        }

        // Output:
        // 17,680.75
    }
}
