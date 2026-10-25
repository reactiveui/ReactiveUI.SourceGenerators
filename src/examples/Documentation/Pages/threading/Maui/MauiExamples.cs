// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections;
using System.Globalization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Maui;
using ReactiveUI.Binding.Maui.Builder;
using ReactiveUI.Primitives;
using Splat;
using Splat.Builder;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>Shows how ReactiveUI.Binding.Maui binds real MAUI controls: the builder module, the Visibility converters and the dispatcher view thread invoker.</summary>
public static class MauiExamples
{
    /// <summary>The text the user types to narrow the to-do list.</summary>
    private const string CarFilter = "car";

    /// <summary>The title of the first seeded to-do item.</summary>
    private const string CarRegistrationTitle = "Renew car registration";

    /// <summary>The amount the customer types into the transfer form.</summary>
    private const string UtilityAmountText = "250.00";

    /// <summary>The text a posted callback records.</summary>
    private const string PostedText = "posted";

    /// <summary>The size of the file the upload example sends, in bytes.</summary>
    private const long OffsitePhotoSize = 5_562_368;

    /// <summary>The value of a full progress bar, in percent.</summary>
    private const double FullPercent = 100;

    /// <summary>The file the upload example sends.</summary>
    private const string OffsitePhotoName = "team-offsite.jpg";

    /// <summary>The media type of the file the upload example sends.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>Builds the application with the MAUI module, which registers the Visibility converters with the resolver; importing them makes them available to bindings.</summary>
    public static void BuildMauiApplication()
    {
        ReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        IReactiveUIBindingInstance app = builder.WithCoreServices().WithMaui().BuildApp();
        ViewThreadInvokers.Refresh();
        IBindingTypeConverter[] converters = app.Current!.GetServices<IBindingTypeConverter>().ToArray();

        Console.WriteLine(app.Current!.GetServices<IViewThreadInvoker>().OfType<DispatcherViewThreadInvoker>().Any());
        Console.WriteLine(converters.OfType<BooleanToVisibilityTypeConverter>().Any());
        Console.WriteLine(converters.OfType<VisibilityToBooleanTypeConverter>().Any());
        Console.WriteLine(builder.ConverterService.TypedConverters.TryGetConverter(typeof(bool), typeof(Visibility)) is null);

        builder.ConverterService.ImportFrom(app.Current!);

        Console.WriteLine(BindingConverters.Current.TypedConverters.TryGetConverter(typeof(bool), typeof(Visibility)) is BooleanToVisibilityTypeConverter);
        Console.WriteLine(BindingConverters.Current.TypedConverters.TryGetConverter(typeof(Visibility), typeof(bool)) is VisibilityToBooleanTypeConverter);

        // Output:
        // True
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Calls <c>WithMaui</c> on a builder held as <see cref="IAppBuilder"/>; it returns the same builder.</summary>
    public static void ChainWithMauiThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IAppBuilder appBuilder = resolver.CreateReactiveUIBindingBuilder();

        IReactiveUIBindingBuilder chained = appBuilder.WithMaui();

        Console.WriteLine(ReferenceEquals(appBuilder, chained));

        // Output:
        // True
    }

    /// <summary>Applies the MAUI module to a resolver you own; it registers the invoker and both converters.</summary>
    public static void ConfigureMauiModuleOnResolver()
    {
        using ModernDependencyResolver resolver = new();
        MauiBindingModule module = new();

        module.Configure(resolver);

        IBindingTypeConverter[] converters = resolver.GetServices<IBindingTypeConverter>().ToArray();

        Console.WriteLine(resolver.GetServices<IViewThreadInvoker>().OfType<DispatcherViewThreadInvoker>().Any());
        Console.WriteLine(converters.OfType<BooleanToVisibilityTypeConverter>().Any());
        Console.WriteLine(converters.OfType<VisibilityToBooleanTypeConverter>().Any());

        // Output:
        // True
        // True
        // True
    }

