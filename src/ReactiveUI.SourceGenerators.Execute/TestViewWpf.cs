// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using Splat;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestView.
/// </summary>
[IViewFor<TestViewModel>(RegistrationType = SplatRegistrationType.PerRequest, ViewModelRegistrationType = SplatRegistrationType.Constant)]
public partial class TestViewWpf : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestViewWpf"/> class.
    /// </summary>
    public TestViewWpf()
    {
        AppLocator.CurrentMutable.RegisterLazySingleton<IViewFor<TestViewModel>>(() => new TestViewWpf());
        ViewModel = TestViewModel.Instance;
    }

    /// <summary>
    /// Gets or sets the test property.
    /// </summary>
    /// <value>
    /// The test property.
    /// </value>
    public int TestProperty { get; set; }
}
