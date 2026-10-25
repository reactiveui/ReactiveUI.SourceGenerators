// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Marker interface that every <see cref="IViewFor"/> implements; it declares no members.</summary>
[SuppressMessage("Design", "SST1437:Empty interface", Justification = "Intentional marker interface.")]
[SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "Intentional marker interface.")]
public interface IActivatableView;
