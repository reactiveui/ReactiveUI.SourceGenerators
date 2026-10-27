// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.UnsafeOverloads;

/// <summary>A to-do item being edited. It reports each change of its title, notes and done flag twice: before it is applied and after.</summary>
[System.Diagnostics.DebuggerDisplay("EditableTodoItem: Title = {Title}")]
public sealed class EditableTodoItem : ObservableObject, INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event System.ComponentModel.PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the short summary of the task.</summary>
    public string Title
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            RaisePropertyChanging(nameof(Title));
            _ = SetProperty(ref field, value);
        }
    } = string.Empty;

    /// <summary>Gets or sets the longer description of the task.</summary>
    public string Notes
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            RaisePropertyChanging(nameof(Notes));
            _ = SetProperty(ref field, value);
        }
    } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the task is finished.</summary>
    public bool IsDone
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            RaisePropertyChanging(nameof(IsDone));
            _ = SetProperty(ref field, value);
        }
    }

    /// <summary>Reports that a property is about to change.</summary>
    /// <param name="propertyName">The name of the property.</param>
    private void RaisePropertyChanging(string propertyName) =>
        PropertyChanging?.Invoke(this, new System.ComponentModel.PropertyChangingEventArgs(propertyName));
}
