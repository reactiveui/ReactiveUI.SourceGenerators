// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>
/// Shows how ReactiveUI.Binding binds real Avalonia controls, which raise <c>PropertyChanged</c>.
/// <see cref="AvaloniaViewThreadInvoker"/>, from ReactiveUI.Binding.Avalonia, tells the engine which thread owns them.
/// </summary>
public static class AvaloniaExamples
{
    /// <summary>What the transfer example prints for an empty validation message.</summary>
    private const string NoMessage = "(no message)";

    /// <summary>The title of the item before the sync.</summary>
    private const string OriginalTitle = "Renew car registration";

    /// <summary>The title a background sync delivers.</summary>
    private const string RenamedTitle = "Renew car registration online";

    /// <summary>The text the user types to narrow the to-do list.</summary>
    private const string CarFilter = "car";

    /// <summary>The amount the customer types into the transfer form.</summary>
    private const string UtilityAmountText = "250.00";

    /// <summary>The format that shows an amount with two decimal places.</summary>
    private const string TwoDecimalPlaces = "F2";

    /// <summary>The size of the file the upload example sends, in bytes.</summary>
    private const long OffsitePhotoSize = 5_562_368;

    /// <summary>The file the upload example sends.</summary>
    private const string OffsitePhotoName = "team-offsite.jpg";

    /// <summary>The media type of the file the upload example sends.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>Shows that an Avalonia control raises <c>PropertyChanged</c> with the name of the property, which is the notification the engine observes.</summary>
    public static void RaisePropertyChangedFromControl()
    {
        AvaloniaTodoView view = new();
        INotifyPropertyChanged notifying = (INotifyPropertyChanged)view.FilterTextBox;
        notifying.PropertyChanged += static (_, e) => Console.WriteLine(e.PropertyName);

        view.FilterTextBox.Text = CarFilter;

        // Output:
        // Text
    }

    /// <summary>Observes the filter box with <c>WhenChanged</c>; the generator reads the <c>PropertyChanged</c> event at compile time.</summary>
    public static void ObserveFilterBoxWithWhenChanged()
    {
        AvaloniaTodoView view = new();
        List<string?> texts = [];

        using (view.FilterTextBox.WhenChanged(x => x.Text).Subscribe(texts.Add))
        {
            view.FilterTextBox.Text = CarFilter;
        }

        Console.WriteLine(string.Join(", ", texts));

        // Output:
        // , car
    }

    /// <summary>Binds the to-do screen: the filter box and the checkbox both ways, and the count of items left one way.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindTodoFilterAndCheckBoxAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        AvaloniaTodoView view = new() { ViewModel = viewModel };
        int allItems = viewModel.Items.Count;
        viewModel.SelectedItem = viewModel.Items[0];

