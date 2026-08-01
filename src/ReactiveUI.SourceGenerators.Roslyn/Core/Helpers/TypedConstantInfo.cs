// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>A model representing a typed constant item.</summary>
/// <remarks>This model is fully serializable and comparable.</remarks>
internal abstract partial record TypedConstantInfo
{
    /// <summary>Gets an <see cref="ExpressionSyntax"/> instance representing the current constant.</summary>
    /// <returns>The <see cref="ExpressionSyntax"/> instance representing the current constant.</returns>
    internal abstract ExpressionSyntax GetSyntax();

    /// <summary>Creates syntax expressions for an array of typed constants.</summary>
    /// <param name="items">The typed constants to convert.</param>
    /// <returns>The corresponding syntax expressions.</returns>
    private static ExpressionSyntax[] GetItemSyntaxes(EquatableArray<TypedConstantInfo> items)
    {
        var itemSyntaxes = new ExpressionSyntax[items.Length];
        for (var index = 0; index < items.Length; index++)
        {
            itemSyntaxes[index] = items[index].GetSyntax();
        }

        return itemSyntaxes;
    }

    /// <summary>Creates a numeric literal token for a primitive value.</summary>
    /// <typeparam name="T">The primitive value type.</typeparam>
    /// <param name="value">The primitive value.</param>
    /// <returns>The matching numeric literal token.</returns>
    private static SyntaxToken GetNumericLiteral<T>(T value)
        where T : unmanaged, IEquatable<T> => value switch
        {
            byte byteValue => Literal(byteValue),
            char characterValue => Literal(characterValue),
            double doubleValue => Literal($"{doubleValue.ToString("R", CultureInfo.InvariantCulture)}D", doubleValue),
            float singleValue => Literal(singleValue),
            int integerValue => Literal(integerValue),
            long longValue => Literal(longValue),
            sbyte signedByteValue => Literal(signedByteValue),
            short shortValue => Literal(shortValue),
            uint unsignedIntegerValue => Literal(unsignedIntegerValue),
            ulong unsignedLongValue => Literal(unsignedLongValue),
            ushort unsignedShortValue => Literal(unsignedShortValue),
            _ => throw new ArgumentException("Invalid primitive type"),
        };

    /// <summary>A <see cref="TypedConstantInfo"/> type representing an array.</summary>
    /// <param name="ElementTypeName">The type name for array elements.</param>
    /// <param name="Items">The sequence of contained elements.</param>
    internal sealed record Array(string ElementTypeName, EquatableArray<TypedConstantInfo> Items) : TypedConstantInfo
    {
        /// <inheritdoc/>
        internal override ExpressionSyntax GetSyntax() => ArrayCreationExpression(
                ArrayType(IdentifierName(ElementTypeName))
                .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))
                .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression)
                .AddExpressions(GetItemSyntaxes(Items)));
    }

    /// <summary>A <see cref="TypedConstantInfo"/> type representing a primitive value.</summary>
    internal abstract record Primitive : TypedConstantInfo
    {
        /// <summary>A <see cref="TypedConstantInfo"/> type representing a <see cref="string"/> value.</summary>
        /// <param name="Value">The input <see cref="string"/> value.</param>
        internal sealed record String(string Value) : TypedConstantInfo
        {
            /// <inheritdoc/>
        internal override ExpressionSyntax GetSyntax() => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Value));
        }

        /// <summary>A <see cref="TypedConstantInfo"/> type representing a <see cref="bool"/> value.</summary>
        /// <param name="Value">The input <see cref="bool"/> value.</param>
        internal sealed record Boolean(bool Value) : TypedConstantInfo
        {
            /// <inheritdoc/>
        internal override ExpressionSyntax GetSyntax() => LiteralExpression(Value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        }

        /// <summary>A <see cref="TypedConstantInfo"/> type representing a generic primitive value.</summary>
        /// <typeparam name="T">The primitive type.</typeparam>
        /// <param name="Value">The input primitive value.</param>
        internal sealed record Of<T>(T Value) : TypedConstantInfo
            where T : unmanaged, IEquatable<T>
        {
            /// <inheritdoc/>
            internal override ExpressionSyntax GetSyntax() =>
                LiteralExpression(SyntaxKind.NumericLiteralExpression, GetNumericLiteral(Value));
        }
    }

    /// <summary>A <see cref="TypedConstantInfo"/> type representing a type.</summary>
    /// <param name="TypeName">The input type name.</param>
    internal sealed record Type(string TypeName) : TypedConstantInfo
    {
        /// <inheritdoc/>
        internal override ExpressionSyntax GetSyntax() => TypeOfExpression(IdentifierName(TypeName));
    }

    /// <summary>A <see cref="TypedConstantInfo"/> type representing an enum value.</summary>
    /// <param name="TypeName">The enum type name.</param>
    /// <param name="Value">The boxed enum value.</param>
    internal sealed record Enum(string TypeName, object Value) : TypedConstantInfo
    {
        /// <inheritdoc/>
        internal override ExpressionSyntax GetSyntax()
        {
            // We let Roslyn parse the value expression, so that it can automatically handle both positive and negative values. This
            // is needed because negative values have a different syntax tree (UnaryMinusExpression holding the numeric expression).
            var valueExpression = ParseExpression(Value.ToString());

            // If the value is negative, we have to put parentheses around them (to avoid CS0075 errors)
            if (valueExpression is PrefixUnaryExpressionSyntax unaryExpression && unaryExpression.IsKind(SyntaxKind.UnaryMinusExpression))
            {
                valueExpression = ParenthesizedExpression(valueExpression);
            }

            // Now we can safely return the cast expression for the target enum type (with optional parentheses if needed)
            return CastExpression(IdentifierName(TypeName), valueExpression);
        }
    }

    /// <summary>A <see cref="TypedConstantInfo"/> type representing a <see langword="null"/> value.</summary>
    internal sealed record Null : TypedConstantInfo
    {
        /// <inheritdoc/>
        internal override ExpressionSyntax GetSyntax() => LiteralExpression(SyntaxKind.NullLiteralExpression);
    }
}
