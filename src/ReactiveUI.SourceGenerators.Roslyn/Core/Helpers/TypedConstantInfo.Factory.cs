// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <inheritdoc/>
internal sealed partial record TypedConstantInfo
{
    /// <summary>Creates a new <see cref="TypedConstantInfo"/> instance from a given <see cref="TypedConstant"/> value.</summary>
    /// <param name="arg">The input <see cref="TypedConstant"/> value.</param>
    /// <returns>A <see cref="TypedConstantInfo"/> instance representing <paramref name="arg"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the input argument is not valid.</exception>
    internal static TypedConstantInfo Create(TypedConstant arg)
    {
        if (arg.IsNull)
        {
            return Null();
        }

        if (arg.Kind == TypedConstantKind.Array)
        {
            var elementTypeName = ((IArrayTypeSymbol)arg.Type!).ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            using var items = ArrayBuilder<TypedConstantInfo>.Rent();
            foreach (var value in arg.Values)
            {
                items.Add(Create(value));
            }

            return OfArray(elementTypeName, items.WrittenSpan);
        }

        return arg.Kind switch
        {
            TypedConstantKind.Type => OfType(((ITypeSymbol)arg.Value!).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
            TypedConstantKind.Enum => OfEnum(arg.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), arg.Value!),
            _ => OfPrimitive(arg.Value!),
        };
    }

    /// <summary>Creates a new <see cref="TypedConstantInfo"/> instance from a given <see cref="IOperation"/> value.</summary>
    /// <param name="operation">The input <see cref="IOperation"/> value.</param>
    /// <param name="info">The resulting <see cref="TypedConstantInfo"/> instance, if available.</param>
    /// <returns>Whether a resulting <see cref="TypedConstantInfo"/> instance could be created.</returns>
    /// <exception cref="ArgumentException">Thrown if the operation is a constant of an unsupported type.</exception>
    internal static bool TryCreate(IOperation operation, [NotNullWhen(true)] out TypedConstantInfo? info)
    {
        if (operation.ConstantValue.HasValue)
        {
            info = OfConstant(operation);
            return true;
        }

        if (operation is ITypeOfOperation typeOfOperation)
        {
            info = OfType(typeOfOperation.TypeOperand.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            return true;
        }

        if (operation is IArrayCreationOperation arrayCreation)
        {
            return TryCreateArray(arrayCreation, out info);
        }

        info = null;
        return false;
    }

    /// <summary>Creates a typed constant from an operation that has a constant value.</summary>
    /// <param name="operation">The constant operation.</param>
    /// <returns>The constant.</returns>
    /// <exception cref="ArgumentException">Thrown if the constant is a primitive of an unsupported type.</exception>
    private static TypedConstantInfo OfConstant(IOperation operation)
    {
        var value = operation.ConstantValue.Value;
        if (value is null)
        {
            return Null();
        }

        return operation.Type!.TypeKind is TypeKind.Enum
            ? OfEnum(operation.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), value)
            : OfPrimitive(value);
    }

    /// <summary>Creates a typed constant from a boxed string, Boolean, number or character.</summary>
    /// <param name="value">The boxed value.</param>
    /// <returns>The constant.</returns>
    /// <exception cref="ArgumentException">Thrown if the value is not a supported primitive type.</exception>
    private static TypedConstantInfo OfPrimitive(object value) => value switch
    {
        string text => OfString(text),
        bool flag => OfBoolean(flag),
        byte byteValue => OfNumber(byteValue),
        char characterValue => OfNumber(characterValue),
        double doubleValue => OfNumber(doubleValue),
        float singleValue => OfNumber(singleValue),
        int integerValue => OfNumber(integerValue),
        long longValue => OfNumber(longValue),
        sbyte signedByteValue => OfNumber(signedByteValue),
        short shortValue => OfNumber(shortValue),
        uint unsignedIntegerValue => OfNumber(unsignedIntegerValue),
        ulong unsignedLongValue => OfNumber(unsignedLongValue),
        ushort unsignedShortValue => OfNumber(unsignedShortValue),
        _ => throw new ArgumentException("Invalid primitive type"),
    };

    /// <summary>Creates a typed constant from an array creation.</summary>
    /// <param name="arrayCreation">The array creation.</param>
    /// <param name="info">The resulting constant information, when successful.</param>
    /// <returns><see langword="true"/> when all array items can be represented.</returns>
    private static bool TryCreateArray(IArrayCreationOperation arrayCreation, [NotNullWhen(true)] out TypedConstantInfo? info)
    {
        var elementTypeName = ((IArrayTypeSymbol)arrayCreation.Type!).ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        using var items = ArrayBuilder<TypedConstantInfo>.Rent();

        // An array created with a size and no initializer has no elements to write.
        if (arrayCreation.Initializer is { } initializer)
        {
            foreach (var element in initializer.ElementValues)
            {
                if (!TryCreate(WithoutImplicitConversions(element), out var item))
                {
                    info = null;
                    return false;
                }

                items.Add(item);
            }
        }

        info = OfArray(elementTypeName, items.WrittenSpan);
        return true;
    }

    /// <summary>Gets the operation an element was written as, under the conversions the compiler added.</summary>
    /// <param name="operation">The element operation.</param>
    /// <returns>The operation for the element's own expression.</returns>
    private static IOperation WithoutImplicitConversions(IOperation operation)
    {
        // The element is written back as it was typed, so an added conversion to the element type is dropped.
        while (operation is IConversionOperation { IsImplicit: true } conversion)
        {
            operation = conversion.Operand;
        }

        return operation;
    }
}
