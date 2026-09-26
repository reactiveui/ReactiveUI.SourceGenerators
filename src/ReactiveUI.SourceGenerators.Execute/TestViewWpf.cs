// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using ReactiveUI;
using Splat;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides the WPF test view.</summary>
public class TestViewWpf : Window, IViewFor<TestViewModel>
{
    /// <summary>Initializes a new instance of the <see cref="TestViewWpf"/> class.</summary>
    public TestViewWpf()
    {
        AppLocator.CurrentMutable.RegisterLazySingleton<IViewFor<TestViewModel>>(static () => new TestViewWpf());
        ViewModel = TestViewModel.Instance;
    }

    /// <summary>Gets or sets the test property.</summary>
    /// <value>
    /// The test property.
    /// </value>
    public int TestProperty { get; set; }

    /// <inheritdoc/>
    public TestViewModel? ViewModel { get; set; }

    /// <inheritdoc/>
    object? IViewFor.ViewModel { get => ViewModel; set => ViewModel = (TestViewModel?)value; }
}