        using (viewModel.BindTwoWay(view, x => x.FilterText, v => v.FilterTextBox.Text))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem!.IsDone, v => v.DoneCheckBox.IsChecked, static done => done, static isChecked => isChecked == true))
        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            Console.WriteLine(view.RemainingLabel.Text);
            Console.WriteLine(view.DoneCheckBox.IsChecked);

            view.FilterTextBox.Text = CarFilter;

            Console.WriteLine(viewModel.FilterText);
            Console.WriteLine(viewModel.Items.Count < allItems);

            view.DoneCheckBox.IsChecked = true;

            Console.WriteLine(viewModel.SelectedItem!.IsDone);
            Console.WriteLine(viewModel.SelectedItem.Title);
            Console.WriteLine(view.RemainingLabel.Text);
        }

        // Output:
        // 3
        // False
        // car
        // True
        // True
        // Renew car registration
        // 2
    }

    /// <summary>Binds the item list: the view model's items fill the list one way, and the selected item follows the list both ways.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindItemsListAndSelectionAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        AvaloniaTodoView view = new() { ViewModel = viewModel };

        using (viewModel.BindOneWay(view, x => x.Items, v => v.ItemsList.ItemsSource, static items => (IEnumerable)items))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem, v => v.ItemsList.SelectedItem, static item => item, static selected => (TodoItem)selected!))
        {
            Console.WriteLine(view.ItemsList.ItemCount);

            view.ItemsList.SelectedItem = viewModel.Items[1];

            Console.WriteLine(viewModel.SelectedItem!.Title);
        }

        // Output:
        // 4
        // Book dentist appointment
    }

    /// <summary>Fills in the transfer form: the amount box is bound both ways, the validation label follows the draft and the button follows the command.</summary>
    /// <returns>A task that completes when the transfer is sent.</returns>
    public static async Task BindTransferFormAsync()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        await viewModel.LoadAsync();
        using IDisposable confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        AvaloniaTransferView view = new() { ViewModel = viewModel };
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        using (viewModel.Draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, static amount => amount.ToString(TwoDecimalPlaces, CultureInfo.InvariantCulture), ParseAmount))
        using (viewModel.BindOneWay(view, x => x.ValidationSummary, v => v.ValidationLabel.Text))
        using (viewModel.BindOneWay(view, x => x.TransferCommand, v => v.TransferButton.Command, static command => command))
        {
            Console.WriteLine(view.AmountTextBox.Text);
            Console.WriteLine(view.ValidationLabel.Text);
            Console.WriteLine(view.TransferButton.Command!.CanExecute(null));

            view.AmountTextBox.Text = UtilityAmountText;

            Console.WriteLine(viewModel.Draft.Amount);
            Console.WriteLine(string.IsNullOrEmpty(view.ValidationLabel.Text) ? NoMessage : view.ValidationLabel.Text);
            Console.WriteLine(view.TransferButton.Command!.CanExecute(null));

            await viewModel.TransferAsync();

            Console.WriteLine(viewModel.LastReceipt!.NewBalance);
            Console.WriteLine(view.AmountTextBox.Text);
        }

        // Output:
        // 0.00
        // Enter an amount above zero.
        // False
        // 250.00
        // (no message)
        // True
        // 2200.75
        // 0.00
    }

    /// <summary>
    /// Writes from a worker thread through an <c>Unsafe</c> binding with no invoker registered: Avalonia refuses the
    /// write, and the failure reaches the code that changed the view model.
    /// </summary>
    public static void WriteFromWorkerThreadWithoutInvoker()
    {
        AvaloniaTodoView view = new();
        TodoItem item = new() { Title = OriginalTitle };
        string? failure = string.Empty;

        using (item.BindOneWayUnsafe(view, x => x.Title, v => v.RemainingLabel.Text))
        {
            Thread worker = new Thread(() =>
            {
                try
                {
                    item.Title = RenamedTitle;
                }
                catch (Exception ex)
                {
                    failure = ex.GetType().Name;
                }
            });
            worker.Start();
            worker.Join();
        }

        Console.WriteLine(failure);

        // Output:
        // TargetInvocationException
    }

    /// <summary>Follows an upload whose progress arrives on pool threads. The registered invoker claims the bar, so each report waits for the UI thread.</summary>
    /// <returns>A task that completes when the upload is done.</returns>
    public static async Task WriteProgressFromWorkerThreadAsync()
    {
        StorageBrowserViewModel viewModel = new(InMemoryObjectStorage.CreateSeeded());
        await viewModel.LoadBucketsAsync();
        viewModel.SelectedBucket = viewModel.Buckets[0];
        AvaloniaUploadView view = new() { ViewModel = viewModel };
        UploadRequest photo = new(OffsitePhotoName, OffsitePhotoSize, JpegType);

        using (viewModel.BindOneWay(view, x => x.UploadPercent, v => v.UploadProgressBar.Value))
        {
            Console.WriteLine(view.UploadProgressBar.Value);

            await viewModel.UploadAsync(photo);

            Console.WriteLine(view.UploadProgressBar.Value);
        }

        // Output:
        // 0
        // 100
    }

    /// <summary>Reads an amount of money the customer typed.</summary>
    /// <param name="text">The text in the box.</param>
    /// <returns>The amount, or zero when the text is not a number.</returns>
    private static decimal ParseAmount(string? text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : 0M;
}
