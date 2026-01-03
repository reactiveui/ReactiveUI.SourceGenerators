// Copyright (c) 2026 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test.Maui
{
    /// <summary>
    /// IViewForTest.
    /// </summary>
    /// <seealso cref="NavigationPage" />
    [IViewFor<TestViewModel>]
    public partial class IViewForTest : Shell;
}
