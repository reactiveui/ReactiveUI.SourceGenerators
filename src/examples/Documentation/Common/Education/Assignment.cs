// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>A piece of work that students submit for a course.</summary>
/// <param name="Id">The identifier of the assignment.</param>
/// <param name="CourseCode">The code of the course the assignment belongs to.</param>
/// <param name="Title">The name of the assignment.</param>
/// <param name="Weight">How much the assignment counts towards the course, as a percentage.</param>
/// <param name="MaxScore">The highest score the assignment awards.</param>
/// <param name="DueDate">The last day a student can submit without being late.</param>
[System.Diagnostics.DebuggerDisplay("Assignment: {Title} ({Weight}%)")]
public sealed record Assignment(int Id, string CourseCode, string Title, decimal Weight, decimal MaxScore, DateOnly DueDate);
