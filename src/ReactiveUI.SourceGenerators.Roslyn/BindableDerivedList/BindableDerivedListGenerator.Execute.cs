// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.BindableDerivedList.Models;
using ReactiveUI.SourceGenerators.CodeGeneration;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>Implements BindableDerivedList source generation.</summary>
public sealed partial class BindableDerivedListGenerator
{
    /// <summary>Gets the generator type name used in generated-code metadata.</summary>
    internal static readonly string GeneratorName = typeof(BindableDerivedListGenerator).FullName!;

    /// <summary>Gets the generator assembly version used in generated-code metadata.</summary>
    internal static readonly string GeneratorVersion = typeof(BindableDerivedListGenerator).Assembly.GetName().Version.ToString();

    /// <summary>Represents the internal accessibility option.</summary>
    private const int InternalAccessModifier = 2;

    /// <summary>Represents the private accessibility option.</summary>
    private const int PrivateAccessModifier = 3;

    /// <summary>Represents the protected-internal accessibility option.</summary>
    private const int ProtectedInternalAccessModifier = 4;

    /// <summary>Represents the private-protected accessibility option.</summary>
    private const int PrivateProtectedAccessModifier = 5;

    /// <summary>The <c>GeneratedCode</c> attribute stamped on the generated properties, built once.</summary>
    private static readonly string GeneratedCodeAttribute = SourceWriterExtensions.GeneratedCodeAttribute(GeneratorName, GeneratorVersion);

    /// <summary>Creates metadata for a BindableDerivedList-annotated field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<BindableDerivedListInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;

        var attributeData = context.Attributes[0];

        if (symbol is not IFieldSymbol fieldSymbol)
        {
            return default;
        }

        token.ThrowIfCancellationRequested();

        if (!fieldSymbol.Type.HasOrInheritsFromFullyQualifiedMetadataNameStartingWith("System.Collections.ObjectModel.ReadOnlyObservableCollection"))
        {
            builder.Add(
                ReadOnlyObservableCollectionTypeRequiredError,
                fieldSymbol,
                fieldSymbol.ContainingType,
                fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        token.ThrowIfCancellationRequested();

        return CreateResult(context, fieldSymbol, attributeData, builder, token);
    }

    /// <summary>Creates metadata for a validated BindableDerivedList field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="fieldSymbol">The validated field symbol.</param>
    /// <param name="attributeData">The BindableDerivedList attribute data.</param>
    /// <param name="builder">The diagnostics collected while processing the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics.</returns>
    private static Result<BindableDerivedListInfo?> CreateResult(
        in GeneratorAttributeSyntaxContext context,
        IFieldSymbol fieldSymbol,
        AttributeData attributeData,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        CancellationToken token)
    {
        var typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();

        var fieldName = fieldSymbol.Name;
        var propertyName = fieldSymbol.GetGeneratedPropertyName();

        if (fieldName == propertyName)
        {
            builder.Add(
                    ReactivePropertyNameCollisionError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        token.ThrowIfCancellationRequested();

        var accessModifier = GetAccessModifier(attributeData);

        token.ThrowIfCancellationRequested();

        var (isReferenceTypeOrUnconstraindTypeParameter, includeMemberNotNullOnSetAccessor) =
            GetNullabilityInfo(fieldSymbol, context.SemanticModel);

        token.ThrowIfCancellationRequested();
        var fieldDeclaration = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;

        context.GetForwardedAttributes(
            builder,
            fieldSymbol,
            fieldDeclaration.AttributeLists,
            token,
            out var forwardedAttributesString);

        token.ThrowIfCancellationRequested();

        // Get the containing type info
        var targetInfo = TargetInfo.From(fieldSymbol.ContainingType);

        token.ThrowIfCancellationRequested();

        return new(
            new(
            targetInfo,
            typeNameWithNullabilityAnnotations,
            fieldName,
            propertyName,
            isReferenceTypeOrUnconstraindTypeParameter,
            includeMemberNotNullOnSetAccessor,
            forwardedAttributesString,
            accessModifier),
            builder.ToImmutable());
    }

    /// <summary>Gets the generated accessibility text from the attribute configuration.</summary>
    /// <param name="attributeData">The BindableDerivedList attribute data.</param>
    /// <returns>The generated accessibility text.</returns>
    private static string GetAccessModifier(AttributeData attributeData) =>
        attributeData.GetNamedArgument<int>("AccessModifier") switch
        {
            1 => "protected",
            InternalAccessModifier => "internal",
            PrivateAccessModifier => "private",
            ProtectedInternalAccessModifier => "protected internal",
            PrivateProtectedAccessModifier => "private protected",
            _ => "public",
        };

    /// <summary>Gets nullability metadata for a generated BindableDerivedList property.</summary>
    /// <param name="fieldSymbol">The source field symbol.</param>
    /// <param name="semanticModel">The semantic model for the source field.</param>
    /// <returns>The reference-type and member-not-null metadata.</returns>
    private static (bool IsReferenceType, bool IncludeMemberNotNull) GetNullabilityInfo(
        IFieldSymbol fieldSymbol,
        SemanticModel semanticModel)
    {
        fieldSymbol.GetNullabilityInfo(
            semanticModel,
            out var isReferenceTypeOrUnconstraindTypeParameter,
            out var includeMemberNotNullOnSetAccessor);
        return (isReferenceTypeOrUnconstraindTypeParameter, includeMemberNotNullOnSetAccessor);
    }

    /// <summary>Generates the partial declaration holding one type's bindable derived list properties.</summary>
    /// <param name="properties">The properties the type declares, all sharing one <see cref="TargetInfo"/>.</param>
    /// <returns>The file's text.</returns>
    private static string GenerateSource(List<BindableDerivedListInfo> properties)
    {
        var target = properties[0].TargetInfo;
        var writer = SourceWriter.Rent()
            .AutoGenerated()
            .Using("System.Collections.ObjectModel")
            .Using("DynamicData")
            .Using("ReactiveUI")
            .BlankLine()
            .DisableWarningsEnableNullable()
            .BlankLine();

        var depth = writer.OpenNamespace(target.TargetNamespace) + writer.OpenContainingTypes(target.ParentInfo);

        // The GeneratedCode attribute has always been stamped once, ahead of the first property.
        _ = writer.OpenPartialType(target).Line(GeneratedCodeAttribute);
        for (var i = 0; i < properties.Count; i++)
        {
            WriteProperty(writer, properties[i]);
        }

        return writer.CloseBlock()
            .CloseBlocks(depth)
            .RestoreNullableAndWarnings()
            .ToStringAndReturn();
    }

    /// <summary>Writes one read-only property exposing a bindable derived list field.</summary>
    /// <param name="writer">The writer, at the level of the type's members.</param>
    /// <param name="propertyInfo">The property.</param>
    private static void WriteProperty(SourceWriter writer, BindableDerivedListInfo propertyInfo)
    {
        _ = writer.InheritDoc(propertyInfo.FieldName).ExcludeFromCodeCoverage();
        foreach (var attribute in propertyInfo.ForwardedAttributes.AsImmutableArray())
        {
            _ = writer.Line(attribute);
        }

        _ = writer.Append(propertyInfo.AccessModifier).Append(' ')
            .Append(propertyInfo.TypeNameWithNullabilityAnnotations).Append(' ')
            .Append(propertyInfo.PropertyName).Append(" => ").Append(propertyInfo.FieldName).EndStatement();
    }
}
