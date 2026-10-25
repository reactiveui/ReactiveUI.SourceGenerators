// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>Why the student records service refused a request.</summary>
public enum RecordsFailure
{
    /// <summary>The course has no places left.</summary>
    CourseFull = 0,

    /// <summary>The course meets at the same time as another course the student takes.</summary>
    TimetableClash = 1,

    /// <summary>The student already has a place in the course.</summary>
    AlreadyEnrolled = 2,

    /// <summary>The student has no place in the course.</summary>
    NotEnrolled = 3,

    /// <summary>The student does not exist.</summary>
    UnknownStudent = 4,

    /// <summary>The course does not exist.</summary>
    UnknownCourse = 5,

    /// <summary>The assignment does not exist.</summary>
    UnknownAssignment = 6,

    /// <summary>The score is below zero or above the assignment's maximum.</summary>
    ScoreOutOfRange = 7,

    /// <summary>The service is down.</summary>
    ServiceUnavailable = 8,
}