    /// <summary>Converts the validation state to a <see cref="Visibility"/> with each hint.</summary>
    public static void ConvertBooleanToVisibility()
    {
        BooleanToVisibilityTypeConverter converter = new();
        (bool Value, object? Hint)[] cases =
        [
            (true, null),
            (false, null),
            (false, BooleanToVisibilityHints.None),
            (true, BooleanToVisibilityHints.Inverse),
            (false, BooleanToVisibilityHints.Inverse),
            (false, BooleanToVisibilityHints.UseHidden),
            (true, BooleanToVisibilityHints.UseHidden),
            (true, BooleanToVisibilityHints.Inverse | BooleanToVisibilityHints.UseHidden),
            (false, "not a hint"),
        ];

        Console.WriteLine(converter.GetAffinityForObjects());

        foreach (var (value, hint) in cases)
        {
            _ = converter.TryConvert(value, hint, out var visibility);

            Console.WriteLine(visibility);
        }

        // Output:
        // 2
        // Visible
        // Collapsed
        // Collapsed
        // Collapsed
        // Visible
        // Hidden
        // Visible
        // Hidden
        // Collapsed
    }

    /// <summary>Converts a <see cref="Visibility"/> back to a boolean; only Visible is true, and the inverse hint flips the result.</summary>
    public static void ConvertVisibilityToBoolean()
    {
        VisibilityToBooleanTypeConverter converter = new();
        (Visibility Value, object? Hint)[] cases =
        [
            (Visibility.Visible, null),
            (Visibility.Hidden, null),
            (Visibility.Collapsed, BooleanToVisibilityHints.None),
            (Visibility.Visible, BooleanToVisibilityHints.Inverse),
            (Visibility.Collapsed, BooleanToVisibilityHints.Inverse),
            (Visibility.Collapsed, "not a hint"),
        ];

        Console.WriteLine(converter.GetAffinityForObjects());

        foreach (var (value, hint) in cases)
        {
            _ = converter.TryConvert(value, hint, out var isVisible);

            Console.WriteLine(isVisible);
        }

        // Output:
        // 2
        // True
        // False
        // False
        // False
        // True
        // False
    }

    /// <summary>Observes the filter entry with <c>WhenChanged</c>; a MAUI control is a <c>BindableObject</c>, which raises <c>PropertyChanged</c>.</summary>
    public static void ObserveFilterEntryWithWhenChanged()
    {
        MauiTodoPage view = new();
        List<string?> texts = [];

        using (view.FilterEntry.WhenChanged(x => x.Text).Subscribe(texts.Add))
        {
            view.FilterEntry.Text = CarFilter;
        }

        Console.WriteLine(string.Join(", ", texts));

        // Output:
        // , car
    }

    /// <summary>Fills in the transfer form: the amount entry is bound both ways, the validation label shows through the Visibility converter and the button follows the command.</summary>
    /// <returns>A task that completes when the transfer is sent.</returns>
    public static async Task BindTransferFormAsync()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        await viewModel.LoadAsync();
        using IDisposable confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        MauiTransferPage view = new() { ViewModel = viewModel };
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        using (viewModel.Draft.BindTwoWay(view, x => x.Amount, v => v.AmountEntry.Text, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter()))
        using (view.OneWayBind(viewModel, x => x.IsValid, v => v.ValidationVisibility, new BooleanToVisibilityTypeConverter(), BooleanToVisibilityHints.Inverse))
        using (viewModel.BindOneWay(view, x => x.ValidationSummary, v => v.ValidationLabel.Text))
        using (view.BindCommand(viewModel, x => x.TransferCommand, v => v.TransferButton))
        {
            Console.WriteLine(view.AmountEntry.Text);
            Console.WriteLine(view.ValidationVisibility);
            Console.WriteLine(view.ValidationLabel.Text);
            Console.WriteLine(view.TransferButton.IsEnabled);

            view.AmountEntry.Text = UtilityAmountText;

            Console.WriteLine(viewModel.Draft.Amount);
            Console.WriteLine(view.ValidationVisibility);
            Console.WriteLine(view.ValidationLabel.IsVisible);
            Console.WriteLine(view.TransferButton.IsEnabled);

            await viewModel.TransferAsync();

            Console.WriteLine(viewModel.LastReceipt!.NewBalance);
            Console.WriteLine(view.AmountEntry.Text);
            Console.WriteLine(view.ValidationVisibility);
        }

