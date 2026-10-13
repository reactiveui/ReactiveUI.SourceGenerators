// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace ReactiveUI.Binding.Tests.Bindings;

/// <summary>Unit tests for <see cref="BindingTypeConverter{TFrom, TTo}.TryConvertTyped(object?, object?, out object?)"/> to ensure coverage of the base class dispatch path.</summary>
public class BindingTypeConverterTryConvertTypedTests
{
    /// <summary>Literal used as a deliberately wrong-typed conversion input.</summary>
    private const string WrongTypeInput = "wrong";

    /// <summary>Sample byte value used for byte conversion tests.</summary>
    private const byte ByteValue = 42;

    /// <summary>Smaller byte value used for byte conversion tests.</summary>
    private const byte SmallByteValue = 7;

    /// <summary>Sample integer value used for integer conversion tests.</summary>
    private const int IntValue = 123;

    /// <summary>Smaller integer value used for integer conversion tests.</summary>
    private const int SmallIntValue = 42;

    /// <summary>Integer value parsed from a string in conversion tests.</summary>
    private const int ParsedIntValue = 456;

    /// <summary>Sample double value used for double conversion tests.</summary>
    private const double DoubleValue = 3.14;

    /// <summary>Alternate double value used for double conversion tests.</summary>
    private const double EulerDoubleValue = 2.718;

    /// <summary>Sample long value used for long conversion tests.</summary>
    private const long LongValue = 9_876_543_210L;

    /// <summary>Sample short value used for short conversion tests.</summary>
    private const short ShortValue = 321;

    /// <summary>Sample single-precision value used for float conversion tests.</summary>
    private const float SingleValue = 1.5F;

    /// <summary>Sample decimal value used for decimal conversion tests.</summary>
    private const decimal DecimalValue = 99.99M;

    // ===================================================================
    // Non-nullable to String converters
    // ===================================================================
    /// <summary>Verifies ByteToStringTypeConverter converts a byte value to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ByteToString_Success() =>
        AssertConverterSuccess(new ByteToStringTypeConverter(), ByteValue, "42");

