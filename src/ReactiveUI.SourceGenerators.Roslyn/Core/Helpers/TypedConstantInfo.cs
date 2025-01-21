// Copyright (c) 2024 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>
/// A model representing a typed constant item.
/// </summary>
/// <remarks>This model is fully serializable and comparable.</remarks>
internal abstract partial record TypedConstantInfo
{
    /// <summary>
    /// Gets an <see cref="ExpressionSyntax"/> instance representing the current constant.
    /// </summary>
    /// <returns>The <see cref="ExpressionSyntax"/> instance representing the current constant.</returns>
    public abstract ExpressionSyntax GetSyntax();

    /// <summary>
    /// A <see cref="TypedConstantInfo"/> type representing an array.
    /// </summary>
    /// <param name="ElementTypeName">The type name for array elements.</param>
    /// <param name="Items">The sequence of contained elements.</param>
    public sealed record Array(string ElementTypeName, EquatableArray<TypedConstantInfo> Items) : TypedConstantInfo
    {
        /// <inheritdoc/>
        public override ExpressionSyntax GetSyntax() => ArrayCreationExpression(
                ArrayType(IdentifierName(ElementTypeName))
                .AddRankSpecifiers(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))
                .WithInitializer(InitializerExpression(SyntaxKind.ArrayInitializerExpression)
                .AddExpressions(Items.Select(static c => c.GetSyntax()).ToArray()));
    }

    /// <summary>
    /// A <see cref="TypedConstantInfo"/> type representing a primitive value.
    /// </summary>
    public abstract record Primitive : TypedConstantInfo
    {
        /// <summary>
        /// A <see cref="TypedConstantInfo"/> type representing a <see cref="string"/> value.
        /// </summary>
        /// <param name="Value">The input <see cref="string"/> value.</param>
        public sealed record String(string Value) : TypedConstantInfo
        {
            /// <inheritdoc/>
            public override ExpressionSyntax GetSyntax() => LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(Value));
        }

        /// <summary>
        /// A <see cref="TypedConstantInfo"/> type representing a <see cref="bool"/> value.
        /// </summary>
        /// <param name="Value">The input <see cref="bool"/> value.</param>
        public sealed record Boolean(bool Value) : TypedConstantInfo
        {
            /// <inheritdoc/>
            public override ExpressionSyntax GetSyntax() => LiteralExpression(Value ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
        }

        /// <summary>
        /// A <see cref="TypedConstantInfo"/> type representing a generic primitive value.
        /// </summary>
        /// <typeparam name="T">The primitive type.</typeparam>
        /// <param name="Value">The input primitive value.</param>
        public sealed record Of<T>(T Value) : TypedConstantInfo
            where T : unmanaged, IEquatable<T>
        {
            /// <inheritdoc/>
            public override ExpressionSyntax GetSyntax() =>
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Value switch { byte b => Literal(b), char c => Literal(c), double d => Literal(d.ToString("R", CultureInfo.InvariantCulture) + "D", d), float f => Literal(f), int i => Literal(i), long l => Literal(l), sbyte sb => Literal(sb), short sh => Literal(sh), uint ui => Literal(ui), ulong ul => Literal(ul), ushort ush => Literal(ush), _ => throw new ArgumentException("Invalid primitive type") });
        }
    }

    /// <summary>
    /// A <see cref="TypedConstantInfo"/> type representing a type.
    /// </summary>
    /// <param name="TypeName">The input type name.</param>
    public sealed record Type(string TypeName) : TypedConstantInfo
    {
        /// <inheritdoc/>
        public override ExpressionSyntax GetSyntax() => TypeOfExpression(IdentifierName(TypeName));
    }

    /// <summary>
    /// A <see cref="TypedConstantInfo"/> type representing an enum value.
    /// </summary>
    /// <param name="TypeName">The enum type name.</param>
    /// <param name="Value">The boxed enum value.</param>
    public sealed record Enum(string TypeName, object Value) : TypedConstantInfo
    {
        /// <inheritdoc/>
        public override ExpressionSyntax GetSyntax()
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

    /// <summary>
    /// A <see cref="TypedConstantInfo"/> type representing a <see langword="null"/> value.
    /// </summary>
    public sealed record Null : TypedConstantInfo
    {
        /// <inheritdoc/>
        public override ExpressionSyntax GetSyntax() => LiteralExpression(SyntaxKind.NullLiteralExpression);
    }
}
