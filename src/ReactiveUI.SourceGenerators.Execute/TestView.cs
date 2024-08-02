﻿// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Windows;
using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestView.
/// </summary>
[IViewFor(nameof(TestViewModel))]
public partial class TestView : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestView"/> class.
    /// </summary>
    public TestView()
    {
        ViewModel = TestViewModel.Instance;
    }
}
