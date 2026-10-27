// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>
/// A student records service that lives in memory. It refuses a request the way a real registrar does: a full
/// course, a timetable clash, a score out of range. <see cref="Now"/> stamps submissions and grades, and
/// <see cref="Latency"/> sets how long each response takes. It hands out copies, so changing a student you read
/// does not change the records.
/// </summary>
[System.Diagnostics.DebuggerDisplay("InMemoryStudentRecords: Students = {_students.Count}, Enrolments = {_enrolments.Count}, IsUnavailable = {IsUnavailable}")]
public sealed class InMemoryStudentRecords : IStudentRecords
{
    /// <summary>The courses a new service starts with. CS101 has three places, and HIST110 meets while CS101 does.</summary>
    private static readonly Course[] _seedCourses =
    [
        new("CS101", "Introduction to Programming", 3, DayOfWeek.Tuesday, 9, 2, 50),
        new("MATH201", "Linear Algebra", 30, DayOfWeek.Wednesday, 10, 2, 50),
        new("HIST110", "Modern European History", 30, DayOfWeek.Tuesday, 10, 2, 50),
    ];

    /// <summary>The assignments a new service starts with.</summary>
    private static readonly Assignment[] _seedAssignments =
    [
        new(1, "CS101", "Loops and functions", 20, 100, new(2026, 3, 6)),
        new(2, "CS101", "Text adventure", 30, 100, new(2026, 3, 27)),
        new(3, "CS101", "Final project", 50, 100, new(2026, 5, 29)),
        new(4, "MATH201", "Problem set 1", 40, 50, new(2026, 3, 13)),
        new(5, "MATH201", "Midterm", 60, 100, new(2026, 4, 10)),
        new(6, "HIST110", "Essay on the 1848 revolutions", 100, 100, new(2026, 4, 17)),
    ];

    /// <summary>The students a new service starts with.</summary>
    private static readonly (int Id, string Name)[] _seedStudents =
    [
        (1, "Aisha Rahman"),
        (2, "Ben Carter"),
        (3, "Chloe Nguyen"),
        (4, "Diego Alvarez"),
        (5, "Emma Kowalski"),
    ];

    /// <summary>The enrolments a new service starts with: Aisha, Ben and Chloe fill CS101.</summary>
    private static readonly Enrolment[] _seedEnrolments =
    [
        new(
            1,
            "CS101",
            new(2026, 2, 16),
            new List<Grade> { new(1, 1, 85, new(2026, 3, 9, 12, 0, 0, TimeSpan.Zero)), new(1, 2, 72, new(2026, 3, 30, 12, 0, 0, TimeSpan.Zero)) },
            new List<Submission> { new(1, 1, new(2026, 3, 5, 10, 0, 0, TimeSpan.Zero), false) }),
        new(2, "CS101", new(2026, 2, 16), new List<Grade> { new(2, 1, 40, new(2026, 3, 9, 12, 0, 0, TimeSpan.Zero)), new(2, 2, 35, new(2026, 3, 30, 12, 0, 0, TimeSpan.Zero)) }, []),
        new(3, "CS101", new(2026, 2, 17), [], []),
        new(3, "MATH201", new(2026, 2, 17), new List<Grade> { new(3, 4, 45, new(2026, 3, 16, 12, 0, 0, TimeSpan.Zero)) }, []),
        new(4, "HIST110", new(2026, 2, 18), [], []),
        new(5, "MATH201", new(2026, 2, 18), new List<Grade> { new(5, 4, 30, new(2026, 3, 16, 12, 0, 0, TimeSpan.Zero)) }, []),
    ];

    /// <summary>The courses, by code.</summary>
    private readonly List<Course> _courses = [.. _seedCourses];

    /// <summary>The assignments, by identifier.</summary>
    private readonly List<Assignment> _assignments = [.. _seedAssignments];

    /// <summary>The students, without their enrolments.</summary>
    private readonly List<Student> _students = [];

    /// <summary>The enrolments of every student. An enrolment is replaced, not changed, when its grades or submissions change.</summary>
    private readonly List<Enrolment> _enrolments = [.. _seedEnrolments];

    /// <summary>Gets or sets how long each call takes. A call yields once when the latency is zero.</summary>
    public TimeSpan Latency { get; set; }

    /// <summary>Gets or sets the moment that stamps submissions and grades, and that decides whether a submission is late.</summary>
    public DateTimeOffset Now { get; set; } = new(2026, 3, 3, 9, 0, 0, TimeSpan.Zero);

