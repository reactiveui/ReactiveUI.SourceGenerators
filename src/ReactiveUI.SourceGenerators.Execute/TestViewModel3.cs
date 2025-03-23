﻿// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
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

    [Reactive]
    private float _testVM3Property2;

    [ReactiveCommand]
    private int Test1() => 10;

    /// <summary>
    /// TestInnerClass.
    /// </summary>
    public partial class TestInnerClass1 : ReactiveObject
    {
        [Reactive]
        private int _testInner1;

        [Reactive]
        private int _testInner11;

        [ReactiveCommand]
        private int TestI1() => 10;
    }

    /// <summary>
    /// TestInnerClass.
    /// </summary>
    public partial class TestInnerClass2 : ReactiveObject
    {
        [Reactive]
        private int _testInner2;

        [Reactive]
        private int _testInner22;

        [ReactiveCommand]
        private int TestI2() => 10;

        /// <summary>
        /// TestInnerClass4.
        /// </summary>
        /// <seealso cref="ReactiveUI.ReactiveObject" />
        public partial class TestInnerClass3 : ReactiveObject
        {
            [Reactive]
            private int _testInner3;

            [Reactive]
            private int _testInner33;

            [ReactiveCommand]
            private int TestI3() => 10;
        }
    }
}
