// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>
/// Demonstrates a master and detail screen: a busy indicator, filter boxes, a filtered list, a selection that
/// drives a detail box, and a button that follows the selection.
/// </summary>
public static class ListSelectionBindingExamples
{
    /// <summary>The size of the file the busy indicator example sends: four 4 MiB parts.</summary>
    private const long UploadSizeBytes = 16_777_216;

    /// <summary>A filter that matches the items whose title contains the word "book".</summary>
    private const string BookFilter = "book";

    /// <summary>A filter that matches the items whose title contains the word "car".</summary>
    private const string CarFilter = "car";

    /// <summary>The title the second item is renamed to.</summary>
    private const string RenamedTitle = "Book dentist for a check-up";

    /// <summary>The text shown while nothing is selected.</summary>
    private const string NothingSelectedText = "nothing selected";

    /// <summary>The position of the dentist item in the list.</summary>
    private const int DentistIndex = 1;

    /// <summary>Shows a progress bar and disables the upload button while an upload is in flight.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindBusyIndicator()
    {
        StorageBrowserViewModel browser = await OpenBucketAsync();
        StorageBrowserView view = new();

        using (browser.BindOneWay(view, x => x.IsUploading, v => v.UploadProgressBar.IsVisible))
        using (browser.BindOneWay(view, x => x.IsUploading, v => v.UploadButton.IsEnabled, static uploading => !uploading))
        {
            Console.WriteLine(view.UploadProgressBar.IsVisible);
            Console.WriteLine(view.UploadButton.IsEnabled);

            // The upload flags itself as running before its first wait, so the task is still pending here.
            Task upload = browser.UploadAsync(new("launch-video.mp4", UploadSizeBytes, "video/mp4"));

            Console.WriteLine(view.UploadProgressBar.IsVisible);
            Console.WriteLine(view.UploadButton.IsEnabled);

            await upload;

            Console.WriteLine(view.UploadProgressBar.IsVisible);
            Console.WriteLine(view.UploadButton.IsEnabled);
        }

        // Output:
        // False
        // True
        // True
        // False
        // False
        // True
    }

    /// <summary>Binds the new-item box and the filter box to the view model in both directions.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindFilterTextBoxesTwoWay()
    {
        TodoListViewModel viewModel = await OpenTodoListAsync();
        viewModel.FilterText = BookFilter;
        TodoView view = new() { ViewModel = viewModel };

        using (view.Bind(viewModel, x => x.FilterText, v => v.FilterTextBox.Text))
        using (view.Bind(viewModel, x => x.NewTitle, v => v.NewTitleTextBox.Text))
        {
            // The view model's starting filter reaches the box.
            Console.WriteLine(view.FilterTextBox.Text);

            // Typing in a box reaches the view model.
            view.NewTitleTextBox.Text = "Book electrician";

            Console.WriteLine(viewModel.NewTitle);

            // Clearing the filter in code reaches the box.
            viewModel.FilterText = string.Empty;

            Console.WriteLine($"'{view.FilterTextBox.Text}'");
        }

        // Output:
        // book
        // Book electrician
        // ''
    }

    /// <summary>Binds the filtered items into the list control, so the list narrows as the filter changes.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindFilteredItemsToList()
    {
        TodoListViewModel viewModel = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = viewModel };

        using (view.OneWayBind(viewModel, x => x.Items, v => v.ItemsList.ItemsSource))
        {
            Console.WriteLine(string.Join(", ", TitlesOf(view)));

            viewModel.FilterText = BookFilter;

            Console.WriteLine(string.Join(", ", TitlesOf(view)));

            viewModel.FilterText = CarFilter;

            Console.WriteLine(string.Join(", ", TitlesOf(view)));
        }

