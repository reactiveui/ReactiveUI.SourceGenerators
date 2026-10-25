// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>A course with a weekly class time and a limited number of places.</summary>
/// <param name="Code">The code that identifies the course, such as <c>CS101</c>.</param>
/// <param name="Title">The name of the course.</param>
/// <param name="Capacity">The number of students the course takes.</param>
/// <param name="Day">The day of the week the class meets.</param>
/// <param name="StartHour">The hour the class starts, from 0 to 23.</param>
/// <param name="DurationHours">How many hours the class lasts.</param>
/// <param name="PassMark">The weighted average, as a percentage, a student needs to pass.</param>
[System.Diagnostics.DebuggerDisplay("Course: {Code}: {Title}")]
public sealed record Course(string Code, string Title, int Capacity, DayOfWeek Day, int StartHour, int DurationHours, decimal PassMark)
{
    /// <summary>Checks whether this course meets at the same time as another.</summary>
    /// <param name="other">The other course.</param>
    /// <returns><see langword="true"/> when the classes are on the same day and their hours overlap.</returns>
    public bool ClashesWith(Course other) =>
        Day == other.Day && StartHour < other.StartHour + other.DurationHours && other.StartHour < StartHour + DurationHours;
}
