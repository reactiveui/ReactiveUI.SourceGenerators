// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows <c>InvokeCommand</c> driven by streams: a token, a selection, a timer, a trigger and a scroll, none of them a button.</summary>
public static class InvokeCommandExamples
{
    /// <summary>The seconds between two refreshes of the to-do list.</summary>
    private const int RefreshSeconds = 30;

    /// <summary>The time a refresh takes to answer, long enough for the ticks that follow to arrive first.</summary>
    private const int SlowRefreshMilliseconds = 100;

    /// <summary>The amount of the transfer the customer sends.</summary>
    private const decimal TransferAmount = 50M;

    /// <summary>The title of an item another device adds to the to-do list.</summary>
    private const string RatesTitle = "Pay council rates";

    /// <summary>The title of an item another device adds after the refresh stops.</summary>
    private const string PassportTitle = "Renew passport";

    /// <summary>The name of the file the user picks in the upload dialog.</summary>
    private const string SpringCampaignFileName = "spring-campaign.png";

    /// <summary>The size in bytes of the picked file.</summary>
    private const long SpringCampaignSize = 3_145_728;

    /// <summary>The number of objects each page of the object list holds.</summary>
    private const int PageSize = 2;

    /// <summary>The name of the bucket whose objects the paging example lists.</summary>
    private const string MediaBucketName = "acme-media";

    /// <summary>Executes the sign-in command each time the access token changes; a blank token is refused by the command and dropped.</summary>
    /// <returns>A task that completes when the sign-in has finished.</returns>
    public static async Task InvokeSignInWhenTokenChanges()
    {
        IssueBoardViewModel viewModel = new(InMemoryGitHubServer.CreateSeeded());

        using IDisposable subscription = viewModel.WhenChanged(x => x.Token).InvokeCommand(viewModel.SignInCommand);

        viewModel.Token = "   ";

        Console.WriteLine(viewModel.IsSignedIn);
        Console.WriteLine(viewModel.RateLimitRemaining);

        Task<IReadOnlyList<Repository>> loaded = viewModel.WhenChanged(x => x.Repositories).Where(static repositories => repositories.Count > 0).FirstAsync();
        viewModel.Token = InMemoryGitHubServer.PriyaToken;
        await loaded;

        Console.WriteLine(viewModel.IsSignedIn);

        // Output:
        // False
        // 60
        // True
    }

    /// <summary>Executes the browser's upload command with each file the user picks, so the picked file reaches the command as its parameter.</summary>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task InvokeUploadWithPickedFile()
    {
        StorageBrowserViewModel browser = await OpenPhotosFolderAsync();
        UploadPanelViewModel viewModel = new(browser);

        using IDisposable subscription = viewModel.WhenChanged(x => x.PendingUpload!).InvokeCommand(browser.UploadCommand);

        Console.WriteLine(browser.Objects.Count);

        Task<bool> uploaded = browser.WhenChanged(x => x.IsUploading).Skip(1).Where(static uploading => !uploading).FirstAsync();
        viewModel.PendingUpload = new(SpringCampaignFileName, SpringCampaignSize, "image/png");
        await uploaded;

        Console.WriteLine(browser.Objects.Count);

        // Output:
        // 2
        // 3
    }

    /// <summary>Executes the command a property holds each time the selected repository changes; the property is read from the view model.</summary>
    /// <returns>A task that completes when the issues are loaded.</returns>
    public static async Task InvokeLoadIssuesWhenRepositoryChanges()
    {
        IssueBoardViewModel viewModel = await SignInAsync();

        using IDisposable subscription = viewModel.WhenChanged(x => x.SelectedRepository!).InvokeCommand(viewModel, x => x.LoadIssuesCommand);

        Console.WriteLine(viewModel.Issues.Count);

        Task<int> loaded = viewModel.WhenChanged(x => x.RateLimitRemaining).Skip(1).FirstAsync();
        viewModel.SelectedRepository = viewModel.Repositories[0];
        await loaded;

        Console.WriteLine(viewModel.Issues.Count);

        // Output:
        // 0
        // 2
    }

