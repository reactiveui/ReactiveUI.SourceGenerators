// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Mechanisms;

/// <summary>A to-do item being edited. It reports each change of its title and notes twice: before it is applied and after.</summary>
[System.Diagnostics.DebuggerDisplay("DraftTodo: Title = {Title}")]
public sealed class DraftTodo : ObservableObject, System.ComponentModel.INotifyPropertyChanging
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

            PropertyChanging?.Invoke(this, new System.ComponentModel.PropertyChangingEventArgs(nameof(Title)));
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

            PropertyChanging?.Invoke(this, new System.ComponentModel.PropertyChangingEventArgs(nameof(Notes)));
            _ = SetProperty(ref field, value);
        }
    } = string.Empty;
}
