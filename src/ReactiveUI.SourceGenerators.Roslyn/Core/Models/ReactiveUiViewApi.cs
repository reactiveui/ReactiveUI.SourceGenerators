// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace ReactiveUI.SourceGenerators.Models;

/// <summary>Identifies which assembly declares the <c>IViewFor</c> interfaces and the view locator a compilation uses.</summary>
internal enum ReactiveUiViewApi
{
    /// <summary>ReactiveUI declares them in <c>ReactiveUI</c>: every release not built on ReactiveUI.Binding.</summary>
    ReactiveUI,

    /// <summary>ReactiveUI.Binding declares them in <c>ReactiveUI.Binding</c>, used by the Primitives-based ReactiveUI.</summary>
    Binding,

    /// <summary>ReactiveUI.Binding.Reactive declares them in <c>ReactiveUI.Binding.Reactive</c>, used by ReactiveUI.Reactive.</summary>
    BindingReactive,
}
