// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using ReactiveUI.Binding.Documentation.Banking;
using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>Shows a search box that reacts to typing: observe the text, wait for a pause, and show the newest response.</summary>
public static class SearchAsYouTypeExamples
{
    /// <summary>The repository the search box looks in.</summary>
    private const string Webshop = "acme/webshop";

    /// <summary>The first term the user searches for.</summary>
    private const string CheckoutTerm = "checkout";

    /// <summary>The second term the user searches for.</summary>
    private const string GiftTerm = "gift";

    /// <summary>The term as far as the user has typed it when they pause.</summary>
    private const string CraTerm = "cra";

    /// <summary>The term the user finishes typing.</summary>
    private const string CrashTerm = "crash";

    /// <summary>How long the user must stop typing before a search starts, in milliseconds.</summary>
    private const int QuietPeriodMilliseconds = 60;

    /// <summary>How long the user takes between two key presses, in milliseconds.</summary>
    private const int KeyGapMilliseconds = 10;

    /// <summary>How long a slow server takes to answer, in milliseconds.</summary>
    private const int SlowResponseMilliseconds = 400;

    /// <summary>How long the examples wait for a search to finish, in milliseconds.</summary>
    private const int SettleMilliseconds = 250;

    /// <summary>How long the user must stop typing before a search starts.</summary>
    private static readonly TimeSpan _quietPeriod = TimeSpan.FromMilliseconds(QuietPeriodMilliseconds);

    /// <summary>How long the user takes between two key presses.</summary>
    private static readonly TimeSpan _keyGap = TimeSpan.FromMilliseconds(KeyGapMilliseconds);

    /// <summary>How long a slow server takes to answer.</summary>
    private static readonly TimeSpan _slowResponse = TimeSpan.FromMilliseconds(SlowResponseMilliseconds);

    /// <summary>How long the examples wait for a search to finish.</summary>
    private static readonly TimeSpan _settle = TimeSpan.FromMilliseconds(SettleMilliseconds);

    /// <summary>Reacts to each distinct, trimmed, non-blank term the user types.</summary>
    public static void ObserveDistinctTrimmedSearchTerms()
    {
        const string PaddedCheckout = "  checkout ";

        InMemoryGitHubServer server = InMemoryGitHubServer.CreateSeeded();
        IssueSearchViewModel viewModel = new(server, Webshop);

        using IDisposable subscription = viewModel.WhenChanged(x => x.SearchTerm)
            .Select(static term => term.Trim())
            .DistinctUntilChanged()
            .Where(static term => term.Length > 0)
            .Subscribe(Console.WriteLine);

        viewModel.SearchTerm = PaddedCheckout;
        viewModel.SearchTerm = CheckoutTerm;
        viewModel.SearchTerm = "   ";
        viewModel.SearchTerm = GiftTerm;

        // Output:
        // checkout
        // gift
    }

    /// <summary>Waits for a pause in typing before a term goes on, on a virtual clock.</summary>
    public static void ThrottleTypingUntilPause()
    {
        VirtualClock clock = new();
        InMemoryGitHubServer server = InMemoryGitHubServer.CreateSeeded();
        IssueSearchViewModel viewModel = new(server, Webshop);

        using IDisposable subscription = viewModel.WhenAnyValue(x => x.SearchTerm)
            .Throttle(_quietPeriod, clock)
            .Subscribe(Console.WriteLine);

        viewModel.SearchTerm = "c";
        clock.AdvanceBy(_keyGap);
        viewModel.SearchTerm = "cr";
        clock.AdvanceBy(_keyGap);
        viewModel.SearchTerm = CraTerm;

        // Every key press restarts the wait, so nothing has settled yet.
        clock.AdvanceBy(_keyGap);
        Console.WriteLine("Still typing");

        clock.AdvanceBy(_quietPeriod);

        viewModel.SearchTerm = CrashTerm;
        clock.AdvanceBy(_quietPeriod);

        // Output:
        // Still typing
        // cra
        // crash
    }

    /// <summary>Searches once per settled term and shows the matches.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task SearchWhenTypingPauses()
    {
        InMemoryGitHubServer server = await CreateSignedInServerAsync();
        IssueSearchViewModel viewModel = new(server, Webshop);

        using IDisposable subscription = viewModel.StartSearching(Sequencer.Default, _quietPeriod);

        int quotaAtStart = server.RateLimitRemaining;

        viewModel.SearchTerm = "  checkout ";
        await Task.Delay(_settle);

        Console.WriteLine(string.Join(", ", TitlesOf(viewModel.Results)));
        Console.WriteLine(viewModel.IsSearching);
        Console.WriteLine(quotaAtStart - server.RateLimitRemaining);

        // The same term without its padding trims to the term already searched, so no request goes out.
        viewModel.SearchTerm = CheckoutTerm;
        await Task.Delay(_settle);
        Console.WriteLine(quotaAtStart - server.RateLimitRemaining);

        viewModel.SearchTerm = "GIFT";
        await Task.Delay(_settle);

        Console.WriteLine(string.Join(", ", TitlesOf(viewModel.Results)));
        Console.WriteLine(quotaAtStart - server.RateLimitRemaining);

        // Output:
        // Checkout button unresponsive on Safari
        // False
        // 1
        // 1
        // Add gift-card support
        // 2
    }

