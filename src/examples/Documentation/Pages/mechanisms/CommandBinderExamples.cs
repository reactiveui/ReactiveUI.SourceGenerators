// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows.Input;
using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Builder;
using ReactiveUI.Binding.CommandBinding;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Binding.Fallback;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Advanced;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>
/// Shows how a custom <see cref="ICreatesCommandBinding"/> is registered, how it outranks the generated command
/// binding, and how a stream of values drives a command.
/// </summary>
public static class CommandBinderExamples
{
    /// <summary>The title of the item the add button creates.</summary>
    private const string PlumberTitle = "Call the plumber";

    /// <summary>The name of the uploaded screenshot.</summary>
    private const string ScreenshotName = "standup.png";

    /// <summary>The name of the second uploaded screenshot.</summary>
    private const string SecondScreenshotName = "retro.png";

    /// <summary>The name of the third uploaded screenshot.</summary>
    private const string ThirdScreenshotName = "planning.png";

    /// <summary>The media type of the uploaded screenshots.</summary>
    private const string PngType = "image/png";

    /// <summary>The size of an uploaded screenshot in bytes.</summary>
    private const long ScreenshotBytes = 350_000;

    /// <summary>The folder the fixed-command example lists.</summary>
    private const string PhotoFolder = "photos/2026/";

    /// <summary>Binds the add button before any binder is registered; the generated binding hands the command to the button.</summary>
    /// <param name="viewModel">The view model that owns the add command.</param>
    /// <param name="view">The view that owns the add button.</param>
    public static void BindButtonWithoutRegisteredBinder(TodoListViewModel viewModel, TodoView view)
    {
        using (view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton))
        {
            Console.WriteLine($"The button has no command: {view.AddButton.Command is null}");
            Console.WriteLine($"The button holds the view model's command: {ReferenceEquals(viewModel.AddCommand, view.AddButton.Command)}");
        }

