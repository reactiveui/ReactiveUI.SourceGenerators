// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Avalonia;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Avalonia;
#else
namespace ReactiveUI.Binding.Avalonia;
#endif

/// <summary>The object and handler an Avalonia property observation detaches when it is disposed.</summary>
/// <param name="Sender">The observed object.</param>
/// <param name="Handler">The property-changed handler attached to <paramref name="Sender"/>.</param>
[DebuggerDisplay("AvaloniaPropertySubscription: {Sender}")]
internal readonly record struct AvaloniaPropertySubscription(
    AvaloniaObject Sender,
    EventHandler<AvaloniaPropertyChangedEventArgs> Handler);
