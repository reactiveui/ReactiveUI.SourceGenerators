// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Demonstrates the culture used by numeric formatting.</summary>
public static class CultureExamples
{
    /// <summary>Formats a float and its nullable counterpart with the current culture, including format hints.</summary>
    public static void FormatSingleWithCurrentCulture()
    {
        CultureInfo? previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            const float value = 3.14F;
            SingleToStringTypeConverter converter = new SingleToStringTypeConverter();
            NullableSingleToStringTypeConverter nullableConverter = new NullableSingleToStringTypeConverter();

            _ = converter.TryConvert(value, conversionHint: null, out var plain);
            _ = converter.TryConvert(value, conversionHint: 2, out var precision);
            _ = converter.TryConvert(value, conversionHint: "F3", out var formatted);
            _ = nullableConverter.TryConvert(value, conversionHint: null, out var nullable);

            Console.WriteLine(plain);
            Console.WriteLine(precision);
            Console.WriteLine(formatted);
            Console.WriteLine(nullable);

            // Output:
            // 3,14
            // 3,14
            // 3,140
            // 3,14
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }
}