    /// <summary>Loads the issues of a repository each time the user selects one.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task LoadIssuesWhenRepositoryIsSelected()
    {
        IssueBoardViewModel viewModel = await SignInAsync();
        Repository webshop = viewModel.Repositories[0];
        Repository payments = viewModel.Repositories[1];

        // The stream starts with the current selection, which is null. The command refuses it, so nothing loads yet.
        using (viewModel.WhenChanged(x => x.SelectedRepository!).InvokeCommand(viewModel.LoadIssuesCommand))
        {
            Console.WriteLine(viewModel.Issues.Count);

            Task<int>? loaded = viewModel.WhenChanged(x => x.RateLimitRemaining).Skip(1).FirstAsync();
            viewModel.SelectedRepository = webshop;
            await loaded;

            Console.WriteLine(string.Join(", ", viewModel.Issues.Select(static issue => issue.Title)));

            loaded = viewModel.WhenChanged(x => x.RateLimitRemaining).Skip(1).FirstAsync();
            viewModel.SelectedRepository = payments;
            await loaded;

            Console.WriteLine(string.Join(", ", viewModel.Issues.Select(static issue => issue.Title)));
        }

        // Output:
        // 0
        // Add gift-card support, Checkout button unresponsive on Safari
        // Document the refund endpoint, Card gateway times out under load
    }

