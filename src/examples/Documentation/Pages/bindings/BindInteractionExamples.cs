// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Primitives.Concurrency;
using ReactiveUI.Primitives.Signals;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>Demonstrates <c>BindInteraction</c> and the interaction types a view model and a view share.</summary>
public static class BindInteractionExamples
{
    /// <summary>The amount of a transfer large enough to need an approval code.</summary>
    private const decimal LargeTransferAmount = 1200M;

    /// <summary>The position of Ben Carter on the Introduction to Programming roster.</summary>
    private const int BenRosterIndex = 1;

    /// <summary>The number of the issue the interaction examples ask about.</summary>
    private const int CheckoutIssueNumber = 101;

    /// <summary>The message of an exception a host raises when nobody answers close confirmations.</summary>
    private const string NobodyAnswersMessage = "Nobody answers close confirmations.";

    /// <summary>The answer of the handler registered first.</summary>
    private const string FirstAnswer = "first";

    /// <summary>The answer of the handler registered last.</summary>
    private const string LatestAnswer = "latest";

    /// <summary>Binds before the view has a view model and moves the handler when the selected board changes.</summary>
    /// <returns>A task that completes when both boards have answered.</returns>
    public static async Task FollowTheSelectedIssueBoard()
    {
        IssueBoardView view = new();
        using IDisposable binding = view.BindInteraction(view.ViewModel, x => x.ConfirmClose, static context =>
        {
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        Issue issue = new() { Number = CheckoutIssueNumber };
        IssueBoardViewModel first = new(InMemoryGitHubServer.CreateSeeded());
        view.ViewModel = first;
        Console.WriteLine(await first.ConfirmClose.Handle(issue));

        IssueBoardViewModel second = new(InMemoryGitHubServer.CreateSeeded());
        view.ViewModel = second;
        Console.WriteLine(await second.ConfirmClose.Handle(issue));

        try
        {
            _ = await first.ConfirmClose.Handle(issue);
        }
        catch (UnhandledInteractionException<Issue, bool>)
        {
            Console.WriteLine("The previous board has no handler.");
        }

        // Output:
        // True
        // True
        // The previous board has no handler.
    }

    /// <summary>Answers the close confirmation through a task-based handler that waits for the user, so the close stays pending until the dialog is answered.</summary>
    /// <returns>A task that completes when the issue is closed.</returns>
    public static async Task ConfirmCloseWithTaskHandler()
    {
        IssueBoardViewModel viewModel = await OpenWebshopIssuesAsync();
        IssueBoardView view = new() { ViewModel = viewModel };
        TaskCompletionSource<bool> dialog = new(TaskCreationOptions.RunContinuationsAsynchronously);

        using IDisposable binding = view.BindInteraction(viewModel, x => x.ConfirmClose, async context => context.SetOutput(await dialog.Task));

        Issue issue = viewModel.Issues[0];
        viewModel.SelectedIssue = issue;
        Task closing = viewModel.CloseIssueAsync();

        Console.WriteLine(closing.IsCompleted);
        Console.WriteLine(issue.State);

        dialog.SetResult(true);
        await closing;

        Console.WriteLine(issue.State);

        // Output:
        // False
        // Open
        // Closed
    }

    /// <summary>Answers the drop confirmation through an observable-based handler; the handler's sequence completes once the answer is set.</summary>
    /// <returns>A task that completes when the second attempt has dropped the student.</returns>
    public static async Task ConfirmDropWithObservableHandler()
    {
        GradebookViewModel viewModel = new(InMemoryStudentRecords.CreateSeeded());
        await viewModel.LoadCoursesAsync();
        viewModel.SelectedCourse = viewModel.Courses[0];
        await viewModel.OpenCourseAsync();
        viewModel.SelectedStudent = viewModel.Roster[BenRosterIndex];
        GradebookView view = new() { ViewModel = viewModel };
        bool teacherConfirms = false;

        using IDisposable binding = view.BindInteraction(viewModel, x => x.ConfirmDrop, context =>
        {
            context.SetOutput(teacherConfirms);
            return Signal.Return(context.Input.Student.Id);
        });

        await viewModel.DropAsync();

        Console.WriteLine(viewModel.Roster.Count);

        teacherConfirms = true;
        await viewModel.DropAsync();

        Console.WriteLine(viewModel.Roster.Count);

        // Output:
        // 3
        // 2
    }

    /// <summary>Binds an interaction that the view model exposes as <see cref="IInteraction{TInput, TOutput}"/>.</summary>
    /// <returns>A task that completes when the issue is closed.</returns>
    public static async Task ConfirmCloseThroughInterfaceProperty()
    {
        IssueBoardViewModel board = await OpenWebshopIssuesAsync();
        IssueTriageViewModel viewModel = new(board);
        IssueTriageView view = new() { ViewModel = viewModel };

        using IDisposable binding = view.BindInteraction(viewModel, x => x.ConfirmClose, static context =>
        {
            context.SetOutput(context.Input.State == IssueState.Open);
            return Task.CompletedTask;
        });

        Issue issue = board.Issues[0];
        board.SelectedIssue = issue;
        await board.CloseIssueAsync();

        Console.WriteLine(issue.State);

        // Output:
        // Closed
    }

    /// <summary>Approves a large transfer in two steps: the view confirms the draft, and then the bank asks the view for a one-time code.</summary>
    /// <returns>A task that completes when the transfer is sent.</returns>
    public static async Task ApproveLargeTransferInTwoSteps()
    {
        var (viewModel, view) = await OpenTransferScreenAsync();

        using IDisposable confirmation = view.BindInteraction(viewModel, x => x.ConfirmTransfer, static context =>
        {
            Console.WriteLine("confirm");
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        using IDisposable approval = view.BindInteraction(viewModel, x => x.ApproveTransfer, static context =>
        {
            Console.WriteLine("approve");
            context.SetOutput(InMemoryBankingBackend.ApprovalCode);
            return Task.CompletedTask;
        });

        FillLargeTransfer(viewModel);
        await viewModel.TransferAsync();

        Console.WriteLine(viewModel.LastReceipt!.Amount);

        // Output:
        // confirm
        // approve
        // 1200
    }

    /// <summary>Cancels a large transfer at the second step: an empty approval code tells the view model the customer gave up.</summary>
    /// <returns>A task that completes when the view model has reported the cancellation.</returns>
    public static async Task CancelTransferAtApprovalStep()
    {
        var (viewModel, view) = await OpenTransferScreenAsync();

        using IDisposable confirmation = view.BindInteraction(viewModel, x => x.ConfirmTransfer, static context =>
        {
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        using IDisposable approval = view.BindInteraction(viewModel, x => x.ApproveTransfer, static context =>
        {
            context.SetOutput(string.Empty);
            return Task.CompletedTask;
        });

        FillLargeTransfer(viewModel);
        await viewModel.TransferAsync();

        Console.WriteLine(viewModel.ErrorMessage);
        Console.WriteLine(viewModel.LastReceipt is null);

        // Output:
        // The transfer was cancelled.
        // True
    }

    /// <summary>Registers the three kinds of handler on an interaction; the handler registered last answers first, and disposing a registration removes it.</summary>
    /// <returns>A task that completes when the last answer has been read.</returns>
    public static async Task RegisterHandlersInReverseOrder()
    {
        Interaction<TransferDraft, string> approval = new();
        TransferDraft draft = new();
        using IDisposable first = approval.RegisterHandler(static context => context.SetOutput(FirstAnswer));
        using IDisposable ignoring = approval.RegisterHandler(static context => Signal.Return(context.IsHandled));
        IDisposable latest = approval.RegisterHandler(static context =>
        {
            context.SetOutput(LatestAnswer);
            return Task.CompletedTask;
        });

        Console.WriteLine(await approval.Handle(draft));

        latest.Dispose();

        Console.WriteLine(await approval.Handle(draft));

        // Output:
        // latest
        // first
    }

    /// <summary>Reads what a handler receives: the input, whether the interaction is handled, and the output once it is set.</summary>
    /// <returns>A task that completes when the handler has run.</returns>
    public static async Task InspectInteractionContext()
    {
        Interaction<Issue, bool> confirmClose = new();
        Issue issue = new() { Number = CheckoutIssueNumber };
        Issue? receivedInput = null;
        bool handledBefore = true;
        bool handledAfter = false;
        bool isInteractionContext = false;
        bool earlyReadRefused = false;
        bool readBack = false;
        bool secondAnswerRefused = false;

        using IDisposable registration = confirmClose.RegisterHandler(context =>
        {
            receivedInput = context.Input;
            handledBefore = context.IsHandled;
            isInteractionContext = context is InteractionContext<Issue, bool>;

            IOutputContext<Issue, bool> output = (IOutputContext<Issue, bool>)context;
            earlyReadRefused = RefusesWithInvalidOperation(() => output.GetOutput());

            context.SetOutput(true);
            handledAfter = context.IsHandled;
            readBack = output.GetOutput();
            secondAnswerRefused = RefusesWithInvalidOperation(() => context.SetOutput(false));
        });

        bool answer = await confirmClose.Handle(issue);

        Console.WriteLine(ReferenceEquals(issue, receivedInput));
        Console.WriteLine(handledBefore);
        Console.WriteLine(handledAfter);
        Console.WriteLine(isInteractionContext);
        Console.WriteLine(earlyReadRefused);
        Console.WriteLine(readBack);
        Console.WriteLine(secondAnswerRefused);
        Console.WriteLine(answer);

        // Output:
        // True
        // False
        // True
        // True
        // True
        // True
        // True
        // True
    }

    /// <summary>Casts the context a handler receives to <see cref="InteractionContext{TInput, TOutput}"/>, the class that holds the input and the answer.</summary>
    /// <returns>A task that completes when the answer has been read.</returns>
    public static async Task AnswerThroughInteractionContext()
    {
        Interaction<Issue, bool> confirmClose = new();
        Issue issue = new() { Number = CheckoutIssueNumber };

        using IDisposable registration = confirmClose.RegisterHandler(static context =>
        {
            InteractionContext<Issue, bool> answer = (InteractionContext<Issue, bool>)context;

            Console.WriteLine(answer.Input.Number);
            Console.WriteLine(answer.IsHandled);

            answer.SetOutput(true);

            Console.WriteLine(answer.IsHandled);
            Console.WriteLine(answer.GetOutput());
        });

        Console.WriteLine(await confirmClose.Handle(issue));

        // Output:
        // 101
        // False
        // True
        // True
        // True
    }

    /// <summary>Closes an issue while no view has bound a handler; the close faults with <see cref="UnhandledInteractionException{TInput, TOutput}"/>.</summary>
    /// <returns>A task that completes when the failure has been read.</returns>
    public static async Task CloseIssueWithoutHandler()
    {
        IssueBoardViewModel viewModel = await OpenWebshopIssuesAsync();
        Issue issue = viewModel.Issues[0];
        viewModel.SelectedIssue = issue;
        UnhandledInteractionException<Issue, bool>? failure = null;

        try
        {
            await viewModel.CloseIssueAsync();
        }
        catch (UnhandledInteractionException<Issue, bool> ex)
        {
            failure = ex;
        }

        Console.WriteLine(failure is not null);
        Console.WriteLine(failure!.Message);
        Console.WriteLine(ReferenceEquals(issue, failure.Input));
        Console.WriteLine(ReferenceEquals(viewModel.ConfirmClose, failure.Interaction));
        Console.WriteLine(issue.State);

        // Output:
        // True
        // Failed to find a registration for an Interaction.
        // True
        // True
        // Open
    }

    /// <summary>
    /// Derives from <see cref="Interaction{TInput, TOutput}"/> to keep an audit trail: the subclass overrides <c>Handle</c>,
    /// reads the registered handlers with <c>GetHandlers</c> and wraps the context that <c>GenerateContext</c> creates.
    /// </summary>
    /// <returns>A task that completes when the approval has been given.</returns>
    public static async Task AuditApprovalWithDerivedInteraction()
    {
        AuditedInteraction<decimal, string> approval = new();
        using IDisposable issuer = approval.RegisterHandler(static context => context.SetOutput(InMemoryBankingBackend.ApprovalCode));
        using IDisposable observer = approval.RegisterHandler(static context => Signal.Return(context.IsHandled));

        string code = await approval.Handle(LargeTransferAmount);

        Console.WriteLine(code);

        // Output:
        // ask 1200 of 2 handlers
        // answer 482913
        // 482913
    }

    /// <summary>Stops handling once the binding is disposed: the next question goes unanswered.</summary>
    /// <returns>A task that completes when the second question has failed.</returns>
    public static async Task DisposeBindingRemovesHandler()
    {
        IssueBoardViewModel viewModel = await OpenWebshopIssuesAsync();
        IssueBoardView view = new() { ViewModel = viewModel };
        Issue issue = viewModel.Issues[0];
        IDisposable binding = view.BindInteraction(viewModel, x => x.ConfirmClose, static context =>
        {
            context.SetOutput(false);
            return Task.CompletedTask;
        });

        Console.WriteLine(await viewModel.ConfirmClose.Handle(issue));

        binding.Dispose();

        bool unanswered = false;
        try
        {
            _ = await viewModel.ConfirmClose.Handle(issue);
        }
        catch (UnhandledInteractionException<Issue, bool>)
        {
            unanswered = true;
        }

        Console.WriteLine(unanswered);

        // Output:
        // False
        // True
    }

    /// <summary>Binds while the view has no view model yet: nothing is registered, nothing throws, and disposing the binding is safe.</summary>
    public static void BindInteractionWithoutViewModel()
    {
        IssueBoardView view = new();
        IssueBoardViewModel? viewModel = null;
        int handlerRuns = 0;

        IDisposable binding = view.BindInteraction(viewModel, x => x.ConfirmClose, context =>
        {
            handlerRuns++;
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        binding.Dispose();

        Console.WriteLine(handlerRuns);

        // Output:
        // 0
    }

    /// <summary>Creates <see cref="UnhandledInteractionException{TInput, TOutput}"/> with each constructor it offers.</summary>
    public static void ConstructUnhandledInteractionException()
    {
        Interaction<Issue, bool> confirmClose = new();
        Issue issue = new() { Number = CheckoutIssueNumber };
        InvalidOperationException cause = new("The dialog host has shut down.");

        UnhandledInteractionException<Issue, bool> empty = new();
        UnhandledInteractionException<Issue, bool> withMessage = new(NobodyAnswersMessage);
        UnhandledInteractionException<Issue, bool> withCause = new(NobodyAnswersMessage, cause);
        UnhandledInteractionException<Issue, bool> forQuestion = new(confirmClose, issue);

        Console.WriteLine(empty.Interaction is null);
        Console.WriteLine(withMessage.Message);
        Console.WriteLine(ReferenceEquals(cause, withCause.InnerException));
        Console.WriteLine(forQuestion.Message);
        Console.WriteLine(ReferenceEquals(confirmClose, forQuestion.Interaction));
        Console.WriteLine(ReferenceEquals(issue, forQuestion.Input));

        // Output:
        // True
        // Nobody answers close confirmations.
        // True
        // Failed to find a registration for an Interaction.
        // True
        // True
    }

    /// <summary>Registers each kind of handler on an interaction, asks a question and removes a handler, using only <see cref="IInteraction{TInput, TOutput}"/>.</summary>
    /// <returns>A task that completes when the last answer has been read.</returns>
    public static async Task HandleThroughInterface()
    {
        IssueTriageViewModel triage = new(new(InMemoryGitHubServer.CreateSeeded()));
        Interaction<Issue, bool> confirmClose = triage.ConfirmClose;
        Issue issue = new() { Number = CheckoutIssueNumber };
        using IDisposable declines = confirmClose.RegisterHandler(static context => context.SetOutput(false));
        using IDisposable asksAgain = confirmClose.RegisterHandler(static context => Signal.Return(context.IsHandled));
        IDisposable confirms = confirmClose.RegisterHandler(static context =>
        {
            context.SetOutput(true);
            return Task.CompletedTask;
        });

        Console.WriteLine(await confirmClose.Handle(issue));

        confirms.Dispose();

        Console.WriteLine(await confirmClose.Handle(issue));

        // Output:
        // True
        // False
    }

    /// <summary>Asks the question as an observable with <c>WhenHandled</c>: nothing is asked until it is subscribed, and each subscription asks again.</summary>
    /// <returns>A task that completes when both answers have been read.</returns>
    public static async Task AskAsAnObservable()
    {
        Interaction<Issue, bool> confirmClose = new();
        Issue issue = new() { Number = CheckoutIssueNumber };
        int asked = 0;
        using IDisposable registration = confirmClose.RegisterHandler(context =>
        {
            asked++;
            context.SetOutput(true);
        });

        IObservable<bool> question = confirmClose.WhenHandled(issue);

        Console.WriteLine(asked);
        Console.WriteLine(await Signal.ToTask(question));
        Console.WriteLine(await Signal.ToTask(question));
        Console.WriteLine(asked);

        // Output:
        // 0
        // True
        // True
        // 2
    }

    /// <summary>Asks the question through <see cref="IInteraction{TInput, TOutput}"/>, whose <c>Handle</c> returns an observable.</summary>
    /// <returns>A task that completes when the answer has been read.</returns>
    public static async Task AskThroughInterfaceAsAnObservable()
    {
        IInteraction<Issue, bool> confirmClose = new Interaction<Issue, bool>();
        Issue issue = new() { Number = CheckoutIssueNumber };
        using IDisposable registration = confirmClose.RegisterHandler(static context => context.SetOutput(context.Input.Number == CheckoutIssueNumber));

        IObservable<bool> question = confirmClose.Handle(issue);

        Console.WriteLine(await Signal.ToTask(question));

        // Output:
        // True
    }

    /// <summary>Runs every handler on the scheduler the interaction was created with; an app passes its main thread's scheduler so a dialog opens on the UI thread.</summary>
    /// <returns>A task that completes when the answer has been read.</returns>
    public static async Task AnswerOnTheHandlerScheduler()
    {
        Interaction<Issue, bool> confirmClose = new(TaskPoolSequencer.Default);
        Issue issue = new() { Number = CheckoutIssueNumber };
        using IDisposable registration = confirmClose.RegisterHandler(static context =>
        {
            Console.WriteLine(Thread.CurrentThread.IsThreadPoolThread);
            context.SetOutput(true);
        });

        Console.WriteLine(await confirmClose.Handle(issue));

        // Output:
        // True
        // True
    }

    /// <summary>Runs an action and reports whether it threw <see cref="InvalidOperationException"/>.</summary>
    /// <param name="action">The action to run.</param>
    /// <returns><see langword="true"/> when the action threw.</returns>
    private static bool RefusesWithInvalidOperation(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    /// <summary>Fills the transfer draft with a payment to the utility that needs an approval code.</summary>
    /// <param name="viewModel">The transfer view model.</param>
    private static void FillLargeTransfer(TransferViewModel viewModel)
    {
        viewModel.Draft.Source = viewModel.Accounts[0];
        viewModel.Draft.Payee = viewModel.Payees[0];
        viewModel.Draft.Amount = LargeTransferAmount;
    }

    /// <summary>Loads the accounts and payees of the transfer screen.</summary>
    /// <returns>A task that completes with the view model and its view.</returns>
    private static async Task<(TransferViewModel ViewModel, TransferView View)> OpenTransferScreenAsync()
    {
        TransferViewModel viewModel = new(new InMemoryBankingBackend());

        await viewModel.LoadAsync();
        return (viewModel, new() { ViewModel = viewModel });
    }

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
}
