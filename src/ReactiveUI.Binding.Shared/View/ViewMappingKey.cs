// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Identifies one registered view mapping: the view model it serves, under which contract.</summary>
/// <param name="ViewModelType">The view model type the mapping resolves a view for.</param>
/// <param name="Contract">The contract the mapping is registered under, empty for the default.</param>
[DebuggerDisplay("ViewMappingKey: {ViewModelType.Name,nq} ({Contract})")]
internal readonly record struct ViewMappingKey(Type ViewModelType, string Contract);
