// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>The score a teacher gave a student for an assignment.</summary>
/// <param name="StudentId">The identifier of the student.</param>
/// <param name="AssignmentId">The identifier of the assignment.</param>
/// <param name="Score">The score awarded, from 0 to the assignment's maximum.</param>
/// <param name="RecordedOn">When the teacher recorded the score.</param>
[System.Diagnostics.DebuggerDisplay("Grade: Assignment {AssignmentId}: {Score}")]
public sealed record Grade(int StudentId, int AssignmentId, decimal Score, DateTimeOffset RecordedOn);
