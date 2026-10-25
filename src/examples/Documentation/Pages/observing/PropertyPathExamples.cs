// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.CloudStorage;
using ReactiveUI.Binding.Documentation.Education;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Primitives;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Observes properties several objects deep, and shows which changes along the path reach the subscriber.</summary>
public static class PropertyPathExamples
{
    /// <summary>The number of the issue about the unresponsive checkout button, assigned to Priya.</summary>
    private const int CheckoutBugNumber = 101;

    /// <summary>The number of the gift-card issue, which has no assignee.</summary>
    private const int GiftCardNumber = 102;

    /// <summary>The account name of Priya Nair.</summary>
    private const string PriyaLogin = "priya-nair";

    /// <summary>The account name of Tomas Berg.</summary>
    private const string TomasLogin = "tomas-berg";

    /// <summary>The account name of Maria Santos.</summary>
    private const string MariaLogin = "maria-santos";

    /// <summary>The text that stands for nobody working on an issue.</summary>
    private const string Unassigned = "unassigned";

    /// <summary>The index of the everyday account in the loaded accounts.</summary>
    private const int EverydayAccountIndex = 0;

    /// <summary>The index of the savings account in the loaded accounts.</summary>
    private const int SavingsAccountIndex = 1;

    /// <summary>The balance the everyday account has after the rent is paid.</summary>
    private const decimal EverydayBalanceAfterRent = 1250.75M;

    /// <summary>The balance the everyday account drops to after the old account is left behind.</summary>
    private const decimal EverydayBalanceAfterBills = 100M;

    /// <summary>The balance the savings account has after a withdrawal.</summary>
    private const decimal SavingsBalanceAfterWithdrawal = 15000M;

    /// <summary>The identifier of the student Aisha.</summary>
    private const int AishaId = 1;

    /// <summary>The identifier of the student Chloe.</summary>
    private const int ChloeId = 3;

    /// <summary>The index of the final project in the assignments of CS101.</summary>
    private const int FinalProjectIndex = 2;

    /// <summary>The score the teacher records for the final project.</summary>
    private const decimal FinalProjectScore = 88M;

    /// <summary>The index of the launch banner in the objects of the media bucket.</summary>
    private const int LaunchBannerIndex = 2;

    /// <summary>The index of the team offsite photo in the objects of the media bucket.</summary>
    private const int TeamOffsiteIndex = 3;

    /// <summary>The key the launch banner has after it is renamed.</summary>
    private const string RenamedLaunchBannerKey = "photos/2026/launch-banner-v2.png";

    /// <summary>Observes the login of the assignee of the selected issue: a path of three properties.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task ObserveSelectedIssueAssigneeLogin()
    {
        IssueBoardViewModel board = await OpenWebshopBoardAsync();
        Issue checkoutBug = FindIssue(board, CheckoutBugNumber);

        List<string> logins = [];

        using (board.WhenChanged(x => x.SelectedIssue!.Assignee!.Login).Subscribe(logins.Add))
        {
            // Nothing is selected yet, so the path has no value to read.
            Console.WriteLine(logins.Count);

            board.SelectedIssue = checkoutBug;

            checkoutBug.Assignee!.Login = TomasLogin;
        }

        Console.WriteLine(string.Join(", ", logins));

        // Output:
        // 0
        // priya-nair, tomas-berg
    }

    /// <summary>Replaces the assignee, the object in the middle of the path: the observation moves to the new object.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task ReplaceAssigneeInTheMiddleOfThePath()
    {
        IssueBoardViewModel board = await OpenWebshopBoardAsync();
        Issue checkoutBug = FindIssue(board, CheckoutBugNumber);
        User previousAssignee = checkoutBug.Assignee!;

        board.SelectedIssue = checkoutBug;

        List<string> logins = [];

        using (board.WhenChanged(x => x.SelectedIssue!.Assignee!.Login).Subscribe(logins.Add))
        {
            User newAssignee = new() { Login = TomasLogin };

            checkoutBug.Assignee = newAssignee;

            // The previous assignee no longer belongs to the path, so its changes are not observed.
            previousAssignee.Login = MariaLogin;

            newAssignee.Login = MariaLogin;
        }

        Console.WriteLine(string.Join(", ", logins));

        // Output:
        // priya-nair, tomas-berg, maria-santos
    }