        // Output:
        // Renew car registration, Book dentist appointment, Buy birthday present for Sam, File quarterly tax return
        // Book dentist appointment
        // Renew car registration
    }

    /// <summary>Binds the list's selected item to the view model in both directions, and shows the selection in a detail box.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindSelectedItemTwoWay()
    {
        TodoListViewModel viewModel = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = viewModel };
        view.ItemsList.ItemsSource = viewModel.Items;

        // The list holds its selection as an object, so each direction converts.
        using (viewModel.BindTwoWay(view, x => x.SelectedItem, v => v.ItemsList.SelectedItem, static item => item, static selected => (selected as TodoItem)!))
        using (viewModel.BindOneWay(view, x => x.SelectedItem!.Title, v => v.SelectedTitleTextBox.Text))
        {
            Console.WriteLine(view.SelectedTitleTextBox.Text ?? NothingSelectedText);

            // The user clicks the second row.
            TodoItem dentist = viewModel.Items[DentistIndex];
            view.ItemsList.SelectedItem = dentist;

            Console.WriteLine(ReferenceEquals(viewModel.SelectedItem, dentist));
            Console.WriteLine(view.SelectedTitleTextBox.Text);

            // Code selects the first item, and the list follows.
            TodoItem renew = viewModel.Items[0];
            viewModel.SelectedItem = renew;

            Console.WriteLine(ReferenceEquals(view.ItemsList.SelectedItem, renew));
            Console.WriteLine(view.SelectedTitleTextBox.Text);

            // The box follows a change to the selected item itself.
            renew.Title = RenamedTitle;

            Console.WriteLine(view.SelectedTitleTextBox.Text);

            // Clearing the selection in the list clears the detail.
            view.ItemsList.SelectedItem = null;

            Console.WriteLine(view.SelectedTitleTextBox.Text ?? NothingSelectedText);
        }

        // Output:
        // nothing selected
        // True
        // Book dentist appointment
        // True
        // Renew car registration
        // Book dentist for a check-up
        // nothing selected
    }

    /// <summary>Enables the complete button only while an unfinished item is selected, using <c>BindCommand</c>.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindCommandFollowsSelection()
    {
        TodoListViewModel viewModel = await OpenTodoListAsync();
        TodoView view = new() { ViewModel = viewModel };

        using (view.BindCommand(viewModel, x => x.CompleteCommand, v => v.CompleteButton))
        {
            Console.WriteLine(view.CompleteButton.IsEnabled);

            TodoItem renew = viewModel.Items[0];
            viewModel.SelectedItem = renew;

            Console.WriteLine(view.CompleteButton.IsEnabled);

            // Pressing the button runs the command, which finishes the item and disables the button again.
            Task<bool> completed = renew.WhenChanged(x => x.IsDone).Where(static done => done).FirstAsync();
            ((IButtonController)view.CompleteButton).SendClicked();
            await completed;

            Console.WriteLine(renew.IsDone);
            Console.WriteLine(view.CompleteButton.IsEnabled);

            // Selecting a finished item leaves the button disabled, because the command cannot run on it.
            viewModel.SelectedItem = viewModel.Items[^1];

            Console.WriteLine(view.CompleteButton.IsEnabled);

            viewModel.SelectedItem = null;

            Console.WriteLine(view.CompleteButton.IsEnabled);
        }

        // Output:
        // False
        // True
        // True
        // False
        // False
        // False
    }

    /// <summary>Lists the titles of the items a list control shows.</summary>
    /// <param name="view">The to-do screen.</param>
    /// <returns>The title of each item, in order.</returns>
    private static IEnumerable<string> TitlesOf(TodoView view) => view.ItemsList.ItemsSource.Cast<TodoItem>().Select(static item => item.Title);

    /// <summary>Loads the seeded to-do list.</summary>
    /// <returns>A task that completes with the list, which holds three unfinished items and one finished item.</returns>
    private static async Task<TodoListViewModel> OpenTodoListAsync()
    {
        TodoListViewModel list = new(InMemoryTodoStore.CreateSeeded());

        await list.LoadAsync();
        return list;
    }

    /// <summary>Lists the buckets of the storage service and picks the first.</summary>
    /// <returns>A task that completes with the browser, which has a bucket selected.</returns>
    private static async Task<StorageBrowserViewModel> OpenBucketAsync()
    {
        StorageBrowserViewModel browser = new(InMemoryObjectStorage.CreateSeeded());

        await browser.LoadBucketsAsync();
        browser.SelectedBucket = browser.Buckets[0];
        return browser;
    }
}
