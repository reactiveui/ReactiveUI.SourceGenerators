// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using System.Linq.Expressions;
using System.Windows.Forms;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.WinForms;
using ReactiveUI.Binding.WinForms.Builder;
using ReactiveUI.Primitives;
using Splat;
using Splat.Builder;
using Application = System.Windows.Forms.Application;
using CheckBox = System.Windows.Forms.CheckBox;

namespace ReactiveUI.Binding.Documentation.Threading;

/// <summary>Shows how ReactiveUI.Binding.WinForms binds real Windows Forms controls: the builder module, <c>{Name}Changed</c> event observation and the control view thread invoker.</summary>
public static class WinFormsExamples
{
    /// <summary>The text the user types to narrow the to-do list.</summary>
    private const string CarFilter = "car";

    /// <summary>The text the user types after the first filter.</summary>
    private const string CarsFilter = "cars";

    /// <summary>The property whose change event the examples observe.</summary>
    private const string TextPropertyName = "Text";

    /// <summary>The amount the customer types into the transfer form.</summary>
    private const string UtilityAmountText = "250.00";

    /// <summary>The text a posted callback records.</summary>
    private const string PostedText = "posted";

    /// <summary>The size of the file the upload example sends, in bytes.</summary>
    private const long OffsitePhotoSize = 5_562_368;

    /// <summary>The value of a full progress bar.</summary>
    private const int FullPercent = 100;

    /// <summary>The file the upload example sends.</summary>
    private const string OffsitePhotoName = "team-offsite.jpg";

    /// <summary>The media type of the file the upload example sends.</summary>
    private const string JpegType = "image/jpeg";

    /// <summary>Builds the application with the WinForms module, which adds event-based observation and the control invoker.</summary>
    public static void BuildWinFormsApplication()
    {
        IReactiveUIBindingInstance? app = RxBindingBuilder.CreateReactiveUIBindingBuilder()
            .WithCoreServices()
            .WithWinForms()
            .BuildApp();
        ViewThreadInvokers.Refresh();

        Console.WriteLine(app.Current!.GetServices<ICreatesObservableForProperty>().OfType<WinFormsCreatesObservableForProperty>().Any());
        Console.WriteLine(app.Current!.GetServices<IViewThreadInvoker>().OfType<ControlViewThreadInvoker>().Any());

        // Output:
        // True
        // True
    }

    /// <summary>Calls <c>WithWinForms</c> on a builder held as <see cref="IAppBuilder"/>; it returns the same builder.</summary>
    public static void ChainWithWinFormsThroughAppBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IAppBuilder appBuilder = resolver.CreateReactiveUIBindingBuilder();

        IReactiveUIBindingBuilder? chained = appBuilder.WithWinForms();

        Console.WriteLine(ReferenceEquals(appBuilder, chained));