    /// <summary>Follows an unassigned issue: a null in the middle of the path emits nothing, and the next assignee is observed.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task NullInTheMiddleOfThePathEmitsNothing()
    {
        IssueBoardViewModel board = await OpenWebshopBoardAsync();
        Issue giftCards = FindIssue(board, GiftCardNumber);

        List<string> logins = [];

        using (board.WhenChanged(x => x.SelectedIssue!.Assignee!.Login).Subscribe(logins.Add))
        {
            // The issue has no assignee, so there is no login to report.
            board.SelectedIssue = giftCards;

            giftCards.Assignee = new User { Login = TomasLogin };

            // Removing the assignee does not emit a null login either.
            giftCards.Assignee = null;

            giftCards.Assignee = new User { Login = MariaLogin };

            board.SelectedIssue = null;
        }

        Console.WriteLine(string.Join(", ", logins));

        // Output:
        // tomas-berg, maria-santos
    }

    /// <summary>Observes the assignee itself: a null final value is emitted while every object on the path exists.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task NullFinalValueIsEmitted()
    {
        IssueBoardViewModel board = await OpenWebshopBoardAsync();
        Issue giftCards = FindIssue(board, GiftCardNumber);
        User tomas = new() { Login = TomasLogin };

        // The trailing ! only satisfies the compiler: the emitted values include null.
        List<string> assignees = [];

        using (board.WhenChanged(x => x.SelectedIssue!.Assignee!).Subscribe(assignee => assignees.Add(assignee?.Login ?? Unassigned)))
        {
            board.SelectedIssue = giftCards;

            giftCards.Assignee = tomas;

            giftCards.Assignee = null;

            // With nothing selected there is no issue to read the assignee from.
            board.SelectedIssue = null;
        }

        Console.WriteLine(string.Join(", ", assignees));

        // Output:
        // unassigned, tomas-berg, unassigned
    }

    /// <summary>Changes objects along the path without changing the login: only a changed final value is emitted.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task OnlyAChangedFinalValueIsEmitted()
    {
        IssueBoardViewModel board = await OpenWebshopBoardAsync();
        Issue checkoutBug = FindIssue(board, CheckoutBugNumber);

        board.SelectedIssue = checkoutBug;

        List<string> logins = [];

        using (board.WhenChanged(x => x.SelectedIssue!.Assignee!.Login).Subscribe(logins.Add))
        {
            // The same login again.
            checkoutBug.Assignee!.Login = PriyaLogin;

            // A different property of the assignee.
            checkoutBug.Assignee.DisplayName = "Priya N.";

            // A different object with the same login.
            checkoutBug.Assignee = new User { Login = PriyaLogin };

            // A property of the issue that is not on the path.
            checkoutBug.Title = "Checkout button fails on Safari";

            checkoutBug.Assignee.Login = TomasLogin;
        }

        Console.WriteLine(string.Join(", ", logins));

        // Output:
        // priya-nair, tomas-berg
    }

    /// <summary>Observes the enrolments of the student a teacher picks in the gradebook.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task ObserveSelectedStudentEnrolments()
    {
        GradebookViewModel gradebook = await OpenIntroductionToProgrammingAsync();
        Student aisha = FindStudent(gradebook, AishaId);
        Student chloe = FindStudent(gradebook, ChloeId);

        List<int> gradeCounts = [];

        using (gradebook.WhenChanged(x => x.SelectedStudent!.Enrolments).Subscribe(enrolments => gradeCounts.Add(CountGrades(enrolments))))
        {
            gradebook.SelectedStudent = chloe;

            gradebook.SelectedStudent = aisha;

            gradebook.SelectedAssignment = gradebook.Assignments[FinalProjectIndex];
            gradebook.ScoreToRecord = FinalProjectScore;
            await gradebook.RecordGradeAsync();
        }

        // Recording a grade replaces the student's enrolments, and the replacement reaches the subscriber.
        Console.WriteLine(string.Join(", ", gradeCounts));

        // Output:
        // 1, 2, 3
    }

    /// <summary>Observes the money available in the account a draft transfer pays from, through the draft and its source account.</summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task ObserveDraftSourceAvailableBalance()
    {
        TransferViewModel transfer = await OpenTransferScreenAsync();
        Account everyday = transfer.Accounts[EverydayAccountIndex];
        Account savings = transfer.Accounts[SavingsAccountIndex];

        List<decimal> available = [];

        using (transfer.WhenChanged(x => x.Draft.Source!.AvailableBalance).Subscribe(available.Add))
        {
            transfer.Draft.Source = everyday;

            everyday.Balance = EverydayBalanceAfterRent;

            transfer.Draft.Source = savings;

            // The everyday account is no longer the source, so its balance is no longer observed.
            everyday.Balance = EverydayBalanceAfterBills;

            savings.Balance = SavingsBalanceAfterWithdrawal;

            Console.WriteLine(string.Join(", ", available));
        }

        // Output:
        // 2950.75, 1750.75, 15230.00, 15000
    }

