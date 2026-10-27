// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.GitHub;
using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Primitives;
using ReactiveUI.Primitives.Concurrency;

namespace ReactiveUI.Binding.Documentation.Observing;

/// <summary>
/// The search box of the issue board. The user types in <see cref="SearchTerm"/>. After a pause in typing, the view
/// model searches the open issues of one repository and publishes the matches in <see cref="Results"/>.
/// </summary>
/// <param name="api">The server to search.</param>
/// <param name="repository">The repository whose open issues are searched, such as <c>acme/webshop</c>.</param>
[System.Diagnostics.DebuggerDisplay("IssueSearchViewModel: SearchTerm = {SearchTerm}, IsSearching = {IsSearching}, Results = {Results.Count}")]
public sealed class IssueSearchViewModel(IGitHubApi api, string repository) : ObservableObject
{
    /// <summary>Gets or sets the text the user has typed in the search box.</summary>
    public string SearchTerm
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets a value indicating whether a search is waiting for its response.</summary>
    public bool IsSearching
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the open issues that matched the latest search.</summary>
    public IReadOnlyList<Issue> Results
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>
    /// Starts reacting to <see cref="SearchTerm"/>. A search starts once the term has stopped changing for
    /// <paramref name="quietPeriod"/>. The term is trimmed, an unchanged or blank term starts nothing, and a new
    /// search replaces the one still waiting for its response.
    /// </summary>
    /// <param name="sequencer">The sequencer that measures the quiet period.</param>
    /// <param name="quietPeriod">How long the term must stay unchanged before a search starts.</param>
    /// <returns>A subscription that stops the reaction when disposed.</returns>
    public IDisposable StartSearching(ISequencer sequencer, TimeSpan quietPeriod) =>
        this.WhenAnyValue(x => x.SearchTerm)
            .Throttle(quietPeriod, sequencer)
            .Select(static term => term.Trim())
            .DistinctUntilChanged()
            .Where(static term => term.Length > 0)
            .SwitchMap(BeginSearch)
            .Subscribe(EndSearch);

    /// <summary>Finds the open issues whose title contains a term.</summary>
    /// <param name="term">The text to look for, ignoring case.</param>
    /// <returns>The matching issues, newest first.</returns>
    public async Task<IReadOnlyList<Issue>> SearchAsync(string term)
    {
        IReadOnlyList<Issue> open = await api.ListIssuesAsync(repository, IssueState.Open);

        return open.Where(issue => issue.Title.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>Marks the view model as busy and starts a search.</summary>
    /// <param name="term">The trimmed term to search for.</param>
    /// <returns>A stream that emits the matches once, when the response arrives.</returns>
    private IObservable<IReadOnlyList<Issue>> BeginSearch(string term)
    {
        IsSearching = true;
        return SearchAsync(term).ToObservable();
    }

    /// <summary>Publishes the matches of the latest search and clears the busy flag.</summary>
    /// <param name="matches">The matches.</param>
    private void EndSearch(IReadOnlyList<Issue> matches)
    {
        Results = matches;
        IsSearching = false;
    }
}
