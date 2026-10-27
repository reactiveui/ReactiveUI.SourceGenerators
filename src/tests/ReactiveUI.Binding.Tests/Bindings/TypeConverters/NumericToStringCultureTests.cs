// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;
using TUnit.Core.Executors;

namespace ReactiveUI.Binding.Tests.Bindings.TypeConverters;

/// <summary>Verifies numeric formatting follows the test's culture in both runtime flavours.</summary>
public class NumericToStringCultureTests
{
    /// <summary>The culture shared by ordinary tests.</summary>
    private const string CommonCulture = "en-US";

    /// <summary>Verifies ordinary tests start with the common assembly culture.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task TestCulture_WithoutOverride_UsesEnglishCulture()
    {
        await Assert.That(CultureInfo.CurrentCulture.Name).IsEqualTo(CommonCulture);
        await Assert.That(CultureInfo.CurrentUICulture.Name).IsEqualTo(CommonCulture);
    }

    /// <summary>Formats default, precision and string hints with a comma decimal separator.</summary>
    /// <param name="reactive">Whether to use the System.Reactive runtime.</param>
    /// <param name="nullable">Whether the source is nullable.</param>
    /// <param name="numericType">The source numeric type.</param>
    /// <param name="hint">The formatting hint.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [Culture("fr-FR")]
    [MatrixDataSource]
    public async Task TryConvert_FrenchCulture_UsesCurrentCulture(
        [Matrix(false, true)] bool reactive,
        [Matrix(false, true)] bool nullable,
        [Matrix("Single", "Double", "Decimal")] string numericType,
        [Matrix(null, 2, "F2")] object? hint)
    {
        await Assert.That(CultureInfo.CurrentCulture.Name).IsEqualTo("fr-FR");
        await AssertFormatting(reactive, nullable, numericType, hint, hint is null ? "42,5" : "42,50");
    }

    /// <summary>Formats default, precision and string hints with a dot decimal separator.</summary>
    /// <param name="reactive">Whether to use the System.Reactive runtime.</param>
    /// <param name="nullable">Whether the source is nullable.</param>
    /// <param name="numericType">The source numeric type.</param>
    /// <param name="hint">The formatting hint.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    [Culture(CommonCulture)]
    [MatrixDataSource]
    public async Task TryConvert_EnglishCulture_UsesCurrentCulture(
        [Matrix(false, true)] bool reactive,
        [Matrix(false, true)] bool nullable,
        [Matrix("Single", "Double", "Decimal")] string numericType,
        [Matrix(null, 2, "F2")] object? hint)
    {
        await Assert.That(CultureInfo.CurrentCulture.Name).IsEqualTo(CommonCulture);
        await AssertFormatting(reactive, nullable, numericType, hint, hint is null ? "42.5" : "42.50");
    }

    /// <summary>Checks one converter against the culture-specific expected text.</summary>
    /// <param name="reactive">Whether to use the System.Reactive runtime.</param>
    /// <param name="nullable">Whether the source is nullable.</param>
    /// <param name="numericType">The source numeric type.</param>
    /// <param name="hint">The formatting hint.</param>
    /// <param name="expected">The expected text.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task AssertFormatting(bool reactive, bool nullable, string numericType, object? hint, string expected)
    {
        const float singleValue = 42.5F;
        const double doubleValue = 42.5D;
        const decimal decimalValue = 42.5M;
        object value = numericType switch
        {
            "Single" => singleValue,
            "Double" => doubleValue,
            _ => decimalValue,
        };

        bool success;
        object? output;
        if (reactive)
        {
            global::ReactiveUI.Binding.Reactive.IBindingTypeConverter converter = (numericType, nullable) switch
            {
                ("Single", false) => new global::ReactiveUI.Binding.Reactive.SingleToStringTypeConverter(),
                ("Single", true) => new global::ReactiveUI.Binding.Reactive.NullableSingleToStringTypeConverter(),
                ("Double", false) => new global::ReactiveUI.Binding.Reactive.DoubleToStringTypeConverter(),
                ("Double", true) => new global::ReactiveUI.Binding.Reactive.NullableDoubleToStringTypeConverter(),
                ("Decimal", false) => new global::ReactiveUI.Binding.Reactive.DecimalToStringTypeConverter(),
                _ => new global::ReactiveUI.Binding.Reactive.NullableDecimalToStringTypeConverter(),
            };
            success = converter.TryConvertTyped(value, hint, out output);
        }
        else
        {
            IBindingTypeConverter converter = (numericType, nullable) switch
            {
                ("Single", false) => new SingleToStringTypeConverter(),
                ("Single", true) => new NullableSingleToStringTypeConverter(),
                ("Double", false) => new DoubleToStringTypeConverter(),
                ("Double", true) => new NullableDoubleToStringTypeConverter(),
                ("Decimal", false) => new DecimalToStringTypeConverter(),
                _ => new NullableDecimalToStringTypeConverter(),
            };
            success = converter.TryConvertTyped(value, hint, out output);
        }

        await Assert.That(success).IsTrue();
        await Assert.That(output).IsEqualTo(expected);
        await Assert.That(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).IsEqualTo(expected.Contains(',') ? "," : ".");
    }
}
