// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>
/// The view model behind the gradebook. It shows one course at a time: the roster, the assignments and, for the
/// selected student, a weighted average and a pass or fail flag. Each command starts one of the asynchronous
/// methods and returns; call the method itself to wait for the work. A refused request never throws. It sets
/// <see cref="ErrorMessage"/>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("GradebookViewModel: Course = {SelectedCourse}, Student = {SelectedStudent}, Average = {Average}")]
public sealed class GradebookViewModel : ObservableObject
{
    /// <summary>The service the view model calls.</summary>
    private readonly IStudentRecords _records;

    /// <summary>Initializes a new instance of the <see cref="GradebookViewModel"/> class.</summary>
    /// <param name="records">The service to call.</param>
    public GradebookViewModel(IStudentRecords records)
    {
        _records = records;
        LoadCoursesCommand = new(() => _ = LoadCoursesAsync());
        OpenCourseCommand = new(() => _ = OpenCourseAsync(), () => SelectedCourse is not null);
        EnrolCommand = new(candidate => _ = EnrolAsync(candidate), candidate => candidate is not null && SelectedCourse is not null);
        DropCommand = new(() => _ = DropAsync(), () => SelectedCourse is not null && SelectedStudent is not null);
        RecordGradeCommand = new(() => _ = RecordGradeAsync(), () => SelectedStudent is not null && SelectedAssignment is not null);
    }

    /// <summary>Gets the question the view answers before a student is dropped. The answer is <see langword="true"/> to drop the student.</summary>
    public Interaction<DropRequest, bool> ConfirmDrop { get; } = new();

    /// <summary>Gets the command that runs <see cref="LoadCoursesAsync"/>.</summary>
    public Command LoadCoursesCommand { get; }

    /// <summary>Gets the command that runs <see cref="OpenCourseAsync"/>.</summary>
    public Command OpenCourseCommand { get; }

    /// <summary>Gets the command that runs <see cref="EnrolAsync"/>. Its parameter is the <see cref="Student"/> to enrol.</summary>
    public Command<Student> EnrolCommand { get; }

    /// <summary>Gets the command that runs <see cref="DropAsync"/>.</summary>
    public Command DropCommand { get; }

    /// <summary>Gets the command that runs <see cref="RecordGradeAsync"/>.</summary>
    public Command RecordGradeCommand { get; }

