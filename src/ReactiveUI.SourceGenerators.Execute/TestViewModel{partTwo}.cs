// Copyright (c) 2025 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test
{
    /// <summary>
    /// TestViewModel.
    /// </summary>
    /// <seealso cref="ReactiveUI.ReactiveObject" />
    public partial class TestViewModel
    {
        /// <summary>
        /// Test2s this instance.
        /// </summary>
        /// <returns>Rectangle.</returns>
        [ReactiveCommand]
        private Point Test2() => default;
    }
}
