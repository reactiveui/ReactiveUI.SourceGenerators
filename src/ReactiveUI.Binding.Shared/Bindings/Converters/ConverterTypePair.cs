// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>The pair of types a registered binding converter moves a value between.</summary>
/// <param name="FromType">The type converted from.</param>
/// <param name="ToType">The type converted to.</param>
[DebuggerDisplay("ConverterTypePair: {FromType.Name,nq} -> {ToType.Name,nq}")]
internal readonly record struct ConverterTypePair(Type FromType, Type ToType);
