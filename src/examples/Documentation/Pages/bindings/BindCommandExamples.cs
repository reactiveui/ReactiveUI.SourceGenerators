// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Todo;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Shows <c>BindCommand</c>: a button runs a view model command, follows its <c>CanExecute</c> and can pass it a parameter.</summary>
public static class BindCommandExamples
{
    /// <summary>The name of the event a MAUI button raises when the user presses it.</summary>
    private const string ClickedEventName = "Clicked";

    /// <summary>The title of the to-do item the add button creates.</summary>
    private const string NewTodoTitle = "Book electrician";

    /// <summary>The amount the customer sends to the utility.</summary>
    private const decimal UtilityAmount = 250M;

    /// <summary>The position of Linear Algebra in the list of courses.</summary>
    private const int MathCourseIndex = 1;

    /// <summary>The position of Diego Alvarez in the list of students who could join Linear Algebra.</summary>
    private const int DiegoCandidateIndex = 2;

    /// <summary>The name of the file the user picks in the upload dialog.</summary>
    private const string SpringCampaignFileName = "spring-campaign.png";

    /// <summary>The size in bytes of the picked file.</summary>
    private const long SpringCampaignSize = 3_145_728;

    /// <summary>The position of the uploaded file among the objects of the photos folder.</summary>
    private const int SpringCampaignPosition = 1;

    /// <summary>Binds the add button of the to-do screen to the view model's add command; the button follows the command's <c>CanExecute</c>.</summary>
    /// <returns>A task that completes when the item is stored.</returns>
    public static async Task BindAddButton()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        TodoView view = new() { ViewModel = viewModel };

        using IDisposable binding = view.BindCommand(viewModel, x => x.AddCommand, v => v.AddButton);

        Console.WriteLine(view.AddButton.IsEnabled);

        viewModel.NewTitle = NewTodoTitle;

        Console.WriteLine(view.AddButton.IsEnabled);

        // The command starts the work and returns, so start listening for the new selection before the click.
        Task<TodoItem> added = viewModel.WhenChanged(x => x.SelectedItem).Where(static item => item is not null).FirstAsync();
        ((IButtonController)view.AddButton).SendClicked();
        await added;

        Console.WriteLine(viewModel.Items.Count);
        Console.WriteLine(viewModel.SelectedItem!.Title);
        Console.WriteLine(view.AddButton.IsEnabled);

