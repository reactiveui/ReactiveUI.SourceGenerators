// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <inheritdoc/>
internal abstract partial record TypedConstantInfo
{
    /// <summary>Creates a new <see cref="TypedConstantInfo"/> instance from a given <see cref="TypedConstant"/> value.</summary>
    /// <param name="arg">The input <see cref="TypedConstant"/> value.</param>
    /// <returns>A <see cref="TypedConstantInfo"/> instance representing <paramref name="arg"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the input argument is not valid.</exception>
    internal static TypedConstantInfo Create(TypedConstant arg)
    {
        if (arg.IsNull)
        {
            return new Null();
        }

        if (arg.Kind == TypedConstantKind.Array)
        {
            var elementTypeName = ((IArrayTypeSymbol)arg.Type!).ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            using var items = ImmutableArrayBuilder<TypedConstantInfo>.Rent();
            foreach (var value in arg.Values)
            {
                items.Add(Create(value));
            }

            return new Array(elementTypeName, items.ToImmutable());
        }

        return (arg.Kind, arg.Value) switch
        {
            (TypedConstantKind.Primitive, string text) => new Primitive.String(text),
            (TypedConstantKind.Primitive, bool flag) => new Primitive.Boolean(flag),
            (TypedConstantKind.Primitive, object value) => value switch
            {
                byte b => new Primitive.Of<byte>(b),
                char c => new Primitive.Of<char>(c),
                double d => new Primitive.Of<double>(d),
                float f => new Primitive.Of<float>(f),
                int i => new Primitive.Of<int>(i),
                long l => new Primitive.Of<long>(l),
                sbyte sb => new Primitive.Of<sbyte>(sb),
                short sh => new Primitive.Of<short>(sh),
                uint ui => new Primitive.Of<uint>(ui),
                ulong ul => new Primitive.Of<ulong>(ul),
                ushort ush => new Primitive.Of<ushort>(ush),
                _ => throw new ArgumentException("Invalid primitive type")
            },
            (TypedConstantKind.Type, ITypeSymbol type) => new Type(type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
            (TypedConstantKind.Enum, object value) => new Enum(arg.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), value),
            _ => throw new ArgumentException("Invalid typed constant type"),
        };
    }

