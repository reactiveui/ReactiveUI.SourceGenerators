// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Splat.Builder;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive.Builder;
#else
namespace ReactiveUI.Binding.Builder;
#endif

/// <summary>Represents a configured ReactiveUI.Binding application instance.</summary>
/// <seealso cref="IAppInstance" />
public interface IReactiveUIBindingInstance : IAppInstance;