        // Output:
        // 0
        // Visible
        // Enter an amount above zero.
        // False
        // 250.00
        // Collapsed
        // False
        // True
        // 2200.75
        // 0
        // Visible
    }

    /// <summary>Binds the to-do screen: the filter entry and the checkbox both ways, and the count of items left one way.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindTodoFilterAndCheckBoxAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        MauiTodoPage view = new() { ViewModel = viewModel };
        int allItems = viewModel.Items.Count;
        viewModel.SelectedItem = viewModel.Items[0];

        using (viewModel.BindTwoWay(view, x => x.FilterText, v => v.FilterEntry.Text))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem!.IsDone, v => v.DoneCheckBox.IsChecked))
        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            Console.WriteLine(view.RemainingLabel.Text);
            Console.WriteLine(view.DoneCheckBox.IsChecked);

            view.FilterEntry.Text = CarFilter;

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

    /// <summary>Binds a tick mark both ways through the two Visibility converters: the item shows the mark, and hiding the mark reopens the item.</summary>
    public static void BindDoneMarkWithBothConverters()
    {
        TodoItem item = new() { Title = CarRegistrationTitle };
        MauiTodoPage view = new();

        using (item.BindTwoWay(view, x => x.IsDone, v => v.DoneMarkVisibility, new BooleanToVisibilityTypeConverter(), new VisibilityToBooleanTypeConverter()))
        {
            Console.WriteLine(view.DoneMarkVisibility);
            Console.WriteLine(view.DoneMark.IsVisible);

            item.IsDone = true;

            Console.WriteLine(view.DoneMarkVisibility);
            Console.WriteLine(view.DoneMark.IsVisible);

            view.DoneMarkVisibility = Visibility.Hidden;

            Console.WriteLine(item.IsDone);
        }

        // Output:
        // Collapsed
        // False
        // Visible
        // True
        // False
    }

    /// <summary>Binds a <c>CollectionView</c>: the view model's items fill the list one way, and the selected item follows the list both ways.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindListViewItemsAndSelectionAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        TodoView view = new() { ViewModel = viewModel };

        using (viewModel.BindOneWay(view, x => x.Items, v => v.ItemsList.ItemsSource, static items => (IEnumerable)items))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem, v => v.ItemsList.SelectedItem, static item => item, static selected => (TodoItem)selected!))
        {
            Console.WriteLine(view.ItemsList.ItemsSource.Cast<object>().Count());

            view.ItemsList.SelectedItem = viewModel.Items[1];

            Console.WriteLine(viewModel.SelectedItem!.Title);

            viewModel.SelectedItem = viewModel.Items[2];

            Console.WriteLine(((TodoItem)view.ItemsList.SelectedItem).Title);
        }

        // Output:
        // 4
        // Book dentist appointment
        // Buy birthday present for Sam
    }

    /// <summary>Calls the three members of the dispatcher invoker. With no dispatcher, as in a unit test, the calling thread owns every control.</summary>
    public static void CallInvokerMembers()
    {
        DispatcherViewThreadInvoker invoker = new();
        StorageBrowserView view = new();
        ProgressBar progressBar = view.UploadProgressBar;
        List<string> log = [];
        object unclaimed = new();
        bool accessFromWorker = false;

        Thread worker = new Thread(() => accessFromWorker = invoker.CheckAccess(progressBar));
        worker.Start();
        worker.Join();
        invoker.Post(progressBar, static state => ((List<string>)state!).Add(PostedText), log);

        Console.WriteLine(invoker.Claims(progressBar));
        Console.WriteLine(invoker.Claims(unclaimed));
        Console.WriteLine(invoker.CheckAccess(progressBar));
        Console.WriteLine(accessFromWorker);
        Console.WriteLine(string.Join(", ", log));

        // Output:
        // True
        // False
        // True
        // True
        // posted
    }

    /// <summary>Calls the three members through <see cref="IViewThreadInvoker"/>, the type every platform module registers; a platform of your own implements the same three.</summary>
    /// <param name="invoker">The invoker to ask, such as the <see cref="DispatcherViewThreadInvoker"/> the MAUI module registers.</param>
    public static void CallInvokerThroughInterface(IViewThreadInvoker invoker)
    {
        StorageBrowserView view = new();
        ProgressBar progressBar = view.UploadProgressBar;
        List<string> log = [];

        bool claimed = invoker.Claims(progressBar);
        bool mayWrite = invoker.CheckAccess(progressBar);
        invoker.Post(progressBar, static state => ((List<string>)state!).Add(PostedText), log);

        Console.WriteLine(claimed);
        Console.WriteLine(mayWrite);
        Console.WriteLine(string.Join(", ", log));

        // Output:
        // True
        // True
        // posted
    }

    /// <summary>Uses the invoker where no dispatcher exists: MAUI throws when asked for one, so the invoker lets any thread write and posts inline.</summary>
    public static void PostWithoutDispatcherRunsInline()
    {
        DispatcherViewThreadInvoker invoker = new();
        StorageBrowserView view = new();
        List<string> log = [];

        invoker.Post(view.UploadProgressBar, static state => ((List<string>)state!).Add(PostedText), log);

        Console.WriteLine(string.Join(", ", log));

        // Output:
        // posted
    }

    /// <summary>Rejects a missing callback.</summary>
    public static void RejectMissingCallback()
    {
        DispatcherViewThreadInvoker invoker = new();
        StorageBrowserView view = new();
        bool rejected = false;

        try
        {
            invoker.Post(view.UploadProgressBar, null!, null);
        }
        catch (ArgumentNullException)
        {
            rejected = true;
        }

        Console.WriteLine(rejected);

        // Output:
        // True
    }

    /// <summary>Follows an upload whose progress arrives on a pool thread. No module is registered, so the generated binding carries its own MAUI invoker.</summary>
    /// <returns>A task that completes when the upload is done.</returns>
    public static async Task WriteProgressFromWorkerThreadWithoutModuleAsync()
    {
        StorageBrowserView view = new();

        Console.WriteLine(ViewThreadInvokers.ForTarget(view.UploadProgressBar) is null);

        await RunUploadFromWorkerThreadAsync(view);

        // Output:
        // True
        // True
        // 1
    }

    /// <summary>Follows the same upload after <see cref="BuildMauiApplication"/>: the registered dispatcher invoker claims the bar.</summary>
    /// <returns>A task that completes when the upload is done.</returns>
    public static async Task WriteProgressFromWorkerThreadThroughRegisteredInvokerAsync()
    {
        StorageBrowserView view = new();

        Console.WriteLine(ViewThreadInvokers.ForTarget(view.UploadProgressBar) is DispatcherViewThreadInvoker);

        await RunUploadFromWorkerThreadAsync(view);

        // Output:
        // True
        // True
        // 1
    }

    /// <summary>Uploads a photo on a pool thread and shows where the bar was written.</summary>
    /// <param name="view">The screen whose bar follows the upload.</param>
    /// <returns>A task that completes when the upload is done.</returns>
    private static async Task RunUploadFromWorkerThreadAsync(StorageBrowserView view)
    {
        StorageBrowserViewModel viewModel = new(InMemoryObjectStorage.CreateSeeded());
        await viewModel.LoadBucketsAsync();
        viewModel.SelectedBucket = viewModel.Buckets[0];
        UploadRequest photo = new(OffsitePhotoName, OffsitePhotoSize, JpegType);
        bool writtenOnPoolThread = false;

        using (view.UploadProgressBar.WhenChanged(x => x.Progress).Subscribe(_ => writtenOnPoolThread |= Thread.CurrentThread.IsThreadPoolThread))
        using (viewModel.BindOneWay(view, x => x.UploadPercent, v => v.UploadProgressBar.Progress, static percent => percent / FullPercent))
        {
            await viewModel.UploadAsync(photo);

            Console.WriteLine(writtenOnPoolThread);
            Console.WriteLine(view.UploadProgressBar.Progress);
        }
    }
}
