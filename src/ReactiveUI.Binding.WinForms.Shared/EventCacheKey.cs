// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms;
#else
namespace ReactiveUI.Binding.WinForms;
#endif

/// <summary>Identifies the change event looked up for one property of one type.</summary>
/// <param name="Type">The type declaring the property.</param>
/// <param name="PropertyName">The name of the property.</param>
[DebuggerDisplay("EventCacheKey: {Type.Name,nq}.{PropertyName,nq}")]
internal readonly record struct EventCacheKey(Type Type, string PropertyName);
