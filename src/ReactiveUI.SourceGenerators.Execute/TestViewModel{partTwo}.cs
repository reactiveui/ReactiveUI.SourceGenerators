// Copyright (c) 2025 ReactiveUI and contributors. All rights reserved.
// Licensed to the ReactiveUI and contributors under one or more agreements.
// The ReactiveUI and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test
{
    /// <summary>
    /// TestViewModel.
    /// </summary>
    /// <seealso cref="ReactiveUI.ReactiveObject" />
    [ExcludeFromCodeCoverage]
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
