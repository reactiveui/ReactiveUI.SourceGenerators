// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.BindableDerivedList.Models;
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

    /// <summary>Creates metadata for a BindableDerivedList-annotated field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<BindableDerivedListInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;

        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.BindableDerivedListAttributeType, out var attributeData))
        {
            return default;
        }

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

    /// <summary>Generates the complete source document for a BindableDerivedList declaration.</summary>
    /// <param name="containingTypeName">The name of the containing type.</param>
    /// <param name="containingNamespace">The containing namespace.</param>
    /// <param name="containingClassVisibility">The containing type visibility.</param>
    /// <param name="containingType">The containing type kind.</param>
    /// <param name="properties">The generated property metadata.</param>
    /// <returns>The generated source document.</returns>
    private static string GenerateSource(
        string containingTypeName,
        string containingNamespace,
        string containingClassVisibility,
        string containingType,
        BindableDerivedListInfo[] properties)
    {
        // Get Parent class details from properties.ParentInfo
        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations(GetParentInfos(properties));

        var classes = GenerateClassWithProperties(containingTypeName, containingClassVisibility, containingType, properties);

        return
$$"""
// <auto-generated/>
using System.Collections.ObjectModel;
using DynamicData;
using ReactiveUI;

#pragma warning disable
#nullable enable

namespace {{containingNamespace}}
{
    {{parentClassDeclarationsString}}{{classes}}{{closingBrackets}}
}
#nullable restore
#pragma warning restore
""";
    }

    /// <summary>Generates the source code.</summary>
    /// <param name="containingTypeName">The contain type name.</param>
    /// <param name="containingClassVisibility">The containing class visibility.</param>
    /// <param name="containingType">The containing type.</param>
    /// <param name="properties">The properties.</param>
    /// <returns>The value.</returns>
    private static string GenerateClassWithProperties(
        string containingTypeName,
        string containingClassVisibility,
        string containingType,
        BindableDerivedListInfo[] properties)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = GetPropertyDeclarations(properties);

        return
$$"""

    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
{{propertyDeclarations}}
    }
""";
    }

    /// <summary>Generates property declarations for the given observable method information.</summary>
    /// <param name="propertyInfo">Metadata about the observable property.</param>
    /// <returns>A string containing the generated code for the property.</returns>
    private static string GetPropertySyntax(BindableDerivedListInfo propertyInfo)
    {
        if (propertyInfo.PropertyName is null)
        {
            return string.Empty;
        }

        var propertyAttributes = GetPropertyAttributes(propertyInfo);

        return
$$"""
        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        {{propertyAttributes}}
        {{propertyInfo.AccessModifier}} {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}} => {{propertyInfo.FieldName}};
""";
    }

    /// <summary>Gets parent information for each generated property.</summary>
    /// <param name="properties">The generated property metadata.</param>
    /// <returns>The parent information for the generated properties.</returns>
    private static TargetInfo?[] GetParentInfos(BindableDerivedListInfo[] properties)
    {
        var parentInfos = new TargetInfo?[properties.Length];
        for (var index = 0; index < properties.Length; index++)
        {
            parentInfos[index] = properties[index].TargetInfo.ParentInfo;
        }

        return parentInfos;
    }

    /// <summary>Gets the generated property declarations.</summary>
    /// <param name="properties">The generated property metadata.</param>
    /// <returns>The generated property declarations.</returns>
    private static string GetPropertyDeclarations(BindableDerivedListInfo[] properties)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < properties.Length; index++)
        {
            if (index > 0)
            {
                _ = builder.Append('\n');
            }

            _ = builder.Append(GetPropertySyntax(properties[index]));
        }

        return builder.ToString();
    }

    /// <summary>Gets attributes applied to a generated property.</summary>
    /// <param name="propertyInfo">The source field metadata.</param>
    /// <returns>The property attributes separated by generated-code indentation.</returns>
    private static string GetPropertyAttributes(BindableDerivedListInfo propertyInfo)
    {
        var builder = new StringBuilder();
        foreach (var attribute in AttributeDefinitions.ExcludeFromCodeCoverage)
        {
            AppendPropertyAttribute(builder, attribute);
        }

        foreach (var attribute in propertyInfo.ForwardedAttributes.AsImmutableArray())
        {
            AppendPropertyAttribute(builder, attribute);
        }

        return builder.ToString();
    }

    /// <summary>Appends a generated property attribute with the required separator.</summary>
    /// <param name="builder">The destination builder.</param>
    /// <param name="attribute">The attribute source.</param>
    private static void AppendPropertyAttribute(StringBuilder builder, string attribute)
    {
        if (builder.Length > 0)
        {
            _ = builder.Append("\n        ");
        }

        _ = builder.Append(attribute);
    }
}
