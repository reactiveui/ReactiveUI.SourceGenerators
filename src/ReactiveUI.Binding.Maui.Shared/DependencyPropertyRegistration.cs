// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if WINUI_TARGET
using Microsoft.UI.Xaml;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Maui;
#else
namespace ReactiveUI.Binding.Maui;
#endif

/// <summary>The registration a dependency-property observation has to undo when it is disposed.</summary>
/// <param name="Sender">The object the callback was registered on.</param>
/// <param name="Property">The property being watched.</param>
/// <param name="Token">The token the registration returned.</param>
[DebuggerDisplay("DependencyPropertyRegistration: {Property} on {Sender}")]
internal readonly record struct DependencyPropertyRegistration(
    DependencyObject Sender,
    DependencyProperty Property,
    long Token);
#endif
