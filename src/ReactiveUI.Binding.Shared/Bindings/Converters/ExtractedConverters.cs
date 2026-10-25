// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>The converters found in a dependency resolver, grouped by the role each one fills.</summary>
/// <param name="TypedConverters">The converters that move a value between two known types.</param>
/// <param name="FallbackConverters">The converters consulted when no typed converter matches.</param>
/// <param name="SetMethodConverters">The converters that write a value to a target rather than return one.</param>
[DebuggerDisplay("ExtractedConverters: {TypedConverters.Count} typed, {FallbackConverters.Count} fallback, {SetMethodConverters.Count} set-method")]
public sealed record ExtractedConverters(
    IList<IBindingTypeConverter> TypedConverters,
    IList<IBindingFallbackConverter> FallbackConverters,
    IList<ISetMethodBindingConverter> SetMethodConverters);
