// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>The student records service of a school or university. Every call is asynchronous, as a call to a real service is.</summary>
public interface IStudentRecords
{
    /// <summary>Lists every course.</summary>
    /// <returns>The courses, by code.</returns>
    /// <exception cref="StudentRecordsException">The service is down.</exception>
    Task<IReadOnlyList<Course>> GetCoursesAsync();

    /// <summary>Lists every student, each with the courses the student takes.</summary>
    /// <returns>Copies of the students, by identifier.</returns>
    /// <exception cref="StudentRecordsException">The service is down.</exception>
    Task<IReadOnlyList<Student>> GetStudentsAsync();

    /// <summary>Lists the students who take a course, each with the courses the student takes.</summary>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>Copies of the students, by identifier.</returns>
    /// <exception cref="StudentRecordsException">The course does not exist or the service is down.</exception>
    Task<IReadOnlyList<Student>> GetRosterAsync(string courseCode);

    /// <summary>Lists the assignments of a course.</summary>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The assignments, by identifier.</returns>
    /// <exception cref="StudentRecordsException">The course does not exist or the service is down.</exception>
    Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(string courseCode);

    /// <summary>Lists the courses a student takes.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <returns>The student's enrolments, by course code.</returns>
    /// <exception cref="StudentRecordsException">The student does not exist or the service is down.</exception>
    Task<IReadOnlyList<Enrolment>> GetEnrolmentsAsync(int studentId);

    /// <summary>Gives a student a place in a course.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The new enrolment.</returns>
    /// <exception cref="StudentRecordsException">
    /// The student or course does not exist, the student is already enrolled, the course is full, the class time
    /// clashes with another course of the student, or the service is down.
    /// </exception>
    Task<Enrolment> EnrolAsync(int studentId, string courseCode);

    /// <summary>Takes a student out of a course. The student's grades for the course are lost.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>A task that completes when the student is out of the course.</returns>
    /// <exception cref="StudentRecordsException">The student or course does not exist, the student is not enrolled, or the service is down.</exception>
    Task DropAsync(int studentId, string courseCode);

    /// <summary>Records that a student handed in an assignment.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="assignmentId">The identifier of the assignment.</param>
    /// <returns>The submission; <see cref="Submission.IsLate"/> is set when it arrives after the due date.</returns>
    /// <exception cref="StudentRecordsException">The student or assignment does not exist, the student is not enrolled in the course, or the service is down.</exception>
    Task<Submission> SubmitAssignmentAsync(int studentId, int assignmentId);

    /// <summary>Records the score a student received for an assignment, replacing an earlier score.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="assignmentId">The identifier of the assignment.</param>
    /// <param name="score">The score, from 0 to the assignment's maximum.</param>
    /// <returns>The recorded grade.</returns>
    /// <exception cref="StudentRecordsException">The student or assignment does not exist, the student is not enrolled in the course, the score is out of range, or the service is down.</exception>
    Task<Grade> RecordGradeAsync(int studentId, int assignmentId, decimal score);
}
