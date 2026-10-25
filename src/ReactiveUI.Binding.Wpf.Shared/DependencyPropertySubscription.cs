// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.ComponentModel;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Wpf;
#else
namespace ReactiveUI.Binding.Wpf;
#endif

/// <summary>The handler a dependency-property observation has to detach when it is disposed.</summary>
/// <param name="Descriptor">The descriptor the handler was attached through.</param>
/// <param name="Sender">The object being watched.</param>
/// <param name="Handler">The handler to detach.</param>
[DebuggerDisplay("DependencyPropertySubscription: {Descriptor.Name,nq} on {Sender}")]
internal readonly record struct DependencyPropertySubscription(
    DependencyPropertyDescriptor Descriptor,
    object Sender,
    EventHandler Handler);
