// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>A value a two-way binding moved, and which side of the binding produced it.</summary>
/// <param name="Value">The value that was written.</param>
/// <param name="FromViewModel">Whether the view model produced the value, rather than the view.</param>
/// <remarks>
/// A two-way binding's change stream carries both directions, so a subscriber that acted on every value
/// without knowing where it came from would act twice on its own write. The flag is what separates an edit
/// from the echo of one.
/// </remarks>
[DebuggerDisplay("BindingChange: {FromViewModel ? \"ViewModel\" : \"View\",nq} -> {Value}")]
public readonly record struct BindingChange(object? Value, bool FromViewModel);
