// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>A student's place in a course, with the work the student handed in and the scores received.</summary>
/// <param name="StudentId">The identifier of the student.</param>
/// <param name="CourseCode">The code of the course.</param>
/// <param name="EnrolledOn">The day the student took the place.</param>
/// <param name="Grades">The scores recorded so far.</param>
/// <param name="Submissions">The assignments the student handed in.</param>
[System.Diagnostics.DebuggerDisplay("Enrolment: Student {StudentId} in {CourseCode}")]
public sealed record Enrolment(int StudentId, string CourseCode, DateOnly EnrolledOn, IReadOnlyList<Grade> Grades, IReadOnlyList<Submission> Submissions);
