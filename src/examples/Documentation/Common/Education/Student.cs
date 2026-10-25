// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Education;

/// <summary>A student. Each property raises <c>PropertyChanged</c>, and <see cref="Enrolments"/> is replaced whenever the records change.</summary>
[System.Diagnostics.DebuggerDisplay("Student: {Id}: {Name}")]
public sealed class Student : ObservableObject
{
    /// <summary>Gets or sets the identifier of the student.</summary>
    public int Id
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets or sets the name of the student.</summary>
    public string Name
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the email address of the student.</summary>
    public string Email
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>Gets or sets the courses the student takes.</summary>
    public IReadOnlyList<Enrolment> Enrolments
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    /// <summary>Creates an independent copy, as the records service returns a new object for each response.</summary>
    /// <returns>A copy with the same values.</returns>
    public Student Clone() => new() { Id = Id, Name = Name, Email = Email, Enrolments = Enrolments.ToList() };
}
