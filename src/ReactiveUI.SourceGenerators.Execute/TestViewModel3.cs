// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>
/// TestViewModel3.
/// </summary>
public partial class TestViewModel3 : ReactiveObject
{
    [Reactive]
    private float _testVM3Property;

    /////// <summary>
    /////// TestInnerClass.
    /////// </summary>
    ////public partial class TestInnerClass1 : ReactiveObject
    ////{
    ////    [Reactive]
    ////    private int _testInner1;
    ////}

    /////// <summary>
    /////// TestInnerClass.
    /////// </summary>
    ////public partial class TestInnerClass2 : ReactiveObject
    ////{
    ////    [Reactive]
    ////    private int _testInner2;

    ////    /// <summary>
    ////    /// TestInnerClass4.
    ////    /// </summary>
    ////    /// <seealso cref="ReactiveUI.ReactiveObject" />
    ////    public partial class TestInnerClass3 : ReactiveObject
    ////    {
    ////        [Reactive]
    ////        private int _testInner3;
    ////    }
    ////}
}
