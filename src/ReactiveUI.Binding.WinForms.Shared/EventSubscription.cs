// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reflection;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.WinForms;
#else
namespace ReactiveUI.Binding.WinForms;
#endif

/// <summary>The handler a WinForms change-event observation has to detach when it is disposed.</summary>
/// <param name="EventInfo">The event the handler was attached to.</param>
/// <param name="Sender">The component raising it.</param>
/// <param name="Handler">The handler to detach.</param>
[DebuggerDisplay("EventSubscription: {EventInfo.Name,nq} on {Sender}")]
internal readonly record struct EventSubscription(
    EventInfo EventInfo,
    object Sender,
    EventHandler Handler);