    /// <summary>Creates a new <see cref="TypedConstantInfo"/> instance from a given <see cref="IOperation"/> value.</summary>
    /// <param name="operation">The input <see cref="IOperation"/> value.</param>
    /// <param name="semanticModel">The <see cref="SemanticModel"/> that was used to retrieve <paramref name="operation"/>.</param>
    /// <param name="expression">The <see cref="ExpressionSyntax"/> that <paramref name="operation"/> was retrieved from.</param>
    /// <param name="token">The cancellation token for the current operation.</param>
    /// <param name="info">The resulting <see cref="TypedConstantInfo"/> instance, if available.</param>
    /// <returns>Whether a resulting <see cref="TypedConstantInfo"/> instance could be created.</returns>
    /// <exception cref="ArgumentException">Thrown if the input argument is not valid.</exception>
    internal static bool TryCreate(
        IOperation operation,
        SemanticModel semanticModel,
        ExpressionSyntax expression,
        CancellationToken token,
        [NotNullWhen(true)] out TypedConstantInfo? info)
    {
        if (TryCreateConstant(operation, out info))
        {
            return true;
        }

        if (operation is ITypeOfOperation typeOfOperation)
        {
            info = new Type(typeOfOperation.TypeOperand.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

            return true;
        }

        if (operation is IArrayCreationOperation && TryCreateArray(operation, semanticModel, expression, token, out info))
        {
            return true;
        }

        info = null;

        return false;
    }

    /// <summary>Creates a typed constant from a constant operation.</summary>
    /// <param name="operation">The operation to inspect.</param>
    /// <param name="info">The resulting constant information, when successful.</param>
    /// <returns><see langword="true"/> when the operation has a supported constant value.</returns>
    private static bool TryCreateConstant(
        IOperation operation,
        [NotNullWhen(true)] out TypedConstantInfo? info)
    {
        if (!operation.ConstantValue.HasValue)
        {
            info = null;
            return false;
        }

        if (operation.Type?.TypeKind is TypeKind.Enum)
        {
            info = new Enum(
                operation.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                operation.ConstantValue.Value!);
            return true;
        }

        info = operation.ConstantValue.Value switch
        {
            null => new Null(),
            string text => new Primitive.String(text),
            bool flag => new Primitive.Boolean(flag),
            byte byteValue => new Primitive.Of<byte>(byteValue),
            char characterValue => new Primitive.Of<char>(characterValue),
            double doubleValue => new Primitive.Of<double>(doubleValue),
            float singleValue => new Primitive.Of<float>(singleValue),
            int integerValue => new Primitive.Of<int>(integerValue),
            long longValue => new Primitive.Of<long>(longValue),
            sbyte signedByteValue => new Primitive.Of<sbyte>(signedByteValue),
            short shortValue => new Primitive.Of<short>(shortValue),
            uint unsignedIntegerValue => new Primitive.Of<uint>(unsignedIntegerValue),
            ulong unsignedLongValue => new Primitive.Of<ulong>(unsignedLongValue),
            ushort unsignedShortValue => new Primitive.Of<ushort>(unsignedShortValue),
            _ => throw new ArgumentException("Invalid primitive type"),
        };
        return true;
    }

    /// <summary>Creates a typed constant from an array operation.</summary>
    /// <param name="operation">The array operation to inspect.</param>
    /// <param name="semanticModel">The semantic model containing the operation.</param>
    /// <param name="expression">The array expression.</param>
    /// <param name="token">The cancellation token for the current operation.</param>
    /// <param name="info">The resulting constant information, when successful.</param>
    /// <returns><see langword="true"/> when all array items can be represented.</returns>
    private static bool TryCreateArray(
        IOperation operation,
        SemanticModel semanticModel,
        ExpressionSyntax expression,
        CancellationToken token,
        [NotNullWhen(true)] out TypedConstantInfo? info)
    {
        var elementTypeName = ((IArrayTypeSymbol?)operation.Type)?.ElementType
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? "object";
        var initializer = GetInitializer(expression);
        if (initializer is null)
        {
            info = new Array(elementTypeName, ImmutableArray<TypedConstantInfo>.Empty);
            return true;
        }

        using var items = ImmutableArrayBuilder<TypedConstantInfo>.Rent();
        foreach (var itemExpression in initializer.Expressions)
        {
            if (!TryCreateArrayItem(itemExpression, semanticModel, token, out var item))
            {
                info = null;
                return false;
            }

            items.Add(item);
        }

        info = new Array(elementTypeName, items.ToImmutable());
        return true;
    }

    /// <summary>Gets the initializer from an explicit or implicit array expression.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns>The array initializer, if present.</returns>
    private static InitializerExpressionSyntax? GetInitializer(ExpressionSyntax expression) => expression switch
    {
        ImplicitArrayCreationExpressionSyntax implicitArray => implicitArray.Initializer,
        ArrayCreationExpressionSyntax array => array.Initializer,
        _ => null,
    };

    /// <summary>Creates typed constant information for an array item.</summary>
    /// <param name="expression">The array item expression.</param>
    /// <param name="semanticModel">The semantic model containing the expression.</param>
    /// <param name="token">The cancellation token for the current operation.</param>
    /// <param name="item">The resulting item information, when successful.</param>
    /// <returns><see langword="true"/> when the item can be represented.</returns>
    private static bool TryCreateArrayItem(
        ExpressionSyntax expression,
        SemanticModel semanticModel,
        CancellationToken token,
        [NotNullWhen(true)] out TypedConstantInfo? item)
    {
        if (semanticModel.GetOperation(expression, token) is IOperation operation)
        {
            return TryCreate(operation, semanticModel, expression, token, out item);
        }

        item = null;
        return false;
    }
}