        // Output:
        // False
        // True
        // 5
        // Book electrician
        // False
    }

    /// <summary>Binds the complete button through its <c>Clicked</c> event; the binding runs the command on a click and leaves the button's enabled state alone.</summary>
    /// <returns>A task that completes when the item is finished.</returns>
    public static async Task BindCompleteButtonToNamedEvent()
    {
        TodoListViewModel viewModel = new(InMemoryTodoStore.CreateSeeded());
        await viewModel.LoadAsync();
        TodoView view = new() { ViewModel = viewModel };

        using IDisposable binding = view.BindCommand(viewModel, x => x.CompleteCommand, v => v.CompleteButton, toEvent: ClickedEventName);

        // No item is selected, so the command refuses the click.
        ((IButtonController)view.CompleteButton).SendClicked();

        Console.WriteLine(viewModel.RemainingCount);

        TodoItem item = viewModel.Items[0];
        viewModel.SelectedItem = item;
        Task<bool> completed = item.WhenChanged(x => x.IsDone).Where(static done => done).FirstAsync();
        ((IButtonController)view.CompleteButton).SendClicked();
        await completed;

        Console.WriteLine(viewModel.RemainingCount);
        Console.WriteLine(view.CompleteButton.IsEnabled);

        // Output:
        // 3
        // 2
        // True
    }

    /// <summary>
    /// Binds one button to one command twice, with and without <c>toEvent</c>. Each call is its own binding: the plain one
    /// attaches the command to the button and follows its <c>CanExecute</c>, and the event one runs the command on each click.
    /// </summary>
    public static void BindExportButtonWithAndWithoutEvent()
    {
        StatementExportViewModel viewModel = new() { HasStatement = true };
        StatementExportView view = new() { ViewModel = viewModel };

        IDisposable attached = view.BindCommand(viewModel, x => x.ExportCommand, v => v.ExportButton);
        IDisposable clicked = view.BindCommand(viewModel, x => x.ExportCommand, v => v.ExportButton, toEvent: ClickedEventName);

        Console.WriteLine(ReferenceEquals(viewModel.ExportCommand, view.ExportButton.Command));

        // One press reaches the command through both bindings.
        ((IButtonController)view.ExportButton).SendClicked();

        Console.WriteLine(viewModel.ExportCount);

        clicked.Dispose();
        ((IButtonController)view.ExportButton).SendClicked();

        Console.WriteLine(viewModel.ExportCount);

        // Only the plain binding follows CanExecute, so the button goes dark while no statement is chosen.
        viewModel.HasStatement = false;

        Console.WriteLine(view.ExportButton.IsEnabled);

        attached.Dispose();

        // Output:
        // True
        // 2
        // 3
        // False
    }

    /// <summary>Binds the sign-in button; it stays disabled until the user has typed an access token.</summary>
    /// <returns>A task that completes when the sign-in has finished.</returns>
    public static async Task BindSignInButton()
    {
        IssueBoardViewModel viewModel = new(InMemoryGitHubServer.CreateSeeded());
        IssueBoardView view = new() { ViewModel = viewModel };

        using IDisposable binding = view.BindCommand(viewModel, x => x.SignInCommand, v => v.SignInButton);

        Console.WriteLine(view.SignInButton.IsEnabled);

        viewModel.Token = InMemoryGitHubServer.PriyaToken;

        Console.WriteLine(view.SignInButton.IsEnabled);

        Task<IReadOnlyList<Repository>> loaded = viewModel.WhenChanged(x => x.Repositories).Where(static repositories => repositories.Count > 0).FirstAsync();
        ((IButtonController)view.SignInButton).SendClicked();
        await loaded;

        Console.WriteLine(viewModel.CurrentUser!.Login);
        Console.WriteLine(view.SignInButton.IsEnabled);

        // Output:
        // False
        // True
        // priya-nair
        // False
    }

    /// <summary>Binds the close-issue button; the view model asks a confirmation handler before it closes the issue.</summary>
    /// <returns>A task that completes when the issue is closed.</returns>
    public static async Task BindCloseIssueButton()
    {
        IssueBoardViewModel viewModel = await OpenWebshopIssuesAsync();
        IssueBoardView view = new() { ViewModel = viewModel };
        using IDisposable confirmation = viewModel.ConfirmClose.RegisterHandler(static context => context.SetOutput(true));

        using IDisposable binding = view.BindCommand(viewModel, x => x.CloseIssueCommand, v => v.CloseIssueButton);

        Console.WriteLine(view.CloseIssueButton.IsEnabled);

        Issue issue = viewModel.Issues[0];
        viewModel.SelectedIssue = issue;

        Console.WriteLine(view.CloseIssueButton.IsEnabled);

        Task<int> requestFinished = viewModel.WhenChanged(x => x.RateLimitRemaining).Skip(1).FirstAsync();
        ((IButtonController)view.CloseIssueButton).SendClicked();
        await requestFinished;

        Console.WriteLine(issue.State);
        Console.WriteLine(view.CloseIssueButton.IsEnabled);

        // Output:
        // False
        // True
        // Closed
        // False
    }

    /// <summary>Binds the transfer button; it is disabled until the draft breaks none of the view model's rules.</summary>
    /// <returns>A task that completes when the transfer has been sent.</returns>
    public static async Task BindTransferButton()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());
        await viewModel.LoadAsync();
        using IDisposable confirmation = viewModel.ConfirmTransfer.RegisterHandler(static context => context.SetOutput(true));
        TransferView view = new() { ViewModel = viewModel };

        using IDisposable binding = view.BindCommand(viewModel, x => x.TransferCommand, v => v.TransferButton);

        Console.WriteLine(view.TransferButton.IsEnabled);

        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];

        Console.WriteLine(view.TransferButton.IsEnabled);

        viewModel.Draft.Amount = UtilityAmount;

        Console.WriteLine(view.TransferButton.IsEnabled);

        // The transfer resets the amount once the bank has answered.
        Task<decimal> sent = viewModel.Draft.WhenChanged(x => x.Amount).Where(static amount => amount == 0M).FirstAsync();
        ((IButtonController)view.TransferButton).SendClicked();
        await sent;

        Console.WriteLine(viewModel.LastReceipt!.NewBalance);
        Console.WriteLine(view.TransferButton.IsEnabled);

        // Output:
        // False
        // False
        // True
        // 2200.75
        // False
    }

    /// <summary>Binds the enrol button with an observable command parameter: the student selected in the candidate list.</summary>
    /// <returns>A task that completes when the student is on the roster.</returns>
    public static async Task BindEnrolButtonWithObservableParameter()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded());
        await viewModel.LoadCoursesAsync();
        viewModel.SelectedCourse = viewModel.Courses[MathCourseIndex];
        await viewModel.OpenCourseAsync();
        GradebookView view = new() { ViewModel = viewModel };
        view.CandidateList.ItemsSource = viewModel.Candidates.ToList();

        using IDisposable binding = view.BindCommand(viewModel, x => x.EnrolCommand, v => v.EnrolButton, view.CandidateList.WhenChanged(x => x.SelectedItem));

        Console.WriteLine(view.EnrolButton.IsEnabled);

        view.CandidateList.SelectedItem = viewModel.Candidates[DiegoCandidateIndex];

        Console.WriteLine(view.EnrolButton.IsEnabled);

        Task<IReadOnlyList<Student>> rosterLoaded = viewModel.WhenChanged(x => x.Roster).Skip(1).FirstAsync();
        ((IButtonController)view.EnrolButton).SendClicked();
        await rosterLoaded;

        Console.WriteLine(viewModel.Roster.Count);

        // Output:
        // False
        // True
        // 3
    }

    /// <summary>Binds the upload button of the browser through its <c>Clicked</c> event with an observable command parameter: each file the user picks.</summary>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task BindUploadButtonWithObservableParameterOnNamedEvent()
    {
        StorageBrowserViewModel browser = await OpenPhotosFolderAsync();
        StorageBrowserView view = new() { ViewModel = browser };
        Signal<UploadRequest> pickedFiles = new();

        using IDisposable binding = view.BindCommand(browser, x => x.UploadCommand, v => v.UploadButton, pickedFiles, toEvent: ClickedEventName);

        pickedFiles.OnNext(SpringCampaign());
        Task<bool> uploaded = browser.WhenChanged(x => x.IsUploading).Skip(1).Where(static uploading => !uploading).FirstAsync();
        ((IButtonController)view.UploadButton).SendClicked();
        await uploaded;

        Console.WriteLine(browser.Objects.Count);

        // Output:
        // 3
    }

    /// <summary>Binds the upload button through its <c>Clicked</c> event with a parameter expression.</summary>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task BindUploadButtonWithParameterExpressionOnNamedEvent()
    {
        StorageBrowserViewModel browser = await OpenPhotosFolderAsync();
        UploadPanelViewModel viewModel = new(browser);
        UploadPanelView view = new() { ViewModel = viewModel };

        using IDisposable binding = view.BindCommand(viewModel, panel => panel.UploadCommand, v => v.UploadButton, panel => panel.PendingUpload, toEvent: ClickedEventName);

        viewModel.PendingUpload = SpringCampaign();
        Task<bool> uploaded = browser.WhenChanged(x => x.IsUploading).Skip(1).Where(static uploading => !uploading).FirstAsync();
        ((IButtonController)view.UploadButton).SendClicked();
        await uploaded;

        Console.WriteLine(browser.Objects.Count);

        // Output:
        // 3
    }

    /// <summary>Binds the upload button with a parameter expression: the file the user picked, read from the view model.</summary>
    /// <returns>A task that completes when the file is stored.</returns>
    public static async Task BindUploadButtonWithParameterExpression()
    {
        StorageBrowserViewModel browser = await OpenPhotosFolderAsync();
        UploadPanelViewModel viewModel = new(browser);
        UploadPanelView view = new() { ViewModel = viewModel };

        using IDisposable binding = view.BindCommand(viewModel, x => x.UploadCommand, v => v.UploadButton, x => x.PendingUpload);

        Console.WriteLine(view.UploadButton.IsEnabled);

        viewModel.PendingUpload = SpringCampaign();

        Console.WriteLine(view.UploadButton.IsEnabled);

        Task<bool> uploaded = browser.WhenChanged(x => x.IsUploading).Skip(1).Where(static uploading => !uploading).FirstAsync();
        ((IButtonController)view.UploadButton).SendClicked();
        await uploaded;

        Console.WriteLine(browser.Objects.Count);
        Console.WriteLine(browser.Objects[SpringCampaignPosition].Key);

        // Output:
        // False
        // True
        // 3
        // photos/2026/spring-campaign.png
    }

    /// <summary>Creates the file the user picks in the upload dialog.</summary>
    /// <returns>The upload request.</returns>
    private static UploadRequest SpringCampaign() => new(SpringCampaignFileName, SpringCampaignSize, "image/png");

    /// <summary>Signs in as Priya Nair and loads the open issues of the webshop repository.</summary>
    /// <returns>A task that completes with the view model once the issues are loaded.</returns>
    private static async Task<IssueBoardViewModel> OpenWebshopIssuesAsync()
    {
        IssueBoardViewModel viewModel = new(InMemoryGitHubServer.CreateSeeded()) { Token = InMemoryGitHubServer.PriyaToken };

        await viewModel.SignInAsync();
        viewModel.SelectedRepository = viewModel.Repositories[0];
        await viewModel.LoadIssuesAsync();
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
