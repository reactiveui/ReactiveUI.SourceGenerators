// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings.Converters;

/// <summary>Tests for verifying converter affinity values are correctly set.</summary>
public class ConverterAffinityTests
{
    /// <summary>The last-resort affinity reported by the equality converter.</summary>
    private const int LastResortAffinity = 1;

    /// <summary>The standard affinity reported by string-based type converters.</summary>
    private const int StringConverterAffinity = 2;

    /// <summary>Verifies that the EqualityTypeConverter has affinity 1 (last resort).</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task EqualityConverter_ShouldHaveAffinity1() =>
        AssertAffinity(new EqualityTypeConverter(), LastResortAffinity);

    // ===================================================================
    // String identity converter
    // ===================================================================
    /// <summary>Verifies that StringConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringConverter(), StringConverterAffinity);

    // ===================================================================
    // Numeric → String converters
    // ===================================================================
    /// <summary>Verifies that ByteToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ByteToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new ByteToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableByteToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableByteToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableByteToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that ShortToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ShortToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new ShortToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableShortToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableShortToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableShortToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that IntegerToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task IntegerToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new IntegerToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableIntegerToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableIntegerToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableIntegerToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that LongToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task LongToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new LongToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableLongToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableLongToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableLongToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that SingleToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SingleToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new SingleToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableSingleToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableSingleToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableSingleToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that DoubleToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DoubleToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new DoubleToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableDoubleToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDoubleToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableDoubleToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that DecimalToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DecimalToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new DecimalToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableDecimalToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDecimalToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableDecimalToStringTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // String → Numeric converters
    // ===================================================================
    /// <summary>Verifies that StringToByteTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToByteTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToByteTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableByteTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableByteTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableByteTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToShortTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToShortTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToShortTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableShortTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableShortTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableShortTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToIntegerTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToIntegerTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToIntegerTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableIntegerTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableIntegerTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableIntegerTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToLongTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToLongTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToLongTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableLongTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableLongTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableLongTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToSingleTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToSingleTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToSingleTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableSingleTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableSingleTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableSingleTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToDoubleTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToDoubleTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToDoubleTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableDoubleTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableDoubleTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableDoubleTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToDecimalTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToDecimalTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToDecimalTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableDecimalTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableDecimalTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableDecimalTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // Boolean ↔ String converters
    // ===================================================================
    /// <summary>Verifies that BooleanToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BooleanToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new BooleanToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableBooleanToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableBooleanToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableBooleanToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToBooleanTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToBooleanTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToBooleanTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableBooleanTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableBooleanTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableBooleanTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // Guid ↔ String converters
    // ===================================================================
    /// <summary>Verifies that GuidToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task GuidToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new GuidToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableGuidToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableGuidToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableGuidToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToGuidTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToGuidTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToGuidTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableGuidTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableGuidTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableGuidTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // DateTime ↔ String converters
    // ===================================================================
    /// <summary>Verifies that DateTimeToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DateTimeToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new DateTimeToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableDateTimeToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDateTimeToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableDateTimeToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToDateTimeTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToDateTimeTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToDateTimeTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableDateTimeTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableDateTimeTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableDateTimeTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // DateTimeOffset ↔ String converters
    // ===================================================================
    /// <summary>Verifies that DateTimeOffsetToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DateTimeOffsetToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new DateTimeOffsetToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableDateTimeOffsetToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDateTimeOffsetToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableDateTimeOffsetToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToDateTimeOffsetTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToDateTimeOffsetTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToDateTimeOffsetTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableDateTimeOffsetTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableDateTimeOffsetTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableDateTimeOffsetTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // TimeSpan ↔ String converters
    // ===================================================================
    /// <summary>Verifies that TimeSpanToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task TimeSpanToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new TimeSpanToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableTimeSpanToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableTimeSpanToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableTimeSpanToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToTimeSpanTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToTimeSpanTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToTimeSpanTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableTimeSpanTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableTimeSpanTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableTimeSpanTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // DateOnly ↔ String converters
    // ===================================================================
    /// <summary>Verifies that DateOnlyToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DateOnlyToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new DateOnlyToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableDateOnlyToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDateOnlyToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableDateOnlyToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToDateOnlyTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToDateOnlyTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToDateOnlyTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableDateOnlyTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableDateOnlyTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableDateOnlyTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // TimeOnly ↔ String converters
    // ===================================================================
    /// <summary>Verifies that TimeOnlyToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task TimeOnlyToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new TimeOnlyToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that NullableTimeOnlyToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableTimeOnlyToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new NullableTimeOnlyToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToTimeOnlyTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToTimeOnlyTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToTimeOnlyTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToNullableTimeOnlyTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToNullableTimeOnlyTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToNullableTimeOnlyTypeConverter(), StringConverterAffinity);

    // ===================================================================
    // Uri ↔ String converters
    // ===================================================================
    /// <summary>Verifies that UriToStringTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task UriToStringTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new UriToStringTypeConverter(), StringConverterAffinity);

    /// <summary>Verifies that StringToUriTypeConverter has affinity 2.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToUriTypeConverter_ShouldHaveAffinity2() =>
        AssertAffinity(new StringToUriTypeConverter(), StringConverterAffinity);

    /// <summary>Asserts that the specified converter reports the expected affinity value.</summary>
    /// <param name="converter">The converter to test.</param>
    /// <param name="expectedAffinity">The expected affinity value.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertAffinity(IBindingTypeConverter converter, int expectedAffinity)
    {
        var actualAffinity = converter.GetAffinityForObjects();
        await Assert.That(actualAffinity).IsEqualTo(expectedAffinity);
    }
}
