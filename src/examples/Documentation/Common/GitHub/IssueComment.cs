// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.GitHub;

/// <summary>A comment on an issue.</summary>
/// <param name="Author">The account name of the person who wrote the comment.</param>
/// <param name="Body">The text of the comment.</param>
/// <param name="CreatedAt">When the comment was written.</param>
[System.Diagnostics.DebuggerDisplay("IssueComment: {Author}: {Body}")]
public sealed record IssueComment(string Author, string Body, DateTimeOffset CreatedAt);
