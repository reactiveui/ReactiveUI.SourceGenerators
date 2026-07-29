// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides a secondary partial view-model command sample.</summary>
/// <seealso cref="ReactiveUI.Reactive.ReactiveObject" />
[ExcludeFromCodeCoverage]
public partial class TestViewModel
{
    /// <summary>Returns the secondary generated command value.</summary>
    /// <returns>The generated point.</returns>
    [ReactiveCommand]
    private static Point Test2() => default;
}
