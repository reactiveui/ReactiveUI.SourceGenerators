// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>The question the gradebook asks before it takes a student out of a course.</summary>
/// <param name="Student">The student who would be dropped.</param>
/// <param name="Course">The course the student would leave.</param>
[System.Diagnostics.DebuggerDisplay("DropRequest: Drop {Student} from {Course}")]
public sealed record DropRequest(Student Student, Course Course);