    /// <summary>Gets the courses.</summary>
    public IReadOnlyList<Course> Courses
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the course the gradebook shows.</summary>
    public Course? SelectedCourse
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            OpenCourseCommand.ChangeCanExecute();
            EnrolCommand.ChangeCanExecute();
            DropCommand.ChangeCanExecute();
        }
    }

    /// <summary>Gets the students who take <see cref="SelectedCourse"/>.</summary>
    public IReadOnlyList<Student> Roster
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets the students who could join <see cref="SelectedCourse"/>.</summary>
    public IReadOnlyList<Student> Candidates
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets the assignments of <see cref="SelectedCourse"/>.</summary>
    public IReadOnlyList<Assignment> Assignments
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    /// <summary>Gets or sets the student the teacher picked. A path such as <c>SelectedStudent.Enrolments</c> is empty while nobody is picked.</summary>
    public Student? SelectedStudent
    {
        get;
        set
        {
            if (!SetProperty(ref field, value))
            {
                return;
            }

            DropCommand.ChangeCanExecute();
            RecordGradeCommand.ChangeCanExecute();
            RecomputeAverage();
        }
    }

    /// <summary>Gets or sets the assignment the teacher picked.</summary>
    public Assignment? SelectedAssignment
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                RecordGradeCommand.ChangeCanExecute();
            }
        }
    }

    /// <summary>Gets or sets the score the teacher typed.</summary>
    public decimal ScoreToRecord
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the weighted average of the selected student in the selected course, as a percentage from 0 to 100. It is 0 until a score is recorded.</summary>
    public decimal Average
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets a value indicating whether <see cref="Average"/> reaches the pass mark of the selected course.</summary>
    public bool IsPassing
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>Gets the message from the last refused request, or an empty string.</summary>
    public string ErrorMessage
    {
        get;
        private set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Lists the courses.</summary>
    /// <returns>A task that completes when the courses are listed or the request was refused.</returns>
    public async Task LoadCoursesAsync()
    {
        try
        {
            Courses = await _records.GetCoursesAsync().ConfigureAwait(false);
            ErrorMessage = string.Empty;
        }
        catch (StudentRecordsException ex)
        {
            Refused(ex);
        }
    }

    /// <summary>Loads the roster, the candidates and the assignments of <see cref="SelectedCourse"/>.</summary>
    /// <returns>A task that completes when the course is loaded or the request was refused.</returns>
    public async Task OpenCourseAsync()
    {
        if (SelectedCourse is not { } course)
        {
            return;
        }

        try
        {
            await LoadCourseAsync(course).ConfigureAwait(false);
            ErrorMessage = string.Empty;
        }
        catch (StudentRecordsException ex)
        {
            Refused(ex);
        }
    }

    /// <summary>Enrols a student in <see cref="SelectedCourse"/>.</summary>
    /// <param name="candidate">The student to enrol.</param>
    /// <returns>A task that completes when the student is enrolled or the request was refused.</returns>
    public async Task EnrolAsync(Student candidate)
    {
        if (SelectedCourse is not { } course)
        {
            return;
        }

        try
        {
            _ = await _records.EnrolAsync(candidate.Id, course.Code).ConfigureAwait(false);
            await LoadCourseAsync(course).ConfigureAwait(false);
            ErrorMessage = string.Empty;
        }
        catch (StudentRecordsException ex)
        {
            Refused(ex);
        }
    }

    /// <summary>Asks the view to confirm through <see cref="ConfirmDrop"/>, then drops <see cref="SelectedStudent"/> from <see cref="SelectedCourse"/>.</summary>
    /// <returns>A task that completes when the student is dropped, the teacher declined or the request was refused.</returns>
    public async Task DropAsync()
    {
        if (SelectedCourse is not { } course || SelectedStudent is not { } student)
        {
            return;
        }

        if (!await ConfirmDrop.Handle(new(student, course)).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            await _records.DropAsync(student.Id, course.Code).ConfigureAwait(false);
            await LoadCourseAsync(course).ConfigureAwait(false);
            ErrorMessage = string.Empty;
        }
        catch (StudentRecordsException ex)
        {
            Refused(ex);
        }
    }

    /// <summary>Records <see cref="ScoreToRecord"/> for <see cref="SelectedStudent"/> on <see cref="SelectedAssignment"/>.</summary>
    /// <returns>A task that completes when the score is recorded or the request was refused.</returns>
    public async Task RecordGradeAsync()
    {
        if (SelectedStudent is not { } student || SelectedAssignment is not { } assignment)
        {
            return;
        }

        try
        {
            _ = await _records.RecordGradeAsync(student.Id, assignment.Id, ScoreToRecord).ConfigureAwait(false);
            student.Enrolments = await _records.GetEnrolmentsAsync(student.Id).ConfigureAwait(false);
            ErrorMessage = string.Empty;
            RecomputeAverage();
        }
        catch (StudentRecordsException ex)
        {
            Refused(ex);
        }
    }

    /// <summary>Finds the enrolment of a student in a course.</summary>
    /// <param name="student">The student.</param>
    /// <param name="courseCode">The code of the course.</param>
    /// <returns>The enrolment, or <see langword="null"/> when the student is not enrolled.</returns>
    private static Enrolment? FindEnrolment(Student student, string courseCode) =>
        student.Enrolments.FirstOrDefault(enrolment => enrolment.CourseCode == courseCode);

    /// <summary>Finds a student in a list.</summary>
    /// <param name="students">The list to search.</param>
    /// <param name="id">The identifier of the student.</param>
    /// <returns>The student, or <see langword="null"/> when the list does not hold the student.</returns>
    private static Student? FindStudent(IReadOnlyList<Student> students, int? id) =>
        students.FirstOrDefault(student => student.Id == id);

    /// <summary>Lists the students who are not on a roster.</summary>
    /// <param name="students">Every student.</param>
    /// <param name="roster">The students already on the roster.</param>
    /// <returns>The students who could join.</returns>
    private static List<Student> NotOn(IReadOnlyList<Student> students, IReadOnlyList<Student> roster) =>
        students.Where(student => FindStudent(roster, student.Id) is null).ToList();

    /// <summary>Loads the roster, the candidates and the assignments of a course, keeping the selected student when the student is still on the roster.</summary>
    /// <param name="course">The course to load.</param>
    /// <returns>A task that completes when the course is loaded.</returns>
    private async Task LoadCourseAsync(Course course)
    {
        int? selectedId = SelectedStudent?.Id;
        IReadOnlyList<Student> roster = await _records.GetRosterAsync(course.Code).ConfigureAwait(false);
        IReadOnlyList<Student> students = await _records.GetStudentsAsync().ConfigureAwait(false);
        IReadOnlyList<Assignment> assignments = await _records.GetAssignmentsAsync(course.Code).ConfigureAwait(false);

        Roster = roster;
        Candidates = NotOn(students, roster);
        Assignments = assignments;
        SelectedAssignment = null;
        SelectedStudent = FindStudent(roster, selectedId);
        RecomputeAverage();
    }

    /// <summary>Works out the average and the pass flag of the selected student in the selected course.</summary>
    private void RecomputeAverage()
    {
        Enrolment? enrolment = SelectedStudent is { } student && SelectedCourse is { } course ? FindEnrolment(student, course.Code) : null;
        if (enrolment is null || enrolment.Grades.Count == 0)
        {
            Average = 0;
            IsPassing = false;
            return;
        }

        Average = GradeCalculator.WeightedAverage(Assignments, enrolment.Grades);
        IsPassing = Average >= SelectedCourse!.PassMark;
    }

    /// <summary>Shows why the service refused a request.</summary>
    /// <param name="ex">The refusal.</param>
    private void Refused(StudentRecordsException ex) => ErrorMessage = $"{ex.Failure}: {ex.Message}";
}
