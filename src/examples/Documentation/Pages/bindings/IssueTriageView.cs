// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Infrastructure;

namespace ReactiveUI.Binding.Documentation.Bindings;

/// <summary>The triage panel. It shows the confirmation dialog that appears before an issue closes.</summary>
[System.Diagnostics.DebuggerDisplay("IssueTriageView: ViewModel = {ViewModel}")]
public sealed class IssueTriageView : ObservableObject, IViewFor<IssueTriageViewModel>
{
    /// <summary>Gets or sets the view model the view shows.</summary>
    public IssueTriageViewModel? ViewModel
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (IssueTriageViewModel?)value;
    }
}
