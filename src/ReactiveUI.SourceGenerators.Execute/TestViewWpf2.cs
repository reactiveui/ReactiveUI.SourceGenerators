// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestViewWpf2.
/// </summary>
/// <seealso cref="System.Windows.Window" />
[IViewFor("TestViewModel2<int>")]
public partial class TestViewWpf2 : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestViewWpf2"/> class.
    /// </summary>
    public TestViewWpf2() => ViewModel = new();
}
