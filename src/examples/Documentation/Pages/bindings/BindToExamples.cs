// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows how <c>BindTo</c> writes each value of an observable onto a property.</summary>
public static class BindToExamples
{
    /// <summary>The size of the file the upload example sends: four 4 MiB parts.</summary>
    private const long UploadSizeBytes = 16_777_216;

    /// <summary>The number that turns an upload percentage into the fraction a progress bar shows.</summary>
    private const double PercentPerBar = 100D;

    /// <summary>The number of decimal places a hint asks for.</summary>
    private const int TwoDecimalPlaces = 2;

    /// <summary>The number format that groups digits and shows no decimals.</summary>
    private const string GroupedFormat = "N0";

    /// <summary>The text the user types into the filter box.</summary>
    private const string DentistFilter = "dentist";

    /// <summary>Fills a progress bar from the upload percentage; each value is converted to the fraction the bar shows.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindUploadPercentToProgressBar()
    {
        StorageBrowserViewModel browser = await OpenBucketAsync();
        StorageBrowserView view = new();

        using (browser.WhenChanged(x => x.UploadPercent).Select(static percent => percent / PercentPerBar).BindTo(view, v => v.UploadProgressBar.Progress))
        {
            Console.WriteLine(view.UploadProgressBar.Progress);

            await browser.UploadAsync(new("launch-video.mp4", UploadSizeBytes, "video/mp4"));
            Console.WriteLine(view.UploadProgressBar.Progress);
        }

        // Output:
        // 0
        // 1
    }

    /// <summary>Shows a count as text: the value type differs from the property type, so the registered converter for the pair runs. Disposing stops the writes.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindRemainingCountToLabel()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new();

        using (list.WhenChanged(x => x.RemainingCount).BindTo(view, v => v.RemainingLabel.Text))
        {
            Console.WriteLine(view.RemainingLabel.Text);

            list.SelectedItem = list.Items[0];
            await list.CompleteAsync();
            Console.WriteLine(view.RemainingLabel.Text);
        }

        list.SelectedItem = list.Items[1];
        await list.CompleteAsync();
        Console.WriteLine(view.RemainingLabel.Text);

        // Output:
        // 3
        // 2
        // 2
    }

    /// <summary>Passes a conversion hint: the converter reads a whole number as the count of decimal places.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindTotalBalanceWithDecimalPlacesHint()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();

        using (accounts.WhenChanged(x => x.TotalBalance).BindTo(view, v => v.TotalBalanceLabel.Text, TwoDecimalPlaces))
        {
            Console.WriteLine(view.TotalBalanceLabel.Text);
        }

        // Output:
        // 17680.75
    }

    /// <summary>Passes a conversion hint that is a number format. The argument is named because a text in that position would bind to the caller's expression parameter.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindTotalBalanceWithFormatHint()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();

        using (accounts.WhenChanged(x => x.TotalBalance).BindTo(view, v => v.TotalBalanceLabel.Text, conversionHint: GroupedFormat))
        {
            Console.WriteLine(view.TotalBalanceLabel.Text);
        }

        // Output:
        // 17,681
    }

    /// <summary>Names the converter for this binding, which outranks every converter the library registers.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindTotalBalanceWithConverterOverride()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();

        using (accounts.WhenChanged(x => x.TotalBalance).BindTo(view, v => v.TotalBalanceLabel.Text, new CurrencyTextConverter()))
        {
            Console.WriteLine(view.TotalBalanceLabel.Text);
        }

        // Output:
        // $17,680.75
    }

    /// <summary>Names the converter and gives it a hint.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindTotalBalanceWithHintAndConverterOverride()
    {
        AccountsViewModel accounts = await OpenAccountsAsync();
        AccountsView view = new();

        using (accounts.WhenChanged(x => x.TotalBalance).BindTo(view, v => v.TotalBalanceLabel.Text, GroupedFormat, new CurrencyTextConverter()))
        {
            Console.WriteLine(view.TotalBalanceLabel.Text);
        }

        // Output:
        // $17,681
    }

    /// <summary>Feeds a view event into a view model property: each text the user types into the filter box narrows the list.</summary>
    /// <returns>A task that completes when the example finishes.</returns>
    public static async Task BindTypedTextEventsToFilter()
    {
        TodoListViewModel list = await OpenTodoListAsync();
        TodoView view = new();
        IObservable<string> typedTexts = Signal.FromEventPattern<TextChangedEventArgs>(handler => view.FilterTextBox.TextChanged += handler, handler => view.FilterTextBox.TextChanged -= handler)
            .Select(static pattern => pattern.EventArgs.NewTextValue);

        using (typedTexts.BindTo(list, x => x.FilterText))
        {
            Console.WriteLine(list.Items.Count);

            view.FilterTextBox.Text = DentistFilter;

            Console.WriteLine(list.FilterText);
            Console.WriteLine(list.Items.Count);
        }

        // Output:
        // 4
        // dentist
        // 1
    }

    /// <summary>
    /// Writes a value that arrives on a background thread. <c>BindTo</c> hands each write to the label's owning thread, so the
    /// stream needs no <c>ObserveOn</c>; a console process has no UI thread, so here the write runs where the value arrived.
    /// </summary>
    public static void BindBackgroundConnectionStateToLabel()
    {
        InMemoryObjectStorage storage = InMemoryObjectStorage.CreateSeeded();
        StorageBrowserViewModel browser = new(storage);
        StorageBrowserView view = new();
        int callerThread = Environment.CurrentManagedThreadId;

        using (browser.WhenChanged(x => x.ConnectionStatus)
            .Do(state => Console.WriteLine($"{state} arrived on another thread: {Environment.CurrentManagedThreadId != callerThread}"))
            .Select(static state => state.ToString())
            .BindTo(view, v => v.ConnectionLabel.Text))
        {
            // The connection drops on a thread of its own, as a socket callback does.
            Thread socketThread = new(storage.Disconnect);
            socketThread.Start();
            socketThread.Join();

            Console.WriteLine(view.ConnectionLabel.Text);
        }

        // Output:
        // Connected arrived on another thread: False
        // Disconnected arrived on another thread: True
        // Disconnected
    }

    /// <summary>Loads the seeded to-do list.</summary>
    /// <returns>A task that completes with the list, which holds three unfinished items and one finished item.</returns>
    private static async Task<TodoListViewModel> OpenTodoListAsync()
    {
        TodoListViewModel list = new(InMemoryTodoStore.CreateSeeded());

        await list.LoadAsync();
        return list;
    }

    /// <summary>Loads the accounts of the seeded bank.</summary>
    /// <returns>A task that completes with the accounts screen, which lists an everyday and a savings account and has nothing selected.</returns>
    private static async Task<AccountsViewModel> OpenAccountsAsync()
    {
        AccountsViewModel accounts = new(new InMemoryBankingBackend());

        await accounts.LoadAccountsAsync();
        return accounts;
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
