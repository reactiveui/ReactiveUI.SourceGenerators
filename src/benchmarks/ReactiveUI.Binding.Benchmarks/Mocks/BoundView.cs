// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Owns the control and interaction handlers exercised by generated binding benchmarks.</summary>
public sealed class BoundView : IViewFor<BoundViewModel>
{
    /// <summary>Gets the command-bearing control with directly accessible properties.</summary>
    public BoundCommandControl Button { get; } = new();

    /// <inheritdoc/>
    public BoundViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (BoundViewModel?)value;
    }
}
