// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;
using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>
/// The heading of the to-do screen. Its label is a named control, declared as a field in the part the markup
/// compiler generates (<c>TodoHeadingView.g.cs</c>), the way <c>x:Name="TitleLabel"</c> declares one.
/// </summary>
[System.Diagnostics.DebuggerDisplay("TodoHeadingView: ViewModel = {ViewModel}")]
public sealed partial class TodoHeadingView : ObservableObject, IViewFor<TodoListViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public TodoListViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TodoListViewModel?)value;
    }
}
