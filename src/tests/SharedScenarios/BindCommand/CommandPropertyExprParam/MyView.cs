// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;
using ReactiveUI.Binding;

namespace SharedScenarios.BindCommand.CommandPropertyExprParam;

/// <summary>View containing a WPF-like button with Command property.</summary>
#pragma warning disable CS0067 // Event is never used
public class MyView : IViewFor, INotifyPropertyChanged
{
    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public object? ViewModel { get; set; }

    /// <summary>Gets the save button.</summary>
    public WpfLikeButton SaveButton { get; } = new WpfLikeButton();
}
#pragma warning restore CS0067