    /// <summary>
    /// Observes the selected storage object and reads its key on each delivery. <see cref="StorageObject"/> raises
    /// no notification, so the observation follows the selection only: a change to the object itself is not
    /// reported. A path such as <c>x.SelectedObject!.Key</c> would stop following at the object, which the analyzer
    /// reports as RXUIBIND010, so the path ends at the selection.
    /// </summary>
    /// <returns>A task that completes when the example has run.</returns>
    public static async Task ObserveSelectionOfAnObjectThatRaisesNoNotifications()
    {
        StorageBrowserViewModel browser = await OpenMediaBucketAsync();
        StorageObject launchBanner = browser.Objects[LaunchBannerIndex];
        StorageObject teamOffsite = browser.Objects[TeamOffsiteIndex];

        List<string> keys = [];

        browser.SelectedObject = launchBanner;

        using (browser.WhenChanged(x => x.SelectedObject!).Subscribe(selected => keys.Add(selected.Key)))
        {
            // A plain setter raises nothing, so the subscriber is not told.
            launchBanner.Key = RenamedLaunchBannerKey;

            browser.SelectedObject = teamOffsite;

            // Selecting the object again reads the key it has now.
            browser.SelectedObject = launchBanner;
        }

        Console.WriteLine(string.Join(", ", keys));

        // Output:
        // photos/2026/launch-banner.png, photos/2026/team-offsite.jpg, photos/2026/launch-banner-v2.png
    }

    /// <summary>Loads the webshop repository and its open issues, signed in as Priya.</summary>
    /// <returns>The issue board.</returns>
    private static async Task<IssueBoardViewModel> OpenWebshopBoardAsync()
    {
        InMemoryGitHubServer server = InMemoryGitHubServer.CreateSeeded();
        IssueBoardViewModel board = new(server) { Token = InMemoryGitHubServer.PriyaToken };

        await board.SignInAsync();

        board.SelectedRepository = board.Repositories[0];
        await board.LoadIssuesAsync();

        return board;
    }

    /// <summary>Loads the students of the introduction to programming course.</summary>
    /// <returns>The gradebook.</returns>
    private static async Task<GradebookViewModel> OpenIntroductionToProgrammingAsync()
    {
        InMemoryStudentRecords records = InMemoryStudentRecords.CreateSeeded();
        GradebookViewModel gradebook = new(records);

        await gradebook.LoadCoursesAsync();

        gradebook.SelectedCourse = gradebook.Courses[0];
        await gradebook.OpenCourseAsync();

        return gradebook;
    }

    /// <summary>Loads the accounts and payees of the transfer screen.</summary>
    /// <returns>The transfer screen.</returns>
    private static async Task<TransferViewModel> OpenTransferScreenAsync()
    {
        InMemoryBankingBackend backend = new();
        TransferViewModel transfer = new(backend);

        await transfer.LoadAsync();

        return transfer;
    }

    /// <summary>Lists the objects of the media bucket.</summary>
    /// <returns>The storage browser.</returns>
    private static async Task<StorageBrowserViewModel> OpenMediaBucketAsync()
    {
        InMemoryObjectStorage storage = InMemoryObjectStorage.CreateSeeded();
        StorageBrowserViewModel browser = new(storage);

        await browser.LoadBucketsAsync();

        browser.SelectedBucket = browser.Buckets[0];
        await browser.LoadObjectsAsync();

        return browser;
    }

    /// <summary>Finds an issue on the board.</summary>
    /// <param name="board">The issue board.</param>
    /// <param name="number">The number of the issue.</param>
    /// <returns>The issue.</returns>
    /// <exception cref="InvalidOperationException">The board has no issue with the number.</exception>
    private static Issue FindIssue(IssueBoardViewModel board, int number) =>
        board.Issues.First(issue => issue.Number == number);

    /// <summary>Finds a student on the roster.</summary>
    /// <param name="gradebook">The gradebook.</param>
    /// <param name="id">The identifier of the student.</param>
    /// <returns>The student.</returns>
    /// <exception cref="InvalidOperationException">The roster has no student with the identifier.</exception>
    private static Student FindStudent(GradebookViewModel gradebook, int id) =>
        gradebook.Roster.First(student => student.Id == id);

    /// <summary>Counts the scores recorded across a list of enrolments.</summary>
    /// <param name="enrolments">The enrolments.</param>
    /// <returns>The number of scores.</returns>
    private static int CountGrades(IReadOnlyList<Enrolment> enrolments) =>
        enrolments.Sum(static enrolment => enrolment.Grades.Count);
}
