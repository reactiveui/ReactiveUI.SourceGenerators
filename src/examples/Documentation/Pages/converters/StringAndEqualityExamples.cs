// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using ReactiveUI.Binding.Documentation.Todo;

namespace ReactiveUI.Binding.Documentation.Converters;

/// <summary>Examples of using built-in converters for strings, booleans, and equality.</summary>
public static class StringAndEqualityExamples
{
    /// <summary>Demonstrates the StringConverter for identity string-to-string binding.</summary>
    public static void DemonstrateStringConverter()
    {
        StringConverter converter = new StringConverter();
        bool success = converter.TryConvertTyped("Renew car registration", null, out var result);

        Console.WriteLine(success);
        Console.WriteLine(result);
        Console.WriteLine(converter.FromType);
        Console.WriteLine(converter.ToType);

        // Output:
        // True
        // Renew car registration
        // System.String
        // System.String
    }

    /// <summary>Demonstrates the EqualityTypeConverter for comparing values to a hint.</summary>
    public static void DemonstrateEqualityConverter()
    {
        EqualityTypeConverter converter = new EqualityTypeConverter();
        TodoItem item = new() { Title = "Renew car registration", Priority = TodoPriority.High };

        bool success = converter.TryConvertTyped(item.Priority, TodoPriority.High, out var isHigh);

        Console.WriteLine(success);
        Console.WriteLine(isHigh);

        success = converter.TryConvertTyped(item.Priority, TodoPriority.Low, out var isLow);

        Console.WriteLine(success);
        Console.WriteLine(isLow);

        // Output:
        // True
        // True
        // True
        // False
    }

    /// <summary>Demonstrates boolean-to-string and string-to-boolean converters.</summary>
    public static void DemonstrateBooleanConverters()
    {
        BooleanToStringTypeConverter boolToStringConverter = new BooleanToStringTypeConverter();
        bool trueSuccess = boolToStringConverter.TryConvertTyped(true, null, out var trueResult);

        Console.WriteLine(trueSuccess);
        Console.WriteLine(trueResult);

        bool falseSuccess = boolToStringConverter.TryConvertTyped(false, null, out var falseResult);

        Console.WriteLine(falseSuccess);
        Console.WriteLine(falseResult);

        StringToBooleanTypeConverter stringToBoolConverter = new StringToBooleanTypeConverter();
        bool stringTrueSuccess = stringToBoolConverter.TryConvertTyped("True", null, out var parsedTrue);

        Console.WriteLine(stringTrueSuccess);
        Console.WriteLine(parsedTrue);

        bool stringFalseSuccess = stringToBoolConverter.TryConvertTyped("False", null, out var parsedFalse);

        Console.WriteLine(stringFalseSuccess);
        Console.WriteLine(parsedFalse);

        // Output:
        // True
        // True
        // True
        // False
        // True
        // True
        // True
        // False
    }

    /// <summary>Shows the affinity of the string converter, which a string-to-string binding uses.</summary>
    public static void ShowStringConverterAffinity()
    {
        StringConverter converter = new StringConverter();

        Console.WriteLine(converter.GetAffinityForObjects());

        // Output:
        // 2
    }

    /// <summary>Shows the type pair and the affinity of the equality converter; a built-in converter for the same pair outranks it.</summary>
    public static void ShowEqualityConverterTypePair()
    {
        EqualityTypeConverter converter = new EqualityTypeConverter();

        Console.WriteLine($"{converter.FromType} -> {converter.ToType}");
        Console.WriteLine(converter.GetAffinityForObjects());

        // Output:
        // System.Object -> System.Boolean
        // 1
    }
}
