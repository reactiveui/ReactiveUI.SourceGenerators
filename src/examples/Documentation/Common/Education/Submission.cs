// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>A student's hand-in of an assignment.</summary>
/// <param name="StudentId">The identifier of the student.</param>
/// <param name="AssignmentId">The identifier of the assignment.</param>
/// <param name="SubmittedAt">When the student handed the work in.</param>
/// <param name="IsLate">Whether the student handed the work in after the due date.</param>
[System.Diagnostics.DebuggerDisplay("Submission: Assignment {AssignmentId}, late = {IsLate}")]
public sealed record Submission(int StudentId, int AssignmentId, DateTimeOffset SubmittedAt, bool IsLate);
