// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestViewModel2.
/// </summary>
/// <typeparam name="T">the type.</typeparam>
/// <seealso cref="ReactiveUI.ReactiveObject" />
public partial class TestViewModel2<T> : ReactiveObject
{
    [Reactive]
    private bool _IsTrue;
}
