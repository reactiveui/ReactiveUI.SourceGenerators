// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI.SourceGenerators.CodeGeneration;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>A constant an attribute argument holds, as the C# expression that writes it.</summary>
/// <param name="Kind">What the constant is.</param>
/// <param name="Expression">The C# expression that produces the constant.</param>
internal sealed partial record TypedConstantInfo(TypedConstantInfo.ConstantKind Kind, string Expression)
{
    /// <summary>The characters reserved for each element when an array's text is sized.</summary>
    private const int ItemCapacity = 16;

    /// <summary>The kinds of constant an attribute argument can hold.</summary>
    internal enum ConstantKind
    {
        /// <summary>A null reference.</summary>
        Null = 0,

        /// <summary>A string.</summary>
        String = 1,

        /// <summary>A Boolean.</summary>
        Boolean = 2,

        /// <summary>A number or a character.</summary>
        Number = 3,

        /// <summary>A <c>typeof</c> expression.</summary>
        Type = 4,

        /// <summary>An enum value.</summary>
        Enum = 5,

        /// <summary>An array of constants.</summary>
        Array = 6,
    }

    /// <summary>Creates the null constant.</summary>
    /// <returns>The constant.</returns>
    internal static TypedConstantInfo Null() => new(ConstantKind.Null, "null");

    /// <summary>Creates a string constant.</summary>
    /// <param name="value">The string.</param>
    /// <returns>The constant.</returns>
    internal static TypedConstantInfo OfString(string value) => new(ConstantKind.String, SymbolDisplay.FormatLiteral(value, true));

    /// <summary>Creates a Boolean constant.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The constant.</returns>
    internal static TypedConstantInfo OfBoolean(bool value) => new(ConstantKind.Boolean, value ? "true" : "false");

    /// <summary>Creates a numeric or character constant.</summary>
    /// <typeparam name="T">The primitive type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>The constant.</returns>
    /// <exception cref="ArgumentException">Thrown if the value is not a supported primitive type.</exception>
    internal static TypedConstantInfo OfNumber<T>(T value)
        where T : unmanaged, IEquatable<T> => new(ConstantKind.Number, GetNumericLiteral(value));

    /// <summary>Creates a <c>typeof</c> constant.</summary>
    /// <param name="typeName">The fully qualified type name.</param>
    /// <returns>The constant.</returns>
    internal static TypedConstantInfo OfType(string typeName) => new(ConstantKind.Type, $"typeof({typeName})");

    /// <summary>Creates an enum constant.</summary>
    /// <param name="typeName">The fully qualified enum type name.</param>
    /// <param name="value">The enum's underlying value.</param>
    /// <returns>The constant.</returns>
    /// <remarks>A negative value is parenthesized, because a cast directly before a minus sign does not compile (CS0075).</remarks>
    internal static TypedConstantInfo OfEnum(string typeName, object value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture);
        return new(
            ConstantKind.Enum,
            text.StartsWith("-", StringComparison.Ordinal) ? $"({typeName})({text})" : $"({typeName}){text}");
    }

    /// <summary>Creates an array constant.</summary>
    /// <param name="elementTypeName">The fully qualified element type name.</param>
    /// <param name="items">The array's elements.</param>
    /// <returns>The constant.</returns>
    internal static TypedConstantInfo OfArray(string elementTypeName, ReadOnlySpan<TypedConstantInfo> items)
    {
        var builder = PooledBuilder.Rent(elementTypeName.Length + (items.Length * ItemCapacity));
        _ = builder.Append("new ").Append(elementTypeName).Append("[]{");
        for (var index = 0; index < items.Length; index++)
        {
            if (index != 0)
            {
                _ = builder.Append(',');
            }

            _ = builder.Append(items[index].Expression);
        }

        return new(ConstantKind.Array, PooledBuilder.ToStringAndReturn(builder.Append('}')));
    }

    /// <summary>Writes a primitive value as a C# literal that keeps its type.</summary>
    /// <typeparam name="T">The primitive value type.</typeparam>
    /// <param name="value">The primitive value.</param>
    /// <returns>The literal.</returns>
    /// <exception cref="ArgumentException">Thrown if the value is not a supported primitive type.</exception>
    private static string GetNumericLiteral<T>(T value)
        where T : unmanaged, IEquatable<T> => value switch
        {
            byte byteValue => SyntaxFactory.Literal(byteValue).Text,
            char characterValue => SyntaxFactory.Literal(characterValue).Text,
            double doubleValue => $"{doubleValue.ToString("R", CultureInfo.InvariantCulture)}D",
            float singleValue => SyntaxFactory.Literal(singleValue).Text,
            int integerValue => SyntaxFactory.Literal(integerValue).Text,
            long longValue => SyntaxFactory.Literal(longValue).Text,
            sbyte signedByteValue => SyntaxFactory.Literal(signedByteValue).Text,
            short shortValue => SyntaxFactory.Literal(shortValue).Text,
            uint unsignedIntegerValue => SyntaxFactory.Literal(unsignedIntegerValue).Text,
            ulong unsignedLongValue => SyntaxFactory.Literal(unsignedLongValue).Text,
            ushort unsignedShortValue => SyntaxFactory.Literal(unsignedShortValue).Text,
            _ => throw new ArgumentException("Invalid primitive type"),
        };
}
