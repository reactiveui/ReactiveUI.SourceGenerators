// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A stub implementation of <see cref="IBindingFallbackConverter"/> for testing.</summary>
public class StubFallbackConverter : IBindingFallbackConverter
{
    /// <summary>The fixed affinity score returned by this stub converter.</summary>
    private const int StubAffinity = 5;

    /// <summary>The conversion logic.</summary>
    private readonly Func<Type, object, Type, object?, StubConversionResult> _tryConvert;

    /// <summary>Initializes a new instance of the <see cref="StubFallbackConverter"/> class.</summary>
    /// <param name="tryConvert">The conversion logic.</param>
    public StubFallbackConverter(Func<Type, object, Type, object?, StubConversionResult> tryConvert) =>
        _tryConvert = tryConvert;

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects(Type fromType, Type toType) => StubAffinity;

    /// <inheritdoc/>
    public bool TryConvert(
        Type fromType,
        object from,
        Type toType,
        object? conversionHint,
        [NotNullWhen(true)] out object? result)
    {
        var (success, converted) = _tryConvert(fromType, from, toType, conversionHint);
        result = converted;
        return success;
    }
}