    /// <summary>Gets or sets a value indicating whether every call fails with <see cref="RecordsFailure.ServiceUnavailable"/>.</summary>
    public bool IsUnavailable { get; set; }

    /// <summary>Creates a service with three courses, five students and six assignments.</summary>
    /// <returns>A new service.</returns>
    public static InMemoryStudentRecords CreateSeeded()
    {
        InMemoryStudentRecords records = new();
        foreach (var (id, name) in _seedStudents)
        {
            records._students.Add(new() { Id = id, Name = name, Email = $"student{id}@example.edu" });
        }

        return records;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Course>> GetCoursesAsync()
    {
        await EnterAsync().ConfigureAwait(false);

        return _courses.ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Student>> GetStudentsAsync()
    {
        await EnterAsync().ConfigureAwait(false);

        List<Student> students = [];
        foreach (Student student in _students)
        {
            students.Add(Snapshot(student));
        }

        return students;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Student>> GetRosterAsync(string courseCode)
    {
        await EnterAsync().ConfigureAwait(false);

        _ = FindCourse(courseCode);
        List<Student> roster = [];
        foreach (Student student in _students)
        {
            if (IndexOfEnrolment(student.Id, courseCode) >= 0)
            {
                roster.Add(Snapshot(student));
            }
        }

        return roster;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Assignment>> GetAssignmentsAsync(string courseCode)
    {
        await EnterAsync().ConfigureAwait(false);

        _ = FindCourse(courseCode);
        List<Assignment> assignments = [];
        foreach (Assignment assignment in _assignments)
        {
            if (assignment.CourseCode == courseCode)
            {
                assignments.Add(assignment);
            }
        }

        return assignments;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Enrolment>> GetEnrolmentsAsync(int studentId)
    {
        await EnterAsync().ConfigureAwait(false);

        return EnrolmentsOf(FindStudent(studentId).Id);
    }

    /// <inheritdoc/>
    public async Task<Enrolment> EnrolAsync(int studentId, string courseCode)
    {
        await EnterAsync().ConfigureAwait(false);

        Student student = FindStudent(studentId);
        Course course = FindCourse(courseCode);

        if (IndexOfEnrolment(student.Id, course.Code) >= 0)
        {
            throw new StudentRecordsException(RecordsFailure.AlreadyEnrolled, $"{student.Name} is already enrolled in {course.Code}.");
        }

        if (CountEnrolled(course.Code) >= course.Capacity)
        {
            throw new StudentRecordsException(RecordsFailure.CourseFull, $"{course.Code} has no places left.");
        }

        foreach (Enrolment existing in EnrolmentsOf(student.Id))
        {
            Course other = FindCourse(existing.CourseCode);
            if (course.ClashesWith(other))
            {
                throw new StudentRecordsException(RecordsFailure.TimetableClash, $"{course.Code} meets at the same time as {other.Code}.");
            }
        }

        Enrolment enrolment = new(student.Id, course.Code, Today(), [], []);
        _enrolments.Add(enrolment);
        return enrolment;
    }

    /// <inheritdoc/>
    public async Task DropAsync(int studentId, string courseCode)
    {
        await EnterAsync().ConfigureAwait(false);

        Student student = FindStudent(studentId);
        Course course = FindCourse(courseCode);
        int index = IndexOfEnrolment(student.Id, course.Code);
        if (index < 0)
        {
            throw new StudentRecordsException(RecordsFailure.NotEnrolled, $"{student.Name} is not enrolled in {course.Code}.");
        }

        _enrolments.RemoveAt(index);
    }

    /// <inheritdoc/>
    public async Task<Submission> SubmitAssignmentAsync(int studentId, int assignmentId)
    {
        await EnterAsync().ConfigureAwait(false);

        Assignment assignment = FindAssignment(assignmentId);
        int index = RequireEnrolment(studentId, assignment.CourseCode);
        Enrolment enrolment = _enrolments[index];

        Submission submission = new(studentId, assignmentId, Now, Today() > assignment.DueDate);
        List<Submission> submissions = [.. enrolment.Submissions];
        _ = submissions.RemoveAll(existing => existing.AssignmentId == assignmentId);
        submissions.Add(submission);
        _enrolments[index] = enrolment with { Submissions = submissions };
        return submission;
    }

    /// <inheritdoc/>
    public async Task<Grade> RecordGradeAsync(int studentId, int assignmentId, decimal score)
    {
        await EnterAsync().ConfigureAwait(false);

        Assignment assignment = FindAssignment(assignmentId);
        int index = RequireEnrolment(studentId, assignment.CourseCode);
        if (score < 0 || score > assignment.MaxScore)
        {
            throw new StudentRecordsException(RecordsFailure.ScoreOutOfRange, $"A score for '{assignment.Title}' must be between 0 and {assignment.MaxScore}.");
        }

        Enrolment enrolment = _enrolments[index];
        Grade grade = new(studentId, assignmentId, score, Now);
        List<Grade> grades = [.. enrolment.Grades];
        _ = grades.RemoveAll(existing => existing.AssignmentId == assignmentId);
        grades.Add(grade);
        _enrolments[index] = enrolment with { Grades = grades };
        return grade;
    }

    /// <summary>Waits for <see cref="Latency"/>, then fails when the service is unavailable.</summary>
    /// <returns>A task that completes when the call may proceed.</returns>
    /// <exception cref="StudentRecordsException">The service is unavailable.</exception>
    private async Task EnterAsync()
    {
        if (Latency == TimeSpan.Zero)
        {
            await Task.Yield();
        }
        else
        {
            await Task.Delay(Latency).ConfigureAwait(false);
        }

        if (IsUnavailable)
        {
            throw new StudentRecordsException(RecordsFailure.ServiceUnavailable, "The student records service is unavailable.");
        }
    }

    /// <summary>Gets the date of <see cref="Now"/>.</summary>
    /// <returns>The date in UTC.</returns>
    private DateOnly Today() => DateOnly.FromDateTime(Now.UtcDateTime);

    /// <summary>Copies a student together with the courses the student takes.</summary>
    /// <param name="student">The stored student.</param>
    /// <returns>An independent copy.</returns>
    private Student Snapshot(Student student)
    {
        Student copy = student.Clone();
        copy.Enrolments = EnrolmentsOf(student.Id);
        return copy;
    }

    /// <summary>Lists the enrolments of a student.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <returns>The enrolments, in the order they were made.</returns>
    private List<Enrolment> EnrolmentsOf(int studentId)
    {
        List<Enrolment> enrolments = [];
        foreach (Enrolment enrolment in _enrolments)
        {
            if (enrolment.StudentId == studentId)
            {
                enrolments.Add(enrolment);
            }
        }

        return enrolments;
    }

    /// <summary>Finds where an enrolment is stored.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The index of the enrolment, or -1 when the student is not enrolled.</returns>
    private int IndexOfEnrolment(int studentId, string courseCode) =>
        _enrolments.FindIndex(enrolment => enrolment.StudentId == studentId && enrolment.CourseCode == courseCode);

    /// <summary>Finds where an enrolment is stored, and refuses when there is none.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The index of the enrolment.</returns>
    /// <exception cref="StudentRecordsException">The student does not exist or is not enrolled.</exception>
    private int RequireEnrolment(int studentId, string courseCode)
    {
        Student student = FindStudent(studentId);
        int index = IndexOfEnrolment(student.Id, courseCode);
        return index >= 0
            ? index
            : throw new StudentRecordsException(RecordsFailure.NotEnrolled, $"{student.Name} is not enrolled in {courseCode}.");
    }

    /// <summary>Counts the students enrolled in a course.</summary>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The number of enrolments.</returns>
    private int CountEnrolled(string courseCode)
    {
        int count = 0;
        foreach (Enrolment enrolment in _enrolments)
        {
            if (enrolment.CourseCode == courseCode)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>Finds a course.</summary>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The course.</returns>
    /// <exception cref="StudentRecordsException">The course does not exist.</exception>
    private Course FindCourse(string courseCode) =>
        _courses.Find(course => course.Code == courseCode)
        ?? throw new StudentRecordsException(RecordsFailure.UnknownCourse, $"There is no course {courseCode}.");

    /// <summary>Finds a student.</summary>
    /// <param name="studentId">The identifier of the student.</param>
    /// <returns>The stored student.</returns>
    /// <exception cref="StudentRecordsException">The student does not exist.</exception>
    private Student FindStudent(int studentId) =>
        _students.Find(student => student.Id == studentId)
        ?? throw new StudentRecordsException(RecordsFailure.UnknownStudent, $"There is no student {studentId}.");

    /// <summary>Finds an assignment.</summary>
    /// <param name="assignmentId">The identifier of the assignment.</param>
    /// <returns>The assignment.</returns>
    /// <exception cref="StudentRecordsException">The assignment does not exist.</exception>
    private Assignment FindAssignment(int assignmentId) =>
        _assignments.Find(assignment => assignment.Id == assignmentId)
        ?? throw new StudentRecordsException(RecordsFailure.UnknownAssignment, $"There is no assignment {assignmentId}.");
}
