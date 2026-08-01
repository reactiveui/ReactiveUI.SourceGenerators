// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>Implements ObservableAsProperty generation from annotated fields.</summary>
/// <seealso cref="IIncrementalGenerator" />
public sealed partial class ObservableAsPropertyGenerator
{
    /// <summary>Creates metadata for an ObservableAsProperty-annotated field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<ObservableFieldInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ObservableAsPropertyAttributeType, out var attributeData))
        {
            return default;
        }

        if (symbol is not IFieldSymbol fieldSymbol)
        {
            return default;
        }

        if (!fieldSymbol.IsTargetTypeValid())
        {
            builder.Add(
                    InvalidReactiveObjectError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        return CreateResult(context, fieldSymbol, attributeData, builder, token);
    }

    /// <summary>Creates metadata for a validated ObservableAsProperty field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="fieldSymbol">The validated field symbol.</param>
    /// <param name="attributeData">The ObservableAsProperty attribute data.</param>
    /// <param name="builder">The diagnostics collected while processing the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics.</returns>
    private static Result<ObservableFieldInfo?> CreateResult(
        in GeneratorAttributeSyntaxContext context,
        IFieldSymbol fieldSymbol,
        AttributeData attributeData,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        CancellationToken token)
    {
        var (isReadonly, useProtectedModifier, inheritance) = GetAttributeOptions(attributeData);

        token.ThrowIfCancellationRequested();

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

        var fieldDeclaration = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;
        var variables = fieldDeclaration.Declaration.Variables;
        var initializer = variables.Count > 0 ? variables[0].Initializer?.ToFullString() : null;

        token.ThrowIfCancellationRequested();

        context.GetForwardedAttributes(
                builder,
                fieldSymbol,
                fieldDeclaration.AttributeLists,
                token,
                out var forwardedPropertyAttributes);

        token.ThrowIfCancellationRequested();

        var (isReferenceTypeOrUnconstraindTypeParameter, includeMemberNotNullOnSetAccessor) =
            GetNullabilityInfo(fieldSymbol, context.SemanticModel);

        token.ThrowIfCancellationRequested();

        var targetInfo = TargetInfo.From(fieldSymbol.ContainingType);

        return new(
            new(
            targetInfo,
            typeNameWithNullabilityAnnotations,
            fieldName,
            propertyName,
            initializer,
            isReferenceTypeOrUnconstraindTypeParameter,
            includeMemberNotNullOnSetAccessor,
            forwardedPropertyAttributes,
            isReadonly == false ? string.Empty : "readonly",
            useProtectedModifier,
            inheritance),
            builder.ToImmutable());
    }

    /// <summary>Gets property-generation options from an ObservableAsProperty attribute.</summary>
    /// <param name="attributeData">The ObservableAsProperty attribute data.</param>
    /// <returns>The read-only flag, accessibility, and inheritance modifier.</returns>
    private static (bool? IsReadonly, string Accessibility, string Inheritance) GetAttributeOptions(AttributeData attributeData)
    {
        _ = attributeData.TryGetNamedArgument("ReadOnly", out bool? isReadonly);
        _ = attributeData.TryGetNamedArgument("Inheritance", out int inheritanceArgument);
        _ = attributeData.TryGetNamedArgument("UseProtected", out bool useProtected);

        const int OverrideInheritance = 2;
        const int NewInheritance = 3;
        var inheritance = inheritanceArgument switch
        {
            1 => " virtual",
            OverrideInheritance => " override",
            NewInheritance => " new",
            _ => string.Empty,
        };

        return (isReadonly, useProtected ? "protected" : "private", inheritance);
    }

    /// <summary>Gets nullability metadata for a generated observable property.</summary>
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

    /// <summary>Generates the complete source document for field-backed observable properties.</summary>
    /// <param name="containingTypeName">The name of the containing type.</param>
    /// <param name="containingNamespace">The containing namespace.</param>
    /// <param name="containingClassVisibility">The containing type visibility.</param>
    /// <param name="containingType">The containing type kind.</param>
    /// <param name="properties">The generated property metadata.</param>
    /// <param name="integration">The selected ReactiveUI integration metadata.</param>
    /// <returns>The generated source document.</returns>
    private static string GenerateSource(
        string containingTypeName,
        string containingNamespace,
        string containingClassVisibility,
        string containingType,
        ObservableFieldInfo[] properties,
        ReactiveUiIntegration integration)
    {
        // Get Parent class details from properties.ParentInfo
        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations(GetParentInfos(properties));

        var classes = GenerateClassWithProperties(containingTypeName, containingClassVisibility, containingType, properties, integration.DeclarationNamespace);

        return $$"""
// <auto-generated/>
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
    /// <param name="reactiveUiNamespace">The namespace containing the selected ReactiveUI implementation types.</param>
    /// <returns>The value.</returns>
    private static string GenerateClassWithProperties(
        string containingTypeName,
        string containingClassVisibility,
        string containingType,
        ObservableFieldInfo[] properties,
        string reactiveUiNamespace)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = GetPropertyDeclarations(properties, reactiveUiNamespace);

        return
$$"""

    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
{{propertyDeclarations}}
    }
""";
    }

    /// <summary>Generates a property declaration for an observable field.</summary>
    /// <param name="propertyInfo">The source field metadata.</param>
    /// <param name="reactiveUiNamespace">The ReactiveUI implementation namespace.</param>
    /// <returns>The generated property declaration.</returns>
    private static string GetPropertySyntax(ObservableFieldInfo propertyInfo, string reactiveUiNamespace)
    {
        var propertyAttributes = GetPropertyAttributes(propertyInfo);

        var getter = $$"""{ get => {{propertyInfo.FieldName}} = {{propertyInfo.FieldName}}Helper?.Value ?? {{propertyInfo.FieldName}}; }""";

        // If the property is nullable, we need to add a null check to the getter
        if (propertyInfo.TypeNameWithNullabilityAnnotations.EndsWith("?", System.StringComparison.Ordinal))
        {
            getter = $$"""{ get => {{propertyInfo.FieldName}} = ({{propertyInfo.FieldName}}Helper == null ? {{propertyInfo.FieldName}} : {{propertyInfo.FieldName}}Helper.Value); }""";
        }

        var helperTypeName = $"{propertyInfo.AccessModifier} {reactiveUiNamespace}.ObservableAsPropertyHelper<{propertyInfo.TypeNameWithNullabilityAnnotations}>?";

        // If the property is readonly, we need to change the helper to be non-nullable
        if (propertyInfo.IsReadOnly == "readonly")
        {
            helperTypeName = $"{propertyInfo.AccessModifier} readonly {reactiveUiNamespace}.ObservableAsPropertyHelper<{propertyInfo.TypeNameWithNullabilityAnnotations}>";
        }

        return $$"""
        /// <inheritdoc cref="{{propertyInfo.FieldName}}Helper"/>
        {{helperTypeName}} {{propertyInfo.FieldName}}Helper;

        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
        {{propertyAttributes}}
        public{{propertyInfo.Inheritance}} {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}} {{getter}}
""";
    }

    /// <summary>Gets parent information for each generated property.</summary>
    /// <param name="properties">The generated property metadata.</param>
    /// <returns>The parent information for the generated properties.</returns>
    private static TargetInfo?[] GetParentInfos(ObservableFieldInfo[] properties)
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
    /// <param name="reactiveUiNamespace">The ReactiveUI implementation namespace.</param>
    /// <returns>The generated property declarations.</returns>
    private static string GetPropertyDeclarations(ObservableFieldInfo[] properties, string reactiveUiNamespace)
    {
        var builder = new StringBuilder();
        for (var index = 0; index < properties.Length; index++)
        {
            if (index > 0)
            {
                _ = builder.Append('\n');
            }

            _ = builder.Append(GetPropertySyntax(properties[index], reactiveUiNamespace));
        }

        return builder.ToString();
    }

    /// <summary>Gets attributes applied to a generated property.</summary>
    /// <param name="propertyInfo">The source field metadata.</param>
    /// <returns>The property attributes separated by generated-code indentation.</returns>
    private static string GetPropertyAttributes(ObservableFieldInfo propertyInfo)
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
