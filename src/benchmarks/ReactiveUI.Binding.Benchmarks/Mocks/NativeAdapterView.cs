// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.Binding.Benchmarks.Mocks;

/// <summary>Exposes native control contracts to the generated bindings.</summary>
public sealed class NativeAdapterView : IViewFor<BenchmarkViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="NativeAdapterView"/> class.</summary>
    public NativeAdapterView() => Controls = new(new());

    /// <inheritdoc/>
    public BenchmarkViewModel? ViewModel { get; set; }

    /// <summary>Gets the native click target.</summary>
    public Android.Views.View Button { get; } = new();

    /// <summary>Gets the collection receiving typed control arrays.</summary>
    public System.Windows.Forms.Control.ControlCollection Controls { get; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (BenchmarkViewModel?)value;
    }
}
