// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Maui.Controls;
using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>The statement export dialog: a single button that exports the chosen statement.</summary>
[System.Diagnostics.DebuggerDisplay("StatementExportView: ViewModel = {ViewModel}")]
public sealed class StatementExportView : ObservableObject, IViewFor<StatementExportViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public StatementExportViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <summary>Gets the button that exports the statement.</summary>
    public Button ExportButton { get; } = new() { Text = "Export" };

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (StatementExportViewModel?)value;
    }
}
