// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>
/// A view model that declares its own field-like <see cref="PropertyChanged"/> event. Only code inside the type can
/// invoke that event, so the generator adds a small member of its own; that only works while the type stays
/// <see langword="partial"/>. This backs <see cref="Title"/> with the selector overload of <c>ToProperty</c>, and
/// <see cref="Notes"/> with the overload that also returns the helper through an <see langword="out"/> parameter.
/// </summary>
[System.Diagnostics.DebuggerDisplay("PartialOwnEventViewModel: Title = {Title}")]
public sealed partial class PartialOwnEventViewModel : INotifyPropertyChanged
{
    /// <summary>Backs <see cref="Notes"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _notesHelper;

    /// <summary>Backs <see cref="Title"/>.</summary>
    private readonly ObservableAsPropertyHelper<string> _titleHelper;

    /// <summary>Initializes a new instance of the <see cref="PartialOwnEventViewModel"/> class.</summary>
    /// <param name="item">The item whose title and notes the view model follows.</param>
    public PartialOwnEventViewModel(TodoItem item)
    {
        _titleHelper = item.WhenChanged(static x => x.Title).ToProperty(this, static x => x.Title);
        _ = item.WhenChanged(static x => x.Notes).ToProperty(this, static x => x.Notes, out _notesHelper);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the title of the followed item.</summary>
    public string Title => _titleHelper.Value;

    /// <summary>Gets the notes of the followed item.</summary>
    public string Notes => _notesHelper.Value;
}
