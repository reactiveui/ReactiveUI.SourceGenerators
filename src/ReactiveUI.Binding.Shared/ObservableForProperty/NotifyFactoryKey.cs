// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.ObservableForProperty;
#else
namespace ReactiveUI.Binding.ObservableForProperty;
#endif

/// <summary>Identifies the notification factory resolved for one property, in one direction of change.</summary>
/// <param name="SenderType">The type raising the notification.</param>
/// <param name="PropertyName">The name of the observed property.</param>
/// <param name="BeforeChange">Whether before-change notification was asked for.</param>
[DebuggerDisplay("NotifyFactoryKey: {SenderType.Name,nq}.{PropertyName,nq} before={BeforeChange}")]
internal readonly record struct NotifyFactoryKey(Type SenderType, string PropertyName, bool BeforeChange);