        // Output:
        // The button has no command: False
        // The button holds the view model's command: True
    }

    /// <summary>Registers the binder through the builder and asks the service which binder serves each control type.</summary>
    /// <param name="binder">The binder to register.</param>
    public static void RegisterBinderThroughBuilder(ClickCommandBinder binder)
    {
        IReactiveUIBindingBuilder builder = RxBindingBuilder.CreateReactiveUIBindingBuilder();

        _ = builder.WithCoreServices().WithCommandBinder(binder).BuildApp();

        Console.WriteLine($"Binder for a button: {ReferenceEquals(binder, CommandBinderService.GetBinder<Button>(false))}");
        Console.WriteLine($"Binder for a button with an event target: {ReferenceEquals(binder, CommandBinderService.GetBinder<Button>(true))}");
        Console.WriteLine($"No binder for an entry: {CommandBinderService.GetBinder<Entry>(false) is null}");

        // Output:
        // Binder for a button: True
        // Binder for a button with an event target: True
        // No binder for an entry: True
    }

    /// <summary>Compares every affinity score with the binder's bid; the binder outranks the scores below it and loses to the rest.</summary>
    public static void CompareBinderWithGeneratedBinding()
    {
        (string Name, int Score)[] scores =
        [
            (nameof(BindingAffinity.Fallback), BindingAffinity.Fallback),
            (nameof(BindingAffinity.DefaultInternalTypeConverter), BindingAffinity.DefaultInternalTypeConverter),
            (nameof(BindingAffinity.DefaultEvent), BindingAffinity.DefaultEvent),
            (nameof(BindingAffinity.WpfDependencyObject), BindingAffinity.WpfDependencyObject),
            (nameof(BindingAffinity.EventEnabledControl), BindingAffinity.EventEnabledControl),
            (nameof(BindingAffinity.Explicit), BindingAffinity.Explicit),
            (nameof(BindingAffinity.WinUiDependencyObject), BindingAffinity.WinUiDependencyObject),
            (nameof(BindingAffinity.WinFormsEvent), BindingAffinity.WinFormsEvent),
            (nameof(BindingAffinity.ExactType), BindingAffinity.ExactType),
            (nameof(BindingAffinity.Kvo), BindingAffinity.Kvo),
        ];

        foreach (var (name, score) in scores)
        {
            Console.WriteLine($"{name} ({score}): binder outranks it = {CommandBindingAffinityChecker.HasHigherAffinityPlugin<Button>(score, false)}");
        }

        Console.WriteLine($"Binder outranks the fallback for an entry: {CommandBindingAffinityChecker.HasHigherAffinityPlugin<Entry>(BindingAffinity.Fallback, false)}");

        // Output:
        // Fallback (1): binder outranks it = True
        // DefaultInternalTypeConverter (2): binder outranks it = True
        // DefaultEvent (3): binder outranks it = True
        // WpfDependencyObject (4): binder outranks it = True
        // EventEnabledControl (4): binder outranks it = True
        // Explicit (5): binder outranks it = True
        // WinUiDependencyObject (6): binder outranks it = True
        // WinFormsEvent (8): binder outranks it = True
        // ExactType (10): binder outranks it = False
        // Kvo (15): binder outranks it = False
        // Binder outranks the fallback for an entry: False
    }

    /// <summary>Binds the add button; the binder enables it as the command allows and runs the command on each click.</summary>
    /// <param name="viewModel">The view model that owns the add command.</param>
    /// <param name="view">The view that owns the add button.</param>
    /// <returns>A task that completes when the added item is stored.</returns>
    public static async Task BindButtonWithRegisteredBinder(TodoListViewModel viewModel, TodoView view)
    {
        IAwaitSignal<TodoItem> added = viewModel.WhenChanged(x => x.SelectedItem).Where(static item => item is not null).Take(1).GetAwaiter();

        using (view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton))
        {
            Console.WriteLine($"The button has no command: {view.AddButton.Command is null}");
            Console.WriteLine($"Enabled with no title: {view.AddButton.IsEnabled}");

            viewModel.NewTitle = PlumberTitle;

            Console.WriteLine($"Enabled with a title: {view.AddButton.IsEnabled}");

            ((IButtonController)view.AddButton).SendClicked();
            await added;

            Console.WriteLine(viewModel.Items[^1].Title);
        }

        int itemCount = viewModel.Items.Count;

        viewModel.NewTitle = PlumberTitle;
        ((IButtonController)view.AddButton).SendClicked();

        Console.WriteLine($"A click after disposing adds nothing: {viewModel.Items.Count == itemCount}");

        // Output:
        // The click binder is attached to the Add button
        // The button has no command: True
        // Enabled with no title: False
        // Enabled with a title: True
        // Call the plumber
        // A click after disposing adds nothing: True
    }

    /// <summary>Binds the upload button with a stream of parameters; the binder passes the latest one to the command.</summary>
    /// <param name="browser">The browser whose selected bucket receives the upload.</param>
    /// <param name="view">The view that owns the upload button.</param>
    /// <param name="uploads">The stream of files the user picks.</param>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task BindButtonWithParameterStream(StorageBrowserViewModel browser, StorageBrowserView view, Signal<UploadRequest> uploads)
    {
        IAwaitSignal<IReadOnlyList<StorageObject>> stored = browser.WhenChanged(x => x.Objects).Where(static objects => objects.Any(static item => item.Key == ScreenshotName)).Take(1).GetAwaiter();

        using (view.BindCommand(browser, x => x.UploadCommand, v => v.UploadButton, uploads))
        {
            Console.WriteLine($"Enabled with no file: {view.UploadButton.IsEnabled}");

            uploads.OnNext(new(ScreenshotName, ScreenshotBytes, PngType));

            Console.WriteLine($"Enabled with a file: {view.UploadButton.IsEnabled}");

            ((IButtonController)view.UploadButton).SendClicked();
            await stored;
        }

        Console.WriteLine($"Objects: {browser.Objects.Count}");
        Console.WriteLine($"No error: {browser.ErrorMessage.Length == 0}");

        // Output:
        // The click binder is attached to the Upload button
        // Enabled with no file: False
        // Enabled with a file: True
        // Objects: 6
        // No error: True
    }

    /// <summary>Binds the refresh button to a named event; the binder receives the event name.</summary>
    /// <param name="browser">The browser whose command lists the objects.</param>
    /// <param name="view">The view that owns the refresh button.</param>
    /// <returns>A task that completes when the objects are listed.</returns>
    public static async Task BindButtonToNamedEvent(StorageBrowserViewModel browser, StorageBrowserView view)
    {
        browser.CurrentPrefix = PhotoFolder;

        IAwaitSignal<IReadOnlyList<StorageObject>> listed = browser.WhenChanged(x => x.Objects).Skip(1).Take(1).GetAwaiter();

        using (view.BindCommand(browser, x => x.RefreshCommand, v => v.RefreshButton, nameof(Button.Clicked)))
        {
            ((IButtonController)view.RefreshButton).SendClicked();
            await listed;
        }

        Console.WriteLine(browser.Objects.Count);

        // Output:
        // The click binder is asked for the Clicked event, which carries EventArgs
        // The click binder is attached to the Refresh button
        // 2
    }

    /// <summary>Calls the binder for the button's default event, the overload that names no event.</summary>
    /// <param name="browser">The browser whose command lists the objects.</param>
    /// <param name="view">The view that owns the refresh button.</param>
    /// <param name="binder">The registered binder.</param>
    /// <returns>A task that completes when the objects are listed.</returns>
    public static async Task BindWithDefaultEvent(StorageBrowserViewModel browser, StorageBrowserView view, ClickCommandBinder binder)
    {
        browser.CurrentPrefix = string.Empty;

        IAwaitSignal<IReadOnlyList<StorageObject>> listed = browser.WhenChanged(x => x.Objects).Skip(1).Take(1).GetAwaiter();

        using (binder.BindCommandToObject(browser.RefreshCommand, view.RefreshButton, ImmutableEmptySignal<object>.Instance))
        {
            ((IButtonController)view.RefreshButton).SendClicked();
            await listed;
        }

        Console.WriteLine(browser.Objects.Count);

        // Output:
        // The click binder is attached to the Refresh button
        // 6
    }

    /// <summary>Calls the binder with explicit add and remove delegates, the overload that needs no event lookup.</summary>
    /// <param name="browser">The browser whose command reopens the link.</param>
    /// <param name="view">The view that owns the reconnect button.</param>
    /// <param name="storage">The storage service that owns the link.</param>
    /// <param name="binder">The registered binder.</param>
    /// <returns>A task that completes when the link is open again.</returns>
    public static async Task BindWithExplicitEventHandlers(
        StorageBrowserViewModel browser,
        StorageBrowserView view,
        InMemoryObjectStorage storage,
        ClickCommandBinder binder)
    {
        Button button = view.ConnectButton;
        storage.Disconnect();

        IAwaitSignal<ConnectionState> connected = browser.WhenChanged(x => x.ConnectionStatus).Where(static state => state == ConnectionState.Connected).Take(1).GetAwaiter();

        using (binder.BindCommandToObject<Button, EventArgs>(
            browser.ConnectCommand,
            button,
            ImmutableEmptySignal<object>.Instance,
            handler => button.Clicked += handler.Invoke,
            handler => button.Clicked -= handler.Invoke))
        {
            Console.WriteLine($"Enabled while disconnected: {button.IsEnabled}");

            ((IButtonController)button).SendClicked();
            await connected;
        }

        Console.WriteLine($"Connection: {storage.Connection.State}");

        // Output:
        // The click binder is attached to the Reconnect button
        // Enabled while disconnected: True
        // Connection: Connected
    }

    /// <summary>Executes a command with each value a stream produces, whether the stream is a property or an event source.</summary>
    /// <param name="browser">The browser whose command lists the objects.</param>
    /// <returns>A task that completes when the last listing has finished.</returns>
    public static async Task InvokeCommandForEachValue(StorageBrowserViewModel browser)
    {
        browser.CurrentPrefix = string.Empty;

        IAwaitSignal<IReadOnlyList<StorageObject>>? listed = browser.WhenChanged(x => x.Objects).Skip(1).Take(1).GetAwaiter();

        using (CommandInvoker.Invoke(browser.WhenChanged(x => x.CurrentPrefix), browser.RefreshCommand))
        {
            await listed;

            Console.WriteLine($"Objects in the whole bucket: {browser.Objects.Count}");

            listed = browser.WhenChanged(x => x.Objects).Skip(1).Take(1).GetAwaiter();
            browser.CurrentPrefix = PhotoFolder;
            await listed;

            Console.WriteLine($"Objects in the photo folder: {browser.Objects.Count}");
        }

        browser.CurrentPrefix = string.Empty;

        Console.WriteLine($"Objects after disposing: {browser.Objects.Count}");

        // Output:
        // Objects in the whole bucket: 6
        // Objects in the photo folder: 2
        // Objects after disposing: 2
    }

    /// <summary>Executes whichever command the command stream produced last; a value that arrives while there is none is dropped.</summary>
    /// <param name="browser">The browser whose command uploads a file.</param>
    /// <param name="uploads">The stream of files the user picks.</param>
    /// <param name="commands">The stream of commands, which starts without one.</param>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task InvokeLatestCommand(StorageBrowserViewModel browser, Signal<UploadRequest> uploads, Signal<ICommand?> commands)
    {
        browser.CurrentPrefix = string.Empty;
        await browser.LoadObjectsAsync().ConfigureAwait(false);

        int objectCount = browser.Objects.Count;
        IAwaitSignal<IReadOnlyList<StorageObject>> stored = browser.WhenChanged(x => x.Objects)
            .Where(static objects => objects.Any(static item => item.Key == ThirdScreenshotName))
            .Take(1)
            .GetAwaiter();

        using (CommandInvoker.Invoke(uploads, commands))
        {
            commands.OnNext(null);
            uploads.OnNext(new(SecondScreenshotName, ScreenshotBytes, PngType));

            Console.WriteLine($"No command yet, nothing uploaded: {browser.Objects.Count == objectCount}");

            commands.OnNext(browser.UploadCommand);
            uploads.OnNext(new(ThirdScreenshotName, ScreenshotBytes, PngType));
            await stored;
        }

        Console.WriteLine($"One file uploaded: {browser.Objects.Count == objectCount + 1}");
        Console.WriteLine($"The first file was dropped: {!browser.Objects.Any(static item => item.Key == SecondScreenshotName)}");

        // Output:
        // No command yet, nothing uploaded: True
        // One file uploaded: True
        // The first file was dropped: True
    }
}
