// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Identifies the ReactiveUI implementation API selected by a compilation.</summary>
internal enum ReactiveUiApi
{
    /// <summary>ReactiveUI releases before the package split.</summary>
    Legacy,

    /// <summary>The ReactiveUI 24 or later Primitives-based package.</summary>
    Primitives,

    /// <summary>The ReactiveUI 24 or later System.Reactive-based package.</summary>
    SystemReactive,
}
