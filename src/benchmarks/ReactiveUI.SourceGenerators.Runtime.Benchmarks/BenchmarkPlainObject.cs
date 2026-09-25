// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.SourceGenerators;

namespace ReactiveUI.SourceGenerators.Runtime.Benchmarks;

/// <summary>An object that implements <c>IReactiveObject</c> through the generator rather than a base class.</summary>
[IReactiveObject]
public partial class BenchmarkPlainObject
{
    /// <summary>Backs the generated <c>Value</c> property.</summary>
    [Reactive]
    private int _value;
}
