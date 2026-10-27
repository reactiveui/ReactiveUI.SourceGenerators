// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Wpf;
using ReactiveUI.Binding.Wpf.Builder;
using ReactiveUI.Primitives;
using Splat;
using Splat.Builder;
using Expression = System.Linq.Expressions.Expression;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>Shows how ReactiveUI.Binding.Wpf binds real WPF controls: the builder module, dependency-property observation, the Visibility converters and the dispatcher view thread invoker.</summary>
public static class WpfExamples
{
    /// <summary>The text the user types to narrow the to-do list.</summary>
    private const string CarFilter = "car";

    /// <summary>The text the user types after the first filter.</summary>
    private const string CarsFilter = "cars";

    /// <summary>The title of the first seeded to-do item.</summary>
    private const string CarRegistrationTitle = "Renew car registration";

    /// <summary>The property whose dependency property the examples observe.</summary>
    private const string TextPropertyName = "Text";

    /// <summary>The amount the customer types into the transfer form.</summary>
    private const string UtilityAmountText = "250.00";

    /// <summary>The text a posted callback records.</summary>
    private const string PostedText = "posted";

    /// <summary>The size of the file the upload example sends, in bytes.</summary>
    private const long OffsitePhotoSize = 5_562_368;

    /// <summary>The value of a full progress bar.</summary>
    private const double FullPercent = 100;

    /// <summary>The file the upload example sends.</summary>
    private const string OffsitePhotoName = "team-offsite.jpg";

    /// <summary>The media type of the file the upload example sends.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>Builds the application with the WPF module, which adds no Visibility converter, and registers both converters.</summary>
    public static void BuildWpfApplication()
    {
        ReactiveUIBindingBuilder? builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();
        _ = builder.WithCoreServices().WithWpf();

        Console.WriteLine(builder.ConverterService.TypedConverters.TryGetConverter(typeof(bool), typeof(Visibility)) is null);
        Console.WriteLine(builder.ConverterService.TypedConverters.TryGetConverter(typeof(Visibility), typeof(bool)) is null);

        IReactiveUIBindingInstance? app = builder
            .WithConverter(new BooleanToVisibilityTypeConverter())
            .WithConverter(new VisibilityToBooleanTypeConverter())
            .BuildApp();
        ViewThreadInvokers.Refresh();

        Console.WriteLine(app.Current!.GetServices<ICreatesObservableForProperty>().OfType<DependencyObjectObservableForProperty>().Any());
        Console.WriteLine(app.Current!.GetServices<IViewThreadInvoker>().OfType<DispatcherViewThreadInvoker>().Any());
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

    /// <summary>Calls <c>WithWpf</c> on a builder held as <see cref="IAppBuilder"/>; it returns the same builder.</summary>
    public static void ChainWithWpfThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IAppBuilder appBuilder = resolver.CreateReactiveUIBindingBuilder();

        IReactiveUIBindingBuilder? chained = appBuilder.WithWpf();

        Console.WriteLine(ReferenceEquals(appBuilder, chained));

        // Output:
        // True
    }

    /// <summary>Calls <c>WithWpf</c> on a builder held as <see cref="IReactiveUIBindingBuilder"/>; it returns the same builder.</summary>
    public static void ChainWithWpfThroughBindingBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IReactiveUIBindingBuilder? builder = (IReactiveUIBindingBuilder)resolver.CreateReactiveUIBindingBuilder();

        IReactiveUIBindingBuilder? chained = builder.WithWpf();

        Console.WriteLine(ReferenceEquals(builder, chained));

