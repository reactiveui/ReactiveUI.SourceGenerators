// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Properties;

/// <summary>
/// Declares its read-only properties with <see cref="ObservableAsPropertyAttribute"/> instead of writing the body
/// and the backing field by hand. Each property has to be <see langword="partial"/> and get-only, so every
/// generator in the build can see it; the source generator writes the body and a field named
/// <c>_{name}Helper</c>, which the constructor assigns with <c>ToProperty</c>.
/// </summary>
[System.Diagnostics.DebuggerDisplay("SummaryViewModel: Title = {Title}, IsDone = {IsDone}")]
public sealed partial class SummaryViewModel : INotifyPropertyChanged
{
    /// <summary>Initializes a new instance of the <see cref="SummaryViewModel"/> class.</summary>
    /// <param name="item">The item whose title and completion state the view model follows.</param>
    public SummaryViewModel(TodoItem item)
    {
        _titleHelper = item.WhenChanged(static x => x.Title).ToProperty(this, static x => x.Title);
        _isDoneHelper = item.WhenChanged(static x => x.IsDone).ToProperty(this, static x => x.IsDone);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets the title of the followed item.</summary>
    [ObservableAsProperty]
    public partial string Title { get; }

    /// <summary>Gets a value indicating whether the followed item is finished.</summary>
    [ObservableAsProperty]
    public partial bool IsDone { get; }
}
