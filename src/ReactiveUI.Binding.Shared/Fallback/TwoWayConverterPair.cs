// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Fallback;
#else
namespace ReactiveUI.Binding.Fallback;
#endif

/// <summary>The pair of conversions a two-way binding needs to move a value in either direction.</summary>
/// <typeparam name="TSourceProp">The type of the source property.</typeparam>
/// <typeparam name="TTargetProp">The type of the target property.</typeparam>
/// <param name="Forward">Converts a source value to the target's type.</param>
/// <param name="Reverse">Converts a target value back to the source's type.</param>
[EditorBrowsable(EditorBrowsableState.Never)]
[DebuggerDisplay("TwoWayConverterPair: {typeof(TSourceProp).Name,nq} <-> {typeof(TTargetProp).Name,nq}")]
public sealed record TwoWayConverterPair<TSourceProp, TTargetProp>(
    Func<TSourceProp, TTargetProp> Forward,
    Func<TTargetProp, TSourceProp> Reverse);