        // Output:
        // True
    }

    /// <summary>Applies the WPF module to a resolver you own; it registers the observer and the invoker.</summary>
    public static void ConfigureWpfModuleOnResolver()
    {
        using ModernDependencyResolver resolver = new();
        WpfBindingModule module = new();

        module.Configure(resolver);

        Console.WriteLine(resolver.GetServices<ICreatesObservableForProperty>().OfType<DependencyObjectObservableForProperty>().Any());
        Console.WriteLine(resolver.GetServices<IViewThreadInvoker>().OfType<DispatcherViewThreadInvoker>().Any());

        // Output:
        // True
        // True
    }

    /// <summary>Asks the provider which properties it observes: a <c>DependencyObject</c> that declares the dependency property, and nothing else.</summary>
    public static void DescribeDependencyPropertyAffinity()
    {
        DependencyObjectObservableForProperty provider = new();

        Console.WriteLine(provider.GetAffinityForObject(typeof(TextBox), TextPropertyName, false));
        Console.WriteLine(provider.GetAffinityForObject(typeof(TextBox), TextPropertyName, true));
        Console.WriteLine(provider.GetAffinityForObject(typeof(TextBox), "Missing", false));
        Console.WriteLine(provider.GetAffinityForObject(typeof(TodoItem), "Title", false));

        // Output:
        // 4
        // 4
        // 0
        // 0
    }

    /// <summary>Observes the filter box through the provider: it raises after each change and carries no value, so the observer reads the property.</summary>
    public static void ObserveTextBoxThroughProvider()
    {
        DependencyObjectObservableForProperty provider = new();
        WpfTodoWindow view = new();
        ConstantExpression? expression = Expression.Constant(view.FilterTextBox);
        List<string> texts = [];

        using (provider.GetNotificationForProperty(view.FilterTextBox, expression, TextPropertyName, false, false).Subscribe(change => texts.Add(((TextBox)change.Sender).Text)))
        {
            view.FilterTextBox.Text = CarFilter;
            view.FilterTextBox.Text = CarsFilter;
        }

        view.FilterTextBox.Text = CarFilter;

        Console.WriteLine(string.Join(", ", texts));

        // Output:
        // car, cars
    }

    /// <summary>Rejects an observation of a property the control has no dependency property for.</summary>
    public static void RejectPropertyWithoutDependencyProperty()
    {
        DependencyObjectObservableForProperty provider = new();
        WpfTodoWindow view = new();
        ConstantExpression? expression = Expression.Constant(view.FilterTextBox);
        bool rejected = false;

        try
        {
            _ = provider.GetNotificationForProperty(view.FilterTextBox, expression, "Missing", false, false);
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        Console.WriteLine(rejected);

        // Output:
        // True
    }

    /// <summary>Observes the filter box with <c>WhenChanged</c>; the generator reads the dependency property at compile time.</summary>
    public static void ObserveFilterBoxWithWhenChanged()
    {
        WpfTodoWindow view = new();
        List<string> texts = [];

        using (view.FilterTextBox.WhenChanged(x => x.Text).Subscribe(texts.Add))
        {
            view.FilterTextBox.Text = CarFilter;
        }

        Console.WriteLine(string.Join(", ", texts));

        // Output:
        // , car
    }

    /// <summary>Keeps the window's <c>DataContext</c> in step with its <c>ViewModel</c>: <c>ViewModel</c> is a dependency property, so <c>WhenChanged</c> observes it.</summary>
    public static void KeepDataContextInStepWithViewModel()
    {
        WpfTodoWindow view = new();
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());

        using (view.WhenChanged(x => x.ViewModel).BindTo(view, x => x.DataContext))
        {
            Console.WriteLine(view.DataContext is null);

            view.ViewModel = viewModel;

            Console.WriteLine(ReferenceEquals(view.DataContext, viewModel));
        }

        // Output:
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

    /// <summary>Fills in the transfer form: the amount box is bound both ways, the validation label shows through the Visibility converter and the button follows the command.</summary>
    /// <returns>A task that completes when the transfer is sent.</returns>
    public static async Task BindTransferFormAsync()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        await viewModel.LoadAsync();
        using IDisposable? confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        WpfTransferWindow view = new() { ViewModel = viewModel };
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        using (viewModel.Draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter()))
        using (view.OneWayBind(viewModel, x => x.IsValid, v => v.ValidationLabel.Visibility, new BooleanToVisibilityTypeConverter(), BooleanToVisibilityHints.Inverse))
        using (view.OneWayBind(viewModel, x => x.ValidationSummary, v => v.ValidationLabel.Text))
        {
            using (view.BindCommand(viewModel, x => x.TransferCommand, v => v.TransferButton))
            {
                Console.WriteLine(view.AmountTextBox.Text);
                Console.WriteLine(view.ValidationLabel.Visibility);
                Console.WriteLine(view.ValidationLabel.Text);
                Console.WriteLine(view.TransferButton.IsEnabled);

                view.AmountTextBox.Text = UtilityAmountText;

                Console.WriteLine(viewModel.Draft.Amount);
                Console.WriteLine(view.ValidationLabel.Visibility);
                Console.WriteLine(view.TransferButton.IsEnabled);
            }

            await viewModel.TransferAsync();

            Console.WriteLine(viewModel.LastReceipt!.NewBalance);
            Console.WriteLine(view.AmountTextBox.Text);
            Console.WriteLine(view.ValidationLabel.Visibility);
        }

        // Output:
        // 0
        // Visible
        // Enter an amount above zero.
        // False
        // 250.00
        // Collapsed
        // True
        // 2200.75
        // 0
        // Visible
    }

    /// <summary>Binds the to-do screen: the filter box and the checkbox both ways, and the count of items left one way.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindTodoFilterAndCheckBoxAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        WpfTodoWindow view = new() { ViewModel = viewModel };
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

    /// <summary>Binds a tick mark both ways through the converters the application registered: the item shows the mark, and hiding the mark reopens the item.</summary>
    public static void BindDoneMarkWithBothConverters()
    {
        TodoItem item = new() { Title = CarRegistrationTitle };
        WpfTodoWindow view = new();

        using (item.BindTwoWay(view, x => x.IsDone, v => v.DoneMark.Visibility, new BooleanToVisibilityTypeConverter(), new VisibilityToBooleanTypeConverter()))
        {
            Console.WriteLine(view.DoneMark.Visibility);

            item.IsDone = true;

            Console.WriteLine(view.DoneMark.Visibility);

            view.DoneMark.Visibility = Visibility.Hidden;

            Console.WriteLine(item.IsDone);
        }

        // Output:
        // Collapsed
        // Visible
        // False
    }

    /// <summary>Shows that WPF refuses a write from a thread that does not own the control. A binding has to avoid this.</summary>
    public static void RefuseWriteFromWorkerThread()
    {
        WpfUploadWindow view = new();
        bool refused = false;

        Thread? worker = new Thread(() =>
        {
            try
            {
                view.UploadProgressBar.Value = FullPercent;
            }
            catch (InvalidOperationException)
            {
                refused = true;
            }
        });
        worker.Start();
        worker.Join();

        Console.WriteLine(refused);
        Console.WriteLine(view.UploadProgressBar.Value);

        // Output:
        // True
        // 0
    }

    /// <summary>Calls the three members of the dispatcher invoker: who it claims, who may write, and how to queue work on the owning thread.</summary>
    public static void CallInvokerMembers()
    {
        DispatcherViewThreadInvoker invoker = new();
        WpfUploadWindow view = new();
        System.Windows.Controls.ProgressBar? progressBar = view.UploadProgressBar;
        List<string> log = [];
        object unclaimed = new();
        bool accessFromWorker = true;

        Thread? worker = new Thread(() =>
        {
            accessFromWorker = invoker.CheckAccess(progressBar);
            invoker.Post(progressBar, static state => ((List<string>)state!).Add(PostedText), log);
        });
        worker.Start();
        worker.Join();

        Console.WriteLine(invoker.Claims(progressBar));
        Console.WriteLine(invoker.Claims(unclaimed));
        Console.WriteLine(invoker.CheckAccess(progressBar));
        Console.WriteLine(accessFromWorker);
        Console.WriteLine(log.Count);

        progressBar.Dispatcher.Invoke(static () => { }, DispatcherPriority.Background);

        Console.WriteLine(string.Join(", ", log));

        // Output:
        // True
        // False
        // True
        // False
        // 0
        // posted
    }

    /// <summary>Posts to a frozen brush, which belongs to no thread: the invoker runs the callback at once.</summary>
    public static void PostToFrozenBrushRunsInline()
    {
        DispatcherViewThreadInvoker invoker = new();
        SolidColorBrush brush = new(Colors.Green);
        brush.Freeze();
        List<string> log = [];

        invoker.Post(brush, static state => ((List<string>)state!).Add(PostedText), log);

        Console.WriteLine(string.Join(", ", log));

        // Output:
        // posted
    }

    /// <summary>Rejects a missing callback.</summary>
    public static void RejectMissingCallback()
    {
        DispatcherViewThreadInvoker invoker = new();
        WpfUploadWindow view = new();
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

    /// <summary>Follows an upload whose progress arrives on a pool thread. No module is registered, so the generated binding carries its own WPF invoker.</summary>
    /// <returns>A task that completes when the upload is done.</returns>
    public static async Task WriteProgressFromWorkerThreadWithoutModuleAsync()
    {
        WpfUploadWindow view = new();

        Console.WriteLine(ViewThreadInvokers.ForTarget(view.UploadProgressBar) is null);

        await RunUploadFromWorkerThreadAsync(view);

        // Output:
        // True
        // 100
    }

    /// <summary>Follows the same upload after <see cref="BuildWpfApplication"/>: the registered dispatcher invoker claims the bar.</summary>
    /// <returns>A task that completes when the upload is done.</returns>
    public static async Task WriteProgressFromWorkerThreadThroughRegisteredInvokerAsync()
    {
        WpfUploadWindow view = new();

        Console.WriteLine(ViewThreadInvokers.ForTarget(view.UploadProgressBar) is DispatcherViewThreadInvoker);

        await RunUploadFromWorkerThreadAsync(view);

        // Output:
        // True
        // 100
    }

    /// <summary>Uploads a photo on a pool thread; each progress report waits for the owning thread, and the bar shows the last one.</summary>
    /// <param name="view">The window whose bar follows the upload.</param>
    /// <returns>A task that completes when the upload is done.</returns>
    private static async Task RunUploadFromWorkerThreadAsync(WpfUploadWindow view)
    {
        StorageBrowserViewModel viewModel = new(InMemoryObjectStorage.CreateSeeded());
        await viewModel.LoadBucketsAsync();
        viewModel.SelectedBucket = viewModel.Buckets[0];
        UploadRequest photo = new(OffsitePhotoName, OffsitePhotoSize, JpegType);

        using (viewModel.BindOneWay(view, x => x.UploadPercent, v => v.UploadProgressBar.Value))
        {
            await viewModel.UploadAsync(photo);

            Console.WriteLine(view.UploadProgressBar.Value);
        }
    }
}
