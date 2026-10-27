// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.GeneratedCode.TestModels.TestModels;

/// <summary>An open generic view base that closed subclasses derive from.</summary>
/// <typeparam name="TViewModel">The view model type the view displays.</typeparam>
public class TodoViewBase<TViewModel> : IViewFor<TViewModel>
    where TViewModel : class
{
    /// <inheritdoc/>
    public TViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }
}