        // Output:
        // True
    }

    /// <summary>Calls <c>WithWinForms</c> on a builder held as <see cref="IReactiveUIBindingBuilder"/>; it returns the same builder.</summary>
    public static void ChainWithWinFormsThroughBindingBuilder()
    {
        using ModernDependencyResolver resolver = new();
        IReactiveUIBindingBuilder? builder = (IReactiveUIBindingBuilder)resolver.CreateReactiveUIBindingBuilder();

        IReactiveUIBindingBuilder? chained = builder.WithWinForms();

        Console.WriteLine(ReferenceEquals(builder, chained));

        // Output:
        // True
    }

    /// <summary>Applies the WinForms module to a resolver you own; it registers the observer and the invoker.</summary>
    public static void ConfigureWinFormsModuleOnResolver()
    {
        using ModernDependencyResolver resolver = new();
        WinFormsBindingModule module = new();

        module.Configure(resolver);

        Console.WriteLine(resolver.GetServices<ICreatesObservableForProperty>().OfType<WinFormsCreatesObservableForProperty>().Any());
        Console.WriteLine(resolver.GetServices<IViewThreadInvoker>().OfType<ControlViewThreadInvoker>().Any());

        // Output:
        // True
        // True
    }

    /// <summary>Asks the observer which properties it handles: a component property with a public <c>{Name}Changed</c> event, and only after the change.</summary>
    public static void DescribeChangedEventAffinity()
    {
        WinFormsCreatesObservableForProperty observer = new();

        Console.WriteLine(observer.GetAffinityForObject(typeof(TextBox), TextPropertyName, false));
        Console.WriteLine(observer.GetAffinityForObject(typeof(CheckBox), "Checked", false));
        Console.WriteLine(observer.GetAffinityForObject(typeof(ListBox), "DataSource", false));
        Console.WriteLine(observer.GetAffinityForObject(typeof(ListBox), "SelectedValue", false));
        Console.WriteLine(observer.GetAffinityForObject(typeof(ListBox), "SelectedIndex", false));
        Console.WriteLine(observer.GetAffinityForObject(typeof(ListBox), "SelectedItem", false));
        Console.WriteLine(observer.GetAffinityForObject(typeof(TextBox), TextPropertyName, true));
        Console.WriteLine(observer.GetAffinityForObject(typeof(TextBox), "Missing", false));
        Console.WriteLine(observer.GetAffinityForObject(typeof(TodoItem), "Title", false));

        // Output:
        // 8
        // 8
        // 8
        // 8
        // 8
        // 0
        // 0
        // 0
        // 0
    }

    /// <summary>Observes the filter box through the observer: it raises after each <c>TextChanged</c> and carries no value, so the subscriber reads the property.</summary>
    public static void ObserveTextBoxThroughObserver()
    {
        WinFormsCreatesObservableForProperty observer = new();
        using WinFormsTodoForm view = new();
        ConstantExpression? expression = Expression.Constant(view.FilterTextBox);
        List<string> texts = [];

        using (observer.GetNotificationForProperty(view.FilterTextBox, expression, TextPropertyName, false, false).Subscribe(change => texts.Add(((TextBox)change.Sender).Text)))
        {
            view.FilterTextBox.Text = CarFilter;
            view.FilterTextBox.Text = CarsFilter;
        }

        view.FilterTextBox.Text = CarFilter;

        Console.WriteLine(string.Join(", ", texts));

        // Output:
        // car, cars
    }

    /// <summary>Rejects an observation of a property the control raises no change event for.</summary>
    public static void RejectPropertyWithoutChangedEvent()
    {
        WinFormsCreatesObservableForProperty observer = new();
        using WinFormsTodoForm view = new();
        ConstantExpression? expression = Expression.Constant(view.FilterTextBox);
        bool rejected = false;

        try
        {
            _ = observer.GetNotificationForProperty(view.FilterTextBox, expression, "Missing", false, false);
        }
        catch (ArgumentException)
        {
            rejected = true;
        }

        Console.WriteLine(rejected);

        // Output:
        // True
    }

    /// <summary>Observes the filter box with <c>WhenChanged</c>; the generator reads the <c>TextChanged</c> event at compile time.</summary>
    public static void ObserveFilterBoxWithWhenChanged()
    {
        using WinFormsTodoForm view = new();
        List<string> texts = [];

        using (view.FilterTextBox.WhenChanged(x => x.Text).Subscribe(texts.Add))
        {
            view.FilterTextBox.Text = CarFilter;
        }

        Console.WriteLine(string.Join(", ", texts));

        // Output:
        // , car
    }

    /// <summary>Binds the item list to <c>ListBox.DataSource</c> and the priority to <c>ComboBox.SelectedValue</c>, which needs a <c>ValueMember</c>.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindItemsListAndPriorityPickerAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        using WinFormsTodoForm view = new() { ViewModel = viewModel };
        _ = view.Handle;
        view.ItemsList.DisplayMember = nameof(TodoItem.Title);
        view.PriorityBox.DataSource = Enum.GetValues<TodoPriority>().Select(static priority => new { Name = priority.ToString(), Value = priority }).ToList();
        view.PriorityBox.DisplayMember = "Name";
        view.PriorityBox.ValueMember = "Value";
        viewModel.SelectedItem = viewModel.Items[1];

        using (viewModel.BindOneWay(view, x => x.Items, v => v.ItemsList.DataSource, static items => (object)items.ToList()))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem!.Priority, v => v.PriorityBox.SelectedValue, static priority => priority, ToPriority))
        {
            Console.WriteLine(view.ItemsList.Items.Count);
            Console.WriteLine(view.PriorityBox.SelectedValue);

            view.PriorityBox.SelectedValue = TodoPriority.High;

            Console.WriteLine(viewModel.SelectedItem!.Priority);

            viewModel.SelectedItem.Priority = TodoPriority.Low;

            Console.WriteLine(view.PriorityBox.SelectedValue);
        }

        // Output:
        // 4
        // Normal
        // High
        // Low
    }

    /// <summary>Binds <c>ComboBox.SelectedIndex</c> to the priority as a number: <c>SelectedIndexChanged</c> is the event the observer follows.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindPriorityPickerToSelectedIndexAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        using WinFormsTodoForm view = new() { ViewModel = viewModel };
        _ = view.Handle;
        view.PriorityBox.DataSource = Enum.GetValues<TodoPriority>();
        viewModel.SelectedItem = viewModel.Items[0];

        using (viewModel.BindTwoWay(view, x => x.SelectedItem!.Priority, v => v.PriorityBox.SelectedIndex, static priority => (int)priority, static index => (TodoPriority)index))
        {
            Console.WriteLine(view.PriorityBox.SelectedIndex);

            view.PriorityBox.SelectedIndex = 0;

            Console.WriteLine(viewModel.SelectedItem!.Priority);
        }

        // Output:
        // 2
        // Low
    }

    /// <summary>Fills in the transfer form: the amount box is bound both ways, the validation label follows the draft and the button follows the command.</summary>
    /// <returns>A task that completes when the transfer is sent.</returns>
    public static async Task BindTransferFormAsync()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        await viewModel.LoadAsync();
        using IDisposable? confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        using WinFormsTransferForm view = new() { ViewModel = viewModel };
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        using (viewModel.Draft.BindTwoWay(view, x => x.Amount, v => v.AmountTextBox.Text, new DecimalToStringTypeConverter(), new StringToDecimalTypeConverter()))
        using (viewModel.BindOneWay(view, x => x.IsValid, v => v.ValidationLabel.Visible, static isValid => !isValid))
        using (viewModel.BindOneWay(view, x => x.ValidationSummary, v => v.ValidationLabel.Text))
        {
            using (view.BindCommand(viewModel, x => x.TransferCommand, v => v.TransferButton))
            {
                Console.WriteLine(view.AmountTextBox.Text);
                Console.WriteLine(view.ValidationLabel.Visible);
                Console.WriteLine(view.ValidationLabel.Text);
                Console.WriteLine(view.TransferButton.Enabled);

                view.AmountTextBox.Text = UtilityAmountText;

                Console.WriteLine(viewModel.Draft.Amount);
                Console.WriteLine(view.ValidationLabel.Visible);
                Console.WriteLine(view.TransferButton.Enabled);
            }

            await viewModel.TransferAsync();

            Console.WriteLine(viewModel.LastReceipt!.NewBalance);
            Console.WriteLine(view.AmountTextBox.Text);
            Console.WriteLine(view.ValidationLabel.Visible);
        }

        // Output:
        // 0
        // True
        // Enter an amount above zero.
        // False
        // 250.00
        // False
        // True
        // 2200.75
        // 0
        // True
    }

    /// <summary>Binds the to-do screen: the filter box and the checkbox both ways, and the count of items left one way.</summary>
    /// <returns>A task that completes when the items are loaded.</returns>
    public static async Task BindTodoFilterAndCheckBoxAsync()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        using WinFormsTodoForm view = new() { ViewModel = viewModel };
        int allItems = viewModel.Items.Count;
        viewModel.SelectedItem = viewModel.Items[0];

        using (viewModel.BindTwoWay(view, x => x.FilterText, v => v.FilterTextBox.Text))
        using (viewModel.BindTwoWay(view, x => x.SelectedItem!.IsDone, v => v.DoneCheckBox.Checked))
        using (viewModel.BindOneWay(view, x => x.RemainingCount, v => v.RemainingLabel.Text, static count => count.ToString(CultureInfo.InvariantCulture)))
        {
            Console.WriteLine(view.RemainingLabel.Text);
            Console.WriteLine(view.DoneCheckBox.Checked);

            view.FilterTextBox.Text = CarFilter;

            Console.WriteLine(viewModel.FilterText);
            Console.WriteLine(viewModel.Items.Count < allItems);

            view.DoneCheckBox.Checked = true;

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

    /// <summary>Calls the three members of the control invoker on a control that has a handle: who it claims, who may write, and how to queue work on the owning thread.</summary>
    public static void CallInvokerMembers()
    {
        ControlViewThreadInvoker invoker = new();
        using WinFormsUploadForm view = new();
        System.Windows.Forms.ProgressBar? progressBar = view.UploadProgressBar;
        _ = progressBar.Handle;
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

        Application.DoEvents();

        Console.WriteLine(string.Join(", ", log));

        // Output:
        // True
        // False
        // True
        // False
        // 0
        // posted
    }

    /// <summary>Posts to a control that has no handle yet: <c>InvokeRequired</c> is false, so the invoker runs the callback at once.</summary>
    public static void PostToControlWithoutHandleRunsInline()
    {
        ControlViewThreadInvoker invoker = new();
        using WinFormsUploadForm view = new();
        List<string> log = [];
        bool accessFromWorker = false;

        Thread? worker = new Thread(() => accessFromWorker = invoker.CheckAccess(view.UploadProgressBar));
        worker.Start();
        worker.Join();
        invoker.Post(view.UploadProgressBar, static state => ((List<string>)state!).Add(PostedText), log);

        Console.WriteLine(accessFromWorker);
        Console.WriteLine(string.Join(", ", log));

        // Output:
        // True
        // posted
    }

    /// <summary>Rejects a missing callback.</summary>
    public static void RejectMissingCallback()
    {
        ControlViewThreadInvoker invoker = new();
        using WinFormsUploadForm view = new();
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

    /// <summary>Shows that Windows Forms refuses a write from a thread that does not own the control, once cross-thread checks are on. A binding has to avoid this.</summary>
    public static void RefuseWriteFromWorkerThread()
    {
        using WinFormsUploadForm view = new();
        _ = view.UploadProgressBar.Handle;
        bool refused = false;
        Control.CheckForIllegalCrossThreadCalls = true;

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

        // Output:
        // True
    }

    /// <summary>Follows an upload whose progress arrives on a pool thread. No module is registered, so the generated binding carries its own WinForms invoker.</summary>
    /// <returns>A task that completes when the upload is done.</returns>
    public static async Task WriteProgressFromWorkerThreadWithoutModuleAsync()
    {
        using WinFormsUploadForm view = new();

        Console.WriteLine(ViewThreadInvokers.ForTarget(view.UploadProgressBar) is null);

        await RunUploadFromWorkerThreadAsync(view);

        // Output:
        // True
        // 100
    }

    /// <summary>Follows the same upload after <see cref="BuildWinFormsApplication"/>: the registered control invoker claims the bar.</summary>
    /// <returns>A task that completes when the upload is done.</returns>
    public static async Task WriteProgressFromWorkerThreadThroughRegisteredInvokerAsync()
    {
        using WinFormsUploadForm view = new();

        Console.WriteLine(ViewThreadInvokers.ForTarget(view.UploadProgressBar) is ControlViewThreadInvoker);

        await RunUploadFromWorkerThreadAsync(view);

        // Output:
        // True
        // 100
    }

    /// <summary>Uploads a photo on a pool thread; each progress report waits for the owning thread, and the bar shows the last one.</summary>
    /// <param name="view">The form whose bar follows the upload.</param>
    /// <returns>A task that completes when the upload is done.</returns>
    private static async Task RunUploadFromWorkerThreadAsync(WinFormsUploadForm view)
    {
        StorageBrowserViewModel viewModel = new(InMemoryObjectStorage.CreateSeeded());
        await viewModel.LoadBucketsAsync();
        viewModel.SelectedBucket = viewModel.Buckets[0];
        UploadRequest photo = new(OffsitePhotoName, OffsitePhotoSize, JpegType);
        _ = view.UploadProgressBar.Handle;

        using (viewModel.BindOneWay(view, x => x.UploadPercent, v => v.UploadProgressBar.Value, static percent => (int)percent))
        {
            await viewModel.UploadAsync(photo);

            Console.WriteLine(view.UploadProgressBar.Value);
        }
    }

    /// <summary>Reads the priority the picker shows.</summary>
    /// <param name="value">The selected value, or <see langword="null"/> when nothing is selected.</param>
    /// <returns>The priority, or the normal priority when nothing is selected.</returns>
    private static TodoPriority ToPriority(object? value) => value is TodoPriority picked ? picked : TodoPriority.Normal;
}