    /// <summary>Opens the gradebook of a course when the user picks it from the course list, and drops the pick when the view clears it.</summary>
    /// <returns>A task that completes when the roster is loaded.</returns>
    public static async Task OpenCourseWhenTheUserPicksIt()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded());
        await viewModel.LoadCoursesAsync();
        GradebookView view = new() { ViewModel = viewModel };
        view.CourseList.ItemsSource = viewModel.Courses;

        using (view.Bind(viewModel, x => x.SelectedCourse, v => v.CourseList.SelectedItem, static course => course, static selected => (selected as Course)!))
        using (viewModel.WhenChanged(x => x.SelectedCourse).Where(static course => course is not null).InvokeCommand(viewModel.OpenCourseCommand))
        {
            Console.WriteLine(viewModel.Roster.Count);

            Task<IReadOnlyList<Student>> opened = viewModel.WhenChanged(x => x.Roster).Skip(1).FirstAsync();
            view.CourseList.SelectedItem = viewModel.Courses[0];
            await opened;

            Console.WriteLine(viewModel.SelectedCourse!.Title);
            Console.WriteLine(viewModel.Roster.Count);

            // The view clears the selection, and the two-way binding carries the null back to the view model.
            view.CourseList.SelectedItem = null;

            Console.WriteLine(viewModel.SelectedCourse is null);
        }

        // Output:
        // 0
        // Introduction to Programming
        // 3
        // True
    }

    /// <summary>Asks for the next page of objects when the last row of the list appears.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task LoadNextPageWhenTheLastRowAppears()
    {
        InMemoryObjectStorage storage = InMemoryObjectStorage.CreateSeeded();
        IReadOnlyList<StorageObject> objects = await storage.ListObjectsAsync(MediaBucketName, string.Empty);
        List<StorageObject> shown = [];
        Command<int> loadPage = new(offset => shown.AddRange(objects.Skip(offset).Take(PageSize)), offset => offset < objects.Count);
        using Signal<int> rowAppeared = new();

        using (rowAppeared.Where(row => row == shown.Count - 1).Select(_ => shown.Count).InvokeCommand(loadPage))
        {
            // The list shows its first page before any row is scrolled into view.
            loadPage.Execute(0);
            Console.WriteLine(shown.Count);

            // A row in the middle of the page asks for nothing.
            rowAppeared.OnNext(0);
            Console.WriteLine(shown.Count);

            // The last visible row asks for the next page.
            rowAppeared.OnNext(1);
            Console.WriteLine(shown.Count);

            rowAppeared.OnNext(shown.Count - 1);
            Console.WriteLine(shown.Count);

            // The last page has no successor, so the command refuses the request.
            rowAppeared.OnNext(shown.Count - 1);
            Console.WriteLine(shown.Count);
        }

        // Output:
        // 2
        // 2
        // 4
        // 5
        // 5
    }

    /// <summary>Refreshes the to-do list on a fixed interval, with the interval driven by a virtual clock.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task RefreshTodoItemsOnAnInterval()
    {
        InMemoryTodoStore store = InMemoryTodoStore.CreateSeeded();
        TodoListViewModel viewModel = new(store);
        VirtualClock clock = new();
        TimeSpan interval = TimeSpan.FromSeconds(RefreshSeconds);

        using (Signal.Every(interval, clock).InvokeCommand(viewModel, x => x.LoadCommand))
        {
            Console.WriteLine(viewModel.Items.Count);

            Task<IReadOnlyList<TodoItem>>? refreshed = viewModel.WhenChanged(x => x.Items).Skip(1).FirstAsync();
            clock.AdvanceBy(interval);
            await refreshed;

            Console.WriteLine(viewModel.Items.Count);

            _ = await store.AddAsync(new TodoItem { Title = RatesTitle });

            Console.WriteLine(viewModel.Items.Count);

            refreshed = viewModel.WhenChanged(x => x.Items).Skip(1).FirstAsync();
            clock.AdvanceBy(interval);
            await refreshed;

            Console.WriteLine(viewModel.Items.Count);
        }

        // Disposing the subscription stops the timer from reaching the command.
        _ = await store.AddAsync(new TodoItem { Title = PassportTitle });
        clock.AdvanceBy(interval);

        Console.WriteLine(viewModel.Items.Count);

        // Output:
        // 0
        // 4
        // 4
        // 5
        // 5
    }

    /// <summary>Drops the ticks that arrive while the previous refresh is still running, because the command reports it cannot execute.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task SkipTicksWhileTheCommandIsRunning()
    {
        InMemoryTodoStore store = new() { Latency = TimeSpan.FromMilliseconds(SlowRefreshMilliseconds) };
        TodoListViewModel viewModel = new(store);
        VirtualClock clock = new();
        TimeSpan interval = TimeSpan.FromSeconds(RefreshSeconds);
        Task? refresh = null;
        int started = 0;
        Command refreshCommand = new(
            () =>
            {
                started++;
                refresh = viewModel.LoadAsync();
            },
            () => refresh is not { IsCompleted: false });

        using (Signal.Every(interval, clock).InvokeCommand(refreshCommand))
        {
            clock.AdvanceBy(interval);

            Console.WriteLine(started);

            // Two more ticks while the first refresh waits for the database: neither starts a request.
            clock.AdvanceBy(interval);
            clock.AdvanceBy(interval);

            Console.WriteLine(started);

            await refresh!;

            // The next tick after the refresh has finished runs the command again.
            clock.AdvanceBy(interval);

            Console.WriteLine(started);

            await refresh;
        }

        // Output:
        // 1
        // 1
        // 2
    }

    /// <summary>Drops the values that arrive while the command is disabled, and runs the command once the draft is valid.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task SkipValuesWhileTheCommandIsDisabled()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        await viewModel.LoadAsync();

        int confirmations = 0;
        using IDisposable handler = viewModel.ConfirmTransfer.RegisterHandler(context =>
        {
            confirmations++;
            context.SetOutput(true);
        });

        using Signal<RxVoid> submit = new();

        using (submit.InvokeCommand(viewModel.TransferCommand))
        {
            // The draft has no account, payee or amount, so the command cannot execute.
            Console.WriteLine(viewModel.IsValid);

            submit.OnNext(RxVoid.Default);

            Console.WriteLine(confirmations);
            Console.WriteLine(viewModel.LastReceipt is null);

            viewModel.Draft.Source = viewModel.Accounts[0];
            viewModel.Draft.Payee = viewModel.Payees[0];
            viewModel.Draft.Amount = TransferAmount;

            Console.WriteLine(viewModel.IsValid);

            // The transfer resets the amount once the bank has answered.
            Task<decimal> sent = viewModel.Draft.WhenChanged(x => x.Amount).Where(static amount => amount == 0M).FirstAsync();
            submit.OnNext(RxVoid.Default);
            await sent;

            Console.WriteLine(confirmations);
            Console.WriteLine(viewModel.LastReceipt!.NewBalance);
        }

        // Output:
        // False
        // 0
        // True
        // True
        // 1
        // 2400.75
    }

    /// <summary>Runs a command once at start-up from a single-value stream, as a view-model unit test does.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task LoadOnceAtStartUp()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        Task<IReadOnlyList<TodoItem>> loaded = viewModel.WhenChanged(x => x.Items).Skip(1).FirstAsync();

        using (Signal.Emit(RxVoid.Default).InvokeCommand(viewModel.LoadCommand))
        {
            await loaded;

            Console.WriteLine(viewModel.Items.Count);
            Console.WriteLine(viewModel.RemainingCount);
        }

        // Output:
        // 4
        // 3
    }

    /// <summary>Signs in as Priya Nair.</summary>
    /// <returns>A task that completes with the view model once the repositories are loaded.</returns>
    private static async Task<IssueBoardViewModel> SignInAsync()
    {
        IssueBoardViewModel viewModel = new(InMemoryGitHubServer.CreateSeeded()) { Token = InMemoryGitHubServer.PriyaToken };

        await viewModel.SignInAsync();
        return viewModel;
    }

    /// <summary>Opens the 2026 photos folder of the media bucket.</summary>
    /// <returns>A task that completes with the browser once the folder is listed.</returns>
    private static async Task<StorageBrowserViewModel> OpenPhotosFolderAsync()
    {
        StorageBrowserViewModel browser = new(InMemoryObjectStorage.CreateSeeded());

        await browser.LoadBucketsAsync();
        browser.SelectedBucket = browser.Buckets[0];
        browser.CurrentPrefix = "photos/2026/";
        await browser.LoadObjectsAsync();
        return browser;
    }
}
