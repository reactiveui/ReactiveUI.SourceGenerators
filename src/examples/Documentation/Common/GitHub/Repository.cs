// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>A repository that holds issues.</summary>
/// <param name="Owner">The account or organisation that owns the repository.</param>
/// <param name="Name">The name of the repository.</param>
/// <param name="Description">What the repository is for.</param>
/// <param name="OpenIssueCount">How many issues are open when the server reads the repository.</param>
[System.Diagnostics.DebuggerDisplay("Repository: {FullName}")]
public sealed record Repository(string Owner, string Name, string Description, int OpenIssueCount)
{
    /// <summary>Gets the name that identifies the repository to the server, such as <c>acme/webshop</c>.</summary>
    public string FullName => $"{Owner}/{Name}";
}
