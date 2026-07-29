// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ReactiveUI.SourceGenerators;

namespace SGReactiveUI.SourceGenerators.Test;

/// <summary>Provides a generic reactive view-model sample.</summary>
/// <typeparam name="T">the type.</typeparam>
/// <seealso cref="ReactiveUI.Reactive.ReactiveObject" />
[ExcludeFromCodeCoverage]
public partial class TestViewModel2<T> : ReactiveObject
{
    /// <summary>Stores whether the sample state is true.</summary>
    [Reactive]
    private bool _isTrue;
}
