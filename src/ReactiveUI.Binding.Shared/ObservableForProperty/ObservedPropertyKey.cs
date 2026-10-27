// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>Identifies one property of one type, for the caches that answer per property rather than per type.</summary>
/// <param name="Type">The type declaring the property.</param>
/// <param name="PropertyName">The name of the property.</param>
[DebuggerDisplay("ObservedPropertyKey: {Type.Name,nq}.{PropertyName,nq}")]
internal readonly record struct ObservedPropertyKey(Type Type, string PropertyName);