    /// <summary>Binds the busy flag to a button and a progress bar, and the results to a list, while a search waits for its response.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task BindSearchStateToView()
    {
        InMemoryGitHubServer server = await CreateSignedInServerAsync();
        IssueSearchViewModel viewModel = new(server, Webshop);
        IssueSearchView view = new() { ViewModel = viewModel };

        using IDisposable search = viewModel.StartSearching(Sequencer.Default, _quietPeriod);
        using IDisposable typing = viewModel.BindTwoWay(view, x => x.SearchTerm, v => v.SearchBox.Text);
        using IDisposable buttonBinding = viewModel.BindOneWay(view, x => x.IsSearching, v => v.SearchButton.IsEnabled, static searching => !searching);
        using IDisposable barBinding = viewModel.BindOneWay(view, x => x.IsSearching, v => v.BusyBar.IsVisible);
        using IDisposable listBinding = viewModel.BindOneWay(view, x => x.Results, v => v.ResultList.ItemsSource, static results => results);

        Console.WriteLine(view.SearchButton.IsEnabled);
        Console.WriteLine(view.BusyBar.IsVisible);

        server.Latency = _slowResponse;

        view.SearchBox.Text = CheckoutTerm;
        await Task.Delay(_settle);

        Console.WriteLine(view.SearchButton.IsEnabled);
        Console.WriteLine(view.BusyBar.IsVisible);
        Console.WriteLine(view.ResultList.ItemsSource.Cast<Issue>().Count());

        await Task.Delay(_slowResponse);

        Console.WriteLine(view.SearchButton.IsEnabled);
        Console.WriteLine(view.BusyBar.IsVisible);
        Console.WriteLine(string.Join(", ", TitlesOf(viewModel.Results)));

        // Output:
        // True
        // False
        // False
        // True
        // 0
        // True
        // False
        // Checkout button unresponsive on Safari
    }

    /// <summary>Drops the response of a search that a newer search has replaced.</summary>
    /// <returns>A task that completes when the example has finished.</returns>
    public static async Task IgnoreStaleSearchResponse()
    {
        InMemoryGitHubServer server = await CreateSignedInServerAsync();
        IssueSearchViewModel viewModel = new(server, Webshop);

        using IDisposable subscription = viewModel.StartSearching(Sequencer.Default, _quietPeriod);

        // The first response takes longer than the second.
        server.Latency = _slowResponse;
        viewModel.SearchTerm = CheckoutTerm;
        await Task.Delay(_settle);

        server.Latency = TimeSpan.Zero;
        viewModel.SearchTerm = GiftTerm;
        await Task.Delay(_settle);

        Console.WriteLine(string.Join(", ", TitlesOf(viewModel.Results)));
        Console.WriteLine(viewModel.IsSearching);

        // The response for "checkout" arrives now and is dropped.
        await Task.Delay(_slowResponse);

        Console.WriteLine(string.Join(", ", TitlesOf(viewModel.Results)));

        // Output:
        // Add gift-card support
        // False
        // Add gift-card support
    }

    /// <summary>Copies the amount box into the transfer draft once the customer stops typing, without a two-way binding, on a virtual clock.</summary>
    public static void DebounceAmountIntoTransferDraft()
    {
        VirtualClock clock = new();
        TransferViewModel screen = new(new InMemoryBankingBackend());
        TransferView view = new() { ViewModel = screen };

        using IDisposable subscription = view.AmountTextBox.WhenChanged(x => x.Text)
            .Where(static text => !string.IsNullOrEmpty(text))
            .Throttle(_quietPeriod, clock)
            .Subscribe(text => screen.Draft.Amount = decimal.Parse(text, CultureInfo.InvariantCulture));

        view.AmountTextBox.Text = "1";
        clock.AdvanceBy(_keyGap);
        view.AmountTextBox.Text = "12";
        clock.AdvanceBy(_keyGap);
        view.AmountTextBox.Text = "125";

        Console.WriteLine(screen.Draft.Amount);

        clock.AdvanceBy(_quietPeriod);

        Console.WriteLine(screen.Draft.Amount);

        // Output:
        // 0
        // 125
    }

    /// <summary>Creates a seeded server with Priya signed in.</summary>
    /// <returns>A task whose result is the signed-in server.</returns>
    private static async Task<InMemoryGitHubServer> CreateSignedInServerAsync()
    {
        InMemoryGitHubServer server = InMemoryGitHubServer.CreateSeeded();
        _ = await server.SignInAsync(InMemoryGitHubServer.PriyaToken);
        return server;
    }

    /// <summary>Reads the titles of a list of issues.</summary>
    /// <param name="issues">The issues.</param>
    /// <returns>The titles, in the order of the issues.</returns>
    private static IEnumerable<string> TitlesOf(IReadOnlyList<Issue> issues) =>
        issues.Select(static issue => issue.Title);
}
