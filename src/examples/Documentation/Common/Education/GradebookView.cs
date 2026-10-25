// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>
/// The gradebook screen: a course list, the roster of the selected course, the assignments and a grade entry
/// area. A MAUI page builds these controls from markup; here the view creates them in code.
/// </summary>
[System.Diagnostics.DebuggerDisplay("GradebookView: ViewModel = {ViewModel}")]
public sealed class GradebookView : ObservableObject, IViewFor<GradebookViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public GradebookViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the list of courses.</summary>
    public CollectionView CourseList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the list of students in the selected course.</summary>
    public CollectionView RosterList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the list of students who could join the selected course.</summary>
    public CollectionView CandidateList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the list of assignments of the selected course.</summary>
    public CollectionView AssignmentList { get; } = new() { SelectionMode = SelectionMode.Single };

    /// <summary>Gets the button that enrols the chosen candidate.</summary>
    public Button EnrolButton { get; } = new() { Text = "Enrol" };

    /// <summary>Gets the button that drops the selected student.</summary>
    public Button DropButton { get; } = new() { Text = "Drop" };

    /// <summary>Gets the entry where the teacher types a score.</summary>
    public Entry ScoreEntry { get; } = new() { Placeholder = "Score" };

    /// <summary>Gets the button that records the score.</summary>
    public Button RecordGradeButton { get; } = new() { Text = "Record grade" };

    /// <summary>Gets the label that shows the name of the selected student.</summary>
    public Label StudentNameLabel { get; } = new();

    /// <summary>Gets the label that shows the weighted average of the selected student.</summary>
    public Label AverageLabel { get; } = new();

    /// <summary>Gets the bar that shows the weighted average of the selected student. Its <see cref="ProgressBar.Progress"/> runs from 0 to 1.</summary>
    public ProgressBar AverageBar { get; } = new();

    /// <summary>Gets the box that is ticked while the selected student passes.</summary>
    public CheckBox PassCheckBox { get; } = new();

    /// <summary>Gets the label that shows the last error.</summary>
    public Label ErrorLabel { get; } = new();

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (GradebookViewModel?)value;
    }
}
