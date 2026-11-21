// Copyright (c) 2019-2026 ReactiveUI and Contributors. All rights reserved.
// ReactiveUI and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;

namespace ReactiveUI.SourceGenerators.Helpers;

/// <summary>A model representing an attribute declaration.</summary>
/// <param name="TypeName">The type name of the attribute.</param>
/// <param name="ConstructorArgumentInfo">The <see cref="TypedConstantInfo"/> values for all constructor arguments for the attribute.</param>
/// <param name="NamedArgumentInfo">The <see cref="TypedConstantInfo"/> values for all named arguments for the attribute.</param>
internal sealed record AttributeInfo(
    string TypeName,
    EquatableArray<TypedConstantInfo> ConstructorArgumentInfo,
    EquatableArray<(string Name, TypedConstantInfo Value)> NamedArgumentInfo)
{
    /// <inheritdoc/>
    public override string ToString() => Write(this);

    /// <summary>Creates a new <see cref="AttributeInfo"/> instance from a given <see cref="AttributeData"/> value.</summary>
    /// <param name="attributeData">The input <see cref="AttributeData"/> value.</param>
    /// <returns>A <see cref="AttributeInfo"/> instance representing <paramref name="attributeData"/>.</returns>
    internal static AttributeInfo Create(AttributeData attributeData)
    {
        var typeName = attributeData.AttributeClass!.GetFullyQualifiedName();

        using var constructorArguments = ArrayBuilder<TypedConstantInfo>.Rent();
        using var namedArguments = ArrayBuilder<(string, TypedConstantInfo)>.Rent();

        // Get the constructor arguments
        foreach (var typedConstant in attributeData.ConstructorArguments)
        {
            constructorArguments.Add(TypedConstantInfo.Create(typedConstant));
        }

        // Get the named arguments
        foreach (var namedConstant in attributeData.NamedArguments)
        {
            namedArguments.Add((namedConstant.Key, TypedConstantInfo.Create(namedConstant.Value)));
        }

        return new(
            typeName,
            constructorArguments.ToEquatableArray(),
            namedArguments.ToEquatableArray());
    }

    /// <summary>Creates a new <see cref="AttributeInfo"/> instance from a given syntax node.</summary>
    /// <param name="typeSymbol">The symbol for the attribute type.</param>
    /// <param name="semanticModel">The <see cref="SemanticModel"/> instance for the current run.</param>
    /// <param name="arguments">The sequence of <see cref="AttributeArgumentSyntax"/> instances to process.</param>
    /// <param name="token">The cancellation token for the current operation.</param>
    /// <param name="info">The resulting <see cref="AttributeInfo"/> instance, if available.</param>
    /// <returns>Whether a resulting <see cref="AttributeInfo"/> instance could be created.</returns>
    internal static bool TryCreate(
        INamedTypeSymbol typeSymbol,
        SemanticModel semanticModel,
        IEnumerable<AttributeArgumentSyntax> arguments,
        CancellationToken token,
        [NotNullWhen(true)] out AttributeInfo? info)
    {
        var typeName = typeSymbol.GetFullyQualifiedName();

        using var constructorArguments = ArrayBuilder<TypedConstantInfo>.Rent();
        using var namedArguments = ArrayBuilder<(string, TypedConstantInfo)>.Rent();

        foreach (var argument in arguments)
        {
            // The attribute expression has to have an available operation to extract information from
            if (semanticModel.GetOperation(argument.Expression, token) is not IOperation operation)
            {
                continue;
            }

            // Try to get the info for the current argument
            if (!TypedConstantInfo.TryCreate(operation, out var argumentInfo))
            {
                info = null;

                return false;
            }

            // Try to get the identifier name if the current expression is a named argument expression. If it
            // isn't, then the expression is a normal attribute constructor argument, so no extra work is needed.
            if (argument.NameEquals is null)
            {
                constructorArguments.Add(argumentInfo);
            }
            else
            {
                namedArguments.Add((argument.NameEquals.Name.Identifier.ValueText, argumentInfo));
            }
        }

        info = new(
            typeName,
            constructorArguments.ToEquatableArray(),
            namedArguments.ToEquatableArray());

        return true;
    }

    /// <summary>Writes an attribute as the bracketed C# that applies it.</summary>
    /// <param name="attribute">The attribute.</param>
    /// <returns>The attribute list text, with constructor arguments first and named arguments after them.</returns>
    private static string Write(AttributeInfo attribute)
    {
        const int ArgumentCapacity = 32;
        var constructorArguments = attribute.ConstructorArgumentInfo.AsSpan();
        var namedArguments = attribute.NamedArgumentInfo.AsSpan();
        var builder = PooledBuilder.Rent(attribute.TypeName.Length + ((constructorArguments.Length + namedArguments.Length) * ArgumentCapacity));
        _ = builder.Append('[').Append(attribute.TypeName).Append('(');

        var separator = string.Empty;
        for (var index = 0; index < constructorArguments.Length; index++)
        {
            _ = builder.Append(separator).Append(constructorArguments[index].Expression);
            separator = ",";
        }

        for (var index = 0; index < namedArguments.Length; index++)
        {
            _ = builder.Append(separator).Append(namedArguments[index].Name).Append('=').Append(namedArguments[index].Value.Expression);
            separator = ",";
        }

        return PooledBuilder.ToStringAndReturn(builder.Append(")]"));
    }
}
