// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Setup;

/// <summary>A to-do title that reports each change twice: <c>PropertyChanging</c> before it and <c>PropertyChanged</c> after it.</summary>
[System.Diagnostics.DebuggerDisplay("EditableTodo: Title = {Title}")]
public sealed class EditableTodo : ObservableObject, System.ComponentModel.INotifyPropertyChanging
{
    /// <inheritdoc/>
    public event System.ComponentModel.PropertyChangingEventHandler? PropertyChanging;

    /// <summary>Gets or sets the short summary of the task.</summary>
    public string Title
    {
        get;
        set
        {
            PropertyChanging?.Invoke(this, new System.ComponentModel.PropertyChangingEventArgs(nameof(Title)));
            _ = SetProperty(ref field, value);
        }
    } = string.Empty;
}
