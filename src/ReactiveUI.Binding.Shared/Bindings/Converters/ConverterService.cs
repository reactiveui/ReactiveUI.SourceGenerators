// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

#if REACTIVE_SHIM
namespace ReactiveUI.Binding.Reactive;
#else
namespace ReactiveUI.Binding;
#endif

/// <summary>Owns the typed, fallback and set-method converter registries and resolves the best converter for a type pair.</summary>
/// <remarks>
/// A new service holds no converters; <see cref="DefaultConverterRegistration.RegisterDefaults"/> adds the built-in ones.
/// </remarks>
[DebuggerDisplay("ConverterService: Typed: {TypedConverters}; fallback: {FallbackConverters}; set-method: {SetMethodConverters}")]
public sealed class ConverterService
{
    /// <summary>Initializes a new instance of the <see cref="ConverterService"/> class.</summary>
    public ConverterService()
    {
        TypedConverters = new();
        FallbackConverters = new();
        SetMethodConverters = new();
    }

    /// <summary>Gets the registry of typed converters, each matched to an exact source and target type pair.</summary>
    public BindingTypeConverterRegistry TypedConverters { get; }

    /// <summary>Gets the registry of fallback converters, consulted when no typed converter applies.</summary>
    public BindingFallbackConverterRegistry FallbackConverters { get; }

    /// <summary>Gets the registry of converters that replace how a binding writes to its target.</summary>
    public SetMethodBindingConverterRegistry SetMethodConverters { get; }

    /// <summary>Returns the best typed converter for the type pair, or else the best fallback converter.</summary>
    /// <param name="fromType">The source type to convert from.</param>
    /// <param name="toType">The target type to convert to.</param>
    /// <returns>
    /// An <see cref="IBindingTypeConverter"/> or an <see cref="IBindingFallbackConverter"/>; a typed converter always
    /// wins over a fallback one whatever their affinities. <see langword="null"/> when neither registry has a match.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="fromType"/> or <paramref name="toType"/> is null.
    /// </exception>
    public object? ResolveConverter(
        Type fromType,
        Type toType)
    {
        ArgumentExceptionHelper.ThrowIfNull(fromType);
        ArgumentExceptionHelper.ThrowIfNull(toType);

        // Typed converters match the exact pair; fallback converters are asked only when none does.
        var typed = TypedConverters.TryGetConverter(fromType, toType);
        return typed is not null ? typed : FallbackConverters.TryGetConverter(fromType, toType);
    }

    /// <summary>Returns the best set-method converter for the type pair.</summary>
    /// <param name="fromType">The type of the value being written; may be null.</param>
    /// <param name="toType">The type of the target being written to; may be null.</param>
    /// <returns>The converter with the highest positive affinity, or <see langword="null"/> when none applies.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ISetMethodBindingConverter? ResolveSetMethodConverter(
        Type? fromType,
        Type? toType) => SetMethodConverters.TryGetConverter(fromType, toType);
}
