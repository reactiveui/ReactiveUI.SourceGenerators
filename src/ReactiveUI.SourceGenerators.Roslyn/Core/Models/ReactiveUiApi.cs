// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Identifies the ReactiveUI implementation API selected by a compilation.</summary>
internal enum ReactiveUiApi
{
    /// <summary>The Primitives-based <c>ReactiveUI</c> package.</summary>
    Primitives = 0,

    /// <summary>The System.Reactive-based <c>ReactiveUI.Reactive</c> package.</summary>
    SystemReactive = 1,
}
