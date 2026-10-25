// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>Works out how a student is doing in a course.</summary>
public static class GradeCalculator
{
    /// <summary>The value of a percentage that means everything.</summary>
    private const decimal FullPercent = 100;

    /// <summary>Works out the weighted average of the assignments that have a score.</summary>
    /// <param name="assignments">The assignments of the course.</param>
    /// <param name="grades">The scores recorded for the student.</param>
    /// <returns>The average as a percentage from 0 to 100, or 0 when no assignment has a score.</returns>
    public static decimal WeightedAverage(IReadOnlyList<Assignment> assignments, IReadOnlyList<Grade> grades)
    {
        decimal earned = 0;
        decimal weighed = 0;

        foreach (Assignment assignment in assignments)
        {
            foreach (Grade grade in grades)
            {
                if (grade.AssignmentId != assignment.Id)
                {
                    continue;
                }

                earned += assignment.Weight * grade.Score / assignment.MaxScore;
                weighed += assignment.Weight;
            }
        }

        return weighed == 0 ? 0 : earned / weighed * FullPercent;
    }
}