    /// <summary>Verifies ByteToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ByteToString_Failure_WrongType() =>
        AssertConverterFailure(new ByteToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies ByteToStringTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ByteToString_Failure_Null() =>
        AssertConverterFailure(new ByteToStringTypeConverter(), null);

    /// <summary>Verifies IntegerToStringTypeConverter converts an int value to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task IntegerToString_Success() =>
        AssertConverterSuccess(new IntegerToStringTypeConverter(), IntValue, "123");

    /// <summary>Verifies IntegerToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task IntegerToString_Failure_WrongType() =>
        AssertConverterFailure(new IntegerToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies IntegerToStringTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task IntegerToString_Failure_Null() =>
        AssertConverterFailure(new IntegerToStringTypeConverter(), null);

    /// <summary>Verifies StringToIntegerTypeConverter converts a string to an int.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task StringToInteger_Success() =>
        AssertConverterSuccess(new StringToIntegerTypeConverter(), "456", ParsedIntValue);

    /// <summary>Verifies BooleanToStringTypeConverter converts true to "True".</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task BooleanToString_Success() =>
        AssertConverterSuccess(new BooleanToStringTypeConverter(), true, "True");

    /// <summary>Verifies DoubleToStringTypeConverter converts a double to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DoubleToString_Success() =>
        AssertConverterSuccess(new DoubleToStringTypeConverter(), DoubleValue, DoubleValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>Verifies DoubleToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DoubleToString_Failure_WrongType() =>
        AssertConverterFailure(new DoubleToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies DoubleToStringTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DoubleToString_Failure_Null() =>
        AssertConverterFailure(new DoubleToStringTypeConverter(), null);

    /// <summary>Verifies LongToStringTypeConverter converts a long to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task LongToString_Success() =>
        AssertConverterSuccess(new LongToStringTypeConverter(), LongValue, "9876543210");

    /// <summary>Verifies LongToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task LongToString_Failure_WrongType() =>
        AssertConverterFailure(new LongToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies LongToStringTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task LongToString_Failure_Null() =>
        AssertConverterFailure(new LongToStringTypeConverter(), null);

    /// <summary>Verifies ShortToStringTypeConverter converts a short to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ShortToString_Success() =>
        AssertConverterSuccess(new ShortToStringTypeConverter(), ShortValue, "321");

    /// <summary>Verifies ShortToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ShortToString_Failure_WrongType() =>
        AssertConverterFailure(new ShortToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies ShortToStringTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ShortToString_Failure_Null() =>
        AssertConverterFailure(new ShortToStringTypeConverter(), null);

    /// <summary>Verifies SingleToStringTypeConverter converts a float to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SingleToString_Success() =>
        AssertConverterSuccess(new SingleToStringTypeConverter(), SingleValue, SingleValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>Verifies SingleToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SingleToString_Failure_WrongType() =>
        AssertConverterFailure(new SingleToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies SingleToStringTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SingleToString_Failure_Null() =>
        AssertConverterFailure(new SingleToStringTypeConverter(), null);

    // ===================================================================
    // Nullable to String converters
    // ===================================================================
    /// <summary>Verifies NullableByteToStringTypeConverter converts a byte to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableByteToString_Success() =>
        AssertConverterSuccess(new NullableByteToStringTypeConverter(), ByteValue, "42");

    /// <summary>Verifies NullableByteToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableByteToString_Failure_WrongType() =>
        AssertConverterFailure(new NullableByteToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableByteToStringTypeConverter succeeds when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableByteToString_Success_Null() =>
        AssertConverterSuccess(new NullableByteToStringTypeConverter(), null, null);

    /// <summary>Verifies NullableDecimalToStringTypeConverter converts a decimal to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDecimalToString_Success() =>
        AssertConverterSuccess(new NullableDecimalToStringTypeConverter(), DecimalValue, DecimalValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>Verifies NullableDecimalToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDecimalToString_Failure_WrongType() =>
        AssertConverterFailure(new NullableDecimalToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableDecimalToStringTypeConverter succeeds when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDecimalToString_Success_Null() =>
        AssertConverterSuccess(new NullableDecimalToStringTypeConverter(), null, null);

    /// <summary>Verifies NullableDoubleToStringTypeConverter converts a double to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDoubleToString_Success() =>
        AssertConverterSuccess(new NullableDoubleToStringTypeConverter(), EulerDoubleValue, EulerDoubleValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>Verifies NullableDoubleToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDoubleToString_Failure_WrongType() =>
        AssertConverterFailure(new NullableDoubleToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableDoubleToStringTypeConverter succeeds when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDoubleToString_Success_Null() =>
        AssertConverterSuccess(new NullableDoubleToStringTypeConverter(), null, null);

    /// <summary>Verifies NullableIntegerToStringTypeConverter converts an int to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableIntegerToString_Success() =>
        AssertConverterSuccess(new NullableIntegerToStringTypeConverter(), IntValue, "123");

    /// <summary>Verifies NullableIntegerToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableIntegerToString_Failure_WrongType() =>
        AssertConverterFailure(new NullableIntegerToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableIntegerToStringTypeConverter succeeds when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableIntegerToString_Success_Null() =>
        AssertConverterSuccess(new NullableIntegerToStringTypeConverter(), null, null);

    /// <summary>Verifies NullableLongToStringTypeConverter converts a long to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableLongToString_Success() =>
        AssertConverterSuccess(new NullableLongToStringTypeConverter(), LongValue, "9876543210");

    /// <summary>Verifies NullableLongToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableLongToString_Failure_WrongType() =>
        AssertConverterFailure(new NullableLongToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableLongToStringTypeConverter succeeds when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableLongToString_Success_Null() =>
        AssertConverterSuccess(new NullableLongToStringTypeConverter(), null, null);

    /// <summary>Verifies NullableShortToStringTypeConverter converts a short to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableShortToString_Success() =>
        AssertConverterSuccess(new NullableShortToStringTypeConverter(), ShortValue, "321");

    /// <summary>Verifies NullableShortToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableShortToString_Failure_WrongType() =>
        AssertConverterFailure(new NullableShortToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableShortToStringTypeConverter succeeds when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableShortToString_Success_Null() =>
        AssertConverterSuccess(new NullableShortToStringTypeConverter(), null, null);

    /// <summary>Verifies NullableSingleToStringTypeConverter converts a float to its string representation.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableSingleToString_Success() =>
        AssertConverterSuccess(new NullableSingleToStringTypeConverter(), SingleValue, SingleValue.ToString(System.Globalization.CultureInfo.CurrentCulture));

    /// <summary>Verifies NullableSingleToStringTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableSingleToString_Failure_WrongType() =>
        AssertConverterFailure(new NullableSingleToStringTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableSingleToStringTypeConverter succeeds when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableSingleToString_Success_Null() =>
        AssertConverterSuccess(new NullableSingleToStringTypeConverter(), null, null);

    // ===================================================================
    // Value-to-Nullable converters
    // ===================================================================
    /// <summary>Verifies DoubleToNullableDoubleTypeConverter converts a double to nullable double.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DoubleToNullableDouble_Success() =>
        AssertConverterSuccess(new DoubleToNullableDoubleTypeConverter(), DoubleValue, (double?)DoubleValue);

    /// <summary>Verifies DoubleToNullableDoubleTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DoubleToNullableDouble_Failure_WrongType() =>
        AssertConverterFailure(new DoubleToNullableDoubleTypeConverter(), WrongTypeInput);

    /// <summary>Verifies DoubleToNullableDoubleTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DoubleToNullableDouble_Failure_Null() =>
        AssertConverterFailure(new DoubleToNullableDoubleTypeConverter(), null);

    /// <summary>Verifies IntegerToNullableIntegerTypeConverter converts an int to nullable int.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task IntegerToNullableInteger_Success() =>
        AssertConverterSuccess(new IntegerToNullableIntegerTypeConverter(), SmallIntValue, (int?)SmallIntValue);

    /// <summary>Verifies IntegerToNullableIntegerTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task IntegerToNullableInteger_Failure_WrongType() =>
        AssertConverterFailure(new IntegerToNullableIntegerTypeConverter(), WrongTypeInput);

    /// <summary>Verifies IntegerToNullableIntegerTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task IntegerToNullableInteger_Failure_Null() =>
        AssertConverterFailure(new IntegerToNullableIntegerTypeConverter(), null);

    /// <summary>Verifies ByteToNullableByteTypeConverter converts a byte to nullable byte.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ByteToNullableByte_Success() =>
        AssertConverterSuccess(new ByteToNullableByteTypeConverter(), SmallByteValue, (byte?)SmallByteValue);

    /// <summary>Verifies ByteToNullableByteTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ByteToNullableByte_Failure_WrongType() =>
        AssertConverterFailure(new ByteToNullableByteTypeConverter(), WrongTypeInput);

    /// <summary>Verifies ByteToNullableByteTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ByteToNullableByte_Failure_Null() =>
        AssertConverterFailure(new ByteToNullableByteTypeConverter(), null);

    /// <summary>Verifies LongToNullableLongTypeConverter converts a long to nullable long.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task LongToNullableLong_Success() =>
        AssertConverterSuccess(new LongToNullableLongTypeConverter(), LongValue, (long?)LongValue);

    /// <summary>Verifies LongToNullableLongTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task LongToNullableLong_Failure_WrongType() =>
        AssertConverterFailure(new LongToNullableLongTypeConverter(), WrongTypeInput);

    /// <summary>Verifies LongToNullableLongTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task LongToNullableLong_Failure_Null() =>
        AssertConverterFailure(new LongToNullableLongTypeConverter(), null);

    /// <summary>Verifies ShortToNullableShortTypeConverter converts a short to nullable short.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ShortToNullableShort_Success() =>
        AssertConverterSuccess(new ShortToNullableShortTypeConverter(), ShortValue, (short?)ShortValue);

    /// <summary>Verifies ShortToNullableShortTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ShortToNullableShort_Failure_WrongType() =>
        AssertConverterFailure(new ShortToNullableShortTypeConverter(), WrongTypeInput);

    /// <summary>Verifies ShortToNullableShortTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ShortToNullableShort_Failure_Null() =>
        AssertConverterFailure(new ShortToNullableShortTypeConverter(), null);

    /// <summary>Verifies SingleToNullableSingleTypeConverter converts a float to nullable float.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SingleToNullableSingle_Success() =>
        AssertConverterSuccess(new SingleToNullableSingleTypeConverter(), SingleValue, (float?)SingleValue);

    /// <summary>Verifies SingleToNullableSingleTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SingleToNullableSingle_Failure_WrongType() =>
        AssertConverterFailure(new SingleToNullableSingleTypeConverter(), WrongTypeInput);

    /// <summary>Verifies SingleToNullableSingleTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task SingleToNullableSingle_Failure_Null() =>
        AssertConverterFailure(new SingleToNullableSingleTypeConverter(), null);

    /// <summary>Verifies DecimalToNullableDecimalTypeConverter converts a decimal to nullable decimal.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DecimalToNullableDecimal_Success() =>
        AssertConverterSuccess(new DecimalToNullableDecimalTypeConverter(), DecimalValue, (decimal?)DecimalValue);

    /// <summary>Verifies DecimalToNullableDecimalTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DecimalToNullableDecimal_Failure_WrongType() =>
        AssertConverterFailure(new DecimalToNullableDecimalTypeConverter(), WrongTypeInput);

    /// <summary>Verifies DecimalToNullableDecimalTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task DecimalToNullableDecimal_Failure_Null() =>
        AssertConverterFailure(new DecimalToNullableDecimalTypeConverter(), null);

    // ===================================================================
    // Nullable-to-Value converters
    // ===================================================================
    /// <summary>Verifies NullableDoubleToDoubleTypeConverter converts a double value successfully.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDoubleToDouble_Success() =>
        AssertConverterSuccess(new NullableDoubleToDoubleTypeConverter(), DoubleValue, DoubleValue);

    /// <summary>Verifies NullableDoubleToDoubleTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDoubleToDouble_Failure_WrongType() =>
        AssertConverterFailure(new NullableDoubleToDoubleTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableDoubleToDoubleTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDoubleToDouble_Failure_Null() =>
        AssertConverterFailure(new NullableDoubleToDoubleTypeConverter(), null);

    /// <summary>Verifies NullableIntegerToIntegerTypeConverter converts an int value successfully.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableIntegerToInteger_Success() =>
        AssertConverterSuccess(new NullableIntegerToIntegerTypeConverter(), SmallIntValue, SmallIntValue);

    /// <summary>Verifies NullableIntegerToIntegerTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableIntegerToInteger_Failure_WrongType() =>
        AssertConverterFailure(new NullableIntegerToIntegerTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableIntegerToIntegerTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableIntegerToInteger_Failure_Null() =>
        AssertConverterFailure(new NullableIntegerToIntegerTypeConverter(), null);

    /// <summary>Verifies NullableByteToByteTypeConverter converts a byte value successfully.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableByteToByte_Success() =>
        AssertConverterSuccess(new NullableByteToByteTypeConverter(), SmallByteValue, SmallByteValue);

    /// <summary>Verifies NullableByteToByteTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableByteToByte_Failure_WrongType() =>
        AssertConverterFailure(new NullableByteToByteTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableByteToByteTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableByteToByte_Failure_Null() =>
        AssertConverterFailure(new NullableByteToByteTypeConverter(), null);

    /// <summary>Verifies NullableLongToLongTypeConverter converts a long value successfully.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableLongToLong_Success() =>
        AssertConverterSuccess(new NullableLongToLongTypeConverter(), LongValue, LongValue);

    /// <summary>Verifies NullableLongToLongTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableLongToLong_Failure_WrongType() =>
        AssertConverterFailure(new NullableLongToLongTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableLongToLongTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableLongToLong_Failure_Null() =>
        AssertConverterFailure(new NullableLongToLongTypeConverter(), null);

    /// <summary>Verifies NullableShortToShortTypeConverter converts a short value successfully.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableShortToShort_Success() =>
        AssertConverterSuccess(new NullableShortToShortTypeConverter(), ShortValue, ShortValue);

    /// <summary>Verifies NullableShortToShortTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableShortToShort_Failure_WrongType() =>
        AssertConverterFailure(new NullableShortToShortTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableShortToShortTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableShortToShort_Failure_Null() =>
        AssertConverterFailure(new NullableShortToShortTypeConverter(), null);

    /// <summary>Verifies NullableSingleToSingleTypeConverter converts a float value successfully.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableSingleToSingle_Success() =>
        AssertConverterSuccess(new NullableSingleToSingleTypeConverter(), SingleValue, SingleValue);

    /// <summary>Verifies NullableSingleToSingleTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableSingleToSingle_Failure_WrongType() =>
        AssertConverterFailure(new NullableSingleToSingleTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableSingleToSingleTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableSingleToSingle_Failure_Null() =>
        AssertConverterFailure(new NullableSingleToSingleTypeConverter(), null);

    /// <summary>Verifies NullableDecimalToDecimalTypeConverter converts a decimal value successfully.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDecimalToDecimal_Success() =>
        AssertConverterSuccess(new NullableDecimalToDecimalTypeConverter(), DecimalValue, DecimalValue);

    /// <summary>Verifies NullableDecimalToDecimalTypeConverter fails when given a wrong type.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDecimalToDecimal_Failure_WrongType() =>
        AssertConverterFailure(new NullableDecimalToDecimalTypeConverter(), WrongTypeInput);

    /// <summary>Verifies NullableDecimalToDecimalTypeConverter fails when given null.</summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task NullableDecimalToDecimal_Failure_Null() =>
        AssertConverterFailure(new NullableDecimalToDecimalTypeConverter(), null);

    /// <summary>Asserts that the converter successfully converts the input to the expected output.</summary>
    /// <param name="converter">The converter to test.</param>
    /// <param name="input">The input value to convert.</param>
    /// <param name="expectedOutput">The expected conversion result.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertConverterSuccess(
        IBindingTypeConverter converter,
        object? input,
        object? expectedOutput)
    {
        var success = converter.TryConvertTyped(input, null, out var result);
        await Assert.That(success).IsTrue();
        await Assert.That(result).IsEqualTo(expectedOutput);
    }

    /// <summary>Asserts that the converter fails to convert the given input.</summary>
    /// <param name="converter">The converter to test.</param>
    /// <param name="input">The input value that should fail conversion.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    private static async Task AssertConverterFailure(IBindingTypeConverter converter, object? input)
    {
        var success = converter.TryConvertTyped(input, null, out _);
        await Assert.That(success).IsFalse();
    }
}
