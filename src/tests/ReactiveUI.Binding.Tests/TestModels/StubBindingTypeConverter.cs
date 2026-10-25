// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.TestModels;

/// <summary>A stub implementation of <see cref="IBindingTypeConverter"/> for testing.</summary>
public class StubBindingTypeConverter : IBindingTypeConverter
{
    /// <summary>Affinity returned by this stub converter to indicate it can handle the conversion.</summary>
    private const int StubAffinity = 10;

    /// <summary>The conversion logic.</summary>
    private readonly Func<object?, object?, StubConversionResult> _tryConvert;

    /// <summary>Initializes a new instance of the <see cref="StubBindingTypeConverter"/> class.</summary>
    /// <param name="fromType">The source type.</param>
    /// <param name="toType">The target type.</param>
    /// <param name="tryConvert">The conversion logic.</param>
    public StubBindingTypeConverter(
        Type fromType,
        Type toType,
        Func<object?, object?, StubConversionResult> tryConvert)
    {
        FromType = fromType;
        ToType = toType;
        _tryConvert = tryConvert;
    }

    /// <inheritdoc/>
    public Type FromType { get; }

    /// <inheritdoc/>
    public Type ToType { get; }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetAffinityForObjects() => StubAffinity;

    /// <inheritdoc/>
    public bool TryConvertTyped(object? from, object? conversionHint, out object? result)
    {
        var (success, converted) = _tryConvert(from, conversionHint);
        result = converted;
        return success;
    }
}
