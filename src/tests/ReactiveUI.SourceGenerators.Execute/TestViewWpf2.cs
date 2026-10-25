// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using ReactiveUI;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides the generic WPF test view.</summary>
/// <seealso cref="System.Windows.Window" />
public class TestViewWpf2 : Window, IViewFor<TestViewModel2<int>>
{
    /// <summary>Initializes a new instance of the <see cref="TestViewWpf2"/> class.</summary>
    public TestViewWpf2() => ViewModel = new();

    /// <inheritdoc/>
    public TestViewModel2<int>? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (TestViewModel2<int>?)value; }
}
