// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestViewWpf2.
/// </summary>
/// <seealso cref="System.Windows.Window" />
[IViewFor("SGReactiveUI.SourceGenerators.Test.TestViewModel2<int>", RegistrationType = SplatRegistrationType.PerRequest)]
public partial class TestViewWpf2 : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestViewWpf2"/> class.
    /// </summary>
    public TestViewWpf2() => ViewModel = new();
}
