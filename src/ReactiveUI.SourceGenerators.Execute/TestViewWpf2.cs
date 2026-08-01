// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides the generic WPF test view.</summary>
/// <seealso cref="System.Windows.Window" />
[IViewFor("SGReactiveUI.SourceGenerators.Test.TestViewModel2<int>", RegistrationType = SplatRegistrationType.PerRequest)]
public partial class TestViewWpf2 : Window
{
    /// <summary>Initializes a new instance of the <see cref="TestViewWpf2"/> class.</summary>
    public TestViewWpf2() => ViewModel = new();
}
