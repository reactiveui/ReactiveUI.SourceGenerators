// Copyright (c) 2019-2026 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReactiveUI.SourceGenerators.Extensions;
using ReactiveUI.SourceGenerators.Helpers;
using ReactiveUI.SourceGenerators.Models;
using static ReactiveUI.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace ReactiveUI.SourceGenerators;

/// <summary>Implements ReactiveCollection source generation.</summary>
public sealed partial class ReactiveCollectionGenerator
{
    /// <summary>Creates metadata for a ReactiveCollection-annotated field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics, or <see langword="null"/> when not applicable.</returns>
    private static Result<ReactiveCollectionFieldInfo?>? GetVariableInfo(in GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        using var builder = ImmutableArrayBuilder<DiagnosticInfo>.Rent();
        var symbol = context.TargetSymbol;
        token.ThrowIfCancellationRequested();

        // Skip symbols without the target attribute
        if (!symbol.TryGetAttributeWithFullyQualifiedMetadataName(AttributeDefinitions.ReactiveCollectionAttributeType, out _))
        {
            return default;
        }

        if (symbol is not IFieldSymbol fieldSymbol)
        {
            return default;
        }

        // Validate the target type
        if (!fieldSymbol.IsTargetTypeValid())
        {
            builder.Add(
                    InvalidReactiveObjectError,
                    fieldSymbol,
                    fieldSymbol.ContainingType,
                    fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        return CreateResult(context, fieldSymbol, builder, token);
    }

    /// <summary>Creates metadata for a validated ReactiveCollection field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="fieldSymbol">The validated field symbol.</param>
    /// <param name="builder">The diagnostics collected while processing the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics.</returns>
    private static Result<ReactiveCollectionFieldInfo?> CreateResult(
        in GeneratorAttributeSyntaxContext context,
        IFieldSymbol fieldSymbol,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var fieldName = fieldSymbol.Name;
        var propertyName = fieldSymbol.GetGeneratedPropertyName();

        // Check for name collisions
        if (fieldName == propertyName)
        {
            builder.Add(
                ReactivePropertyNameCollisionError,
                fieldSymbol,
                fieldSymbol.ContainingType,
                fieldSymbol.Name);
            return new(default, builder.ToImmutable());
        }

        return CreateMetadata(context, fieldSymbol, fieldName, propertyName, builder, token);
    }

    /// <summary>Creates the generated property metadata for a non-conflicting field.</summary>
    /// <param name="context">The attribute syntax context for the field.</param>
    /// <param name="fieldSymbol">The validated field symbol.</param>
    /// <param name="fieldName">The source field name.</param>
    /// <param name="propertyName">The generated property name.</param>
    /// <param name="builder">The diagnostics collected while processing the field.</param>
    /// <param name="token">The cancellation token for the generator operation.</param>
    /// <returns>The field metadata and diagnostics.</returns>
    private static Result<ReactiveCollectionFieldInfo?> CreateMetadata(
        in GeneratorAttributeSyntaxContext context,
        IFieldSymbol fieldSymbol,
        string fieldName,
        string propertyName,
        ImmutableArrayBuilder<DiagnosticInfo> builder,
        CancellationToken token)
    {
        var typeNameWithNullabilityAnnotations = fieldSymbol.Type.GetFullyQualifiedNameWithNullabilityAnnotations();
        var fieldDeclaration = (FieldDeclarationSyntax)context.TargetNode.Parent!.Parent!;
        var initializer = GetInitializer(fieldDeclaration);

        token.ThrowIfCancellationRequested();

        context.GetForwardedAttributes(
                builder,
                fieldSymbol,
                fieldDeclaration.AttributeLists,
                token,
                out var forwardedPropertyAttributes);

        token.ThrowIfCancellationRequested();

        // Get the nullability info for the property
        var (isReferenceTypeOrUnconstraindTypeParameter, includeMemberNotNullOnSetAccessor) =
            GetNullabilityInfo(fieldSymbol, context.SemanticModel);

        token.ThrowIfCancellationRequested();

        // Get the containing type info
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
                forwardedPropertyAttributes),
            builder.ToImmutable());
    }

    /// <summary>Gets nullability metadata for a generated reactive collection property.</summary>
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

    /// <summary>Gets the first declared field initializer.</summary>
    /// <param name="fieldDeclaration">The source field declaration.</param>
    /// <returns>The initializer text, when present.</returns>
    private static string? GetInitializer(FieldDeclarationSyntax fieldDeclaration)
    {
        var variables = fieldDeclaration.Declaration.Variables;
        return variables.Count > 0 ? variables[0].Initializer?.ToFullString() : null;
    }

    /// <summary>Generates the complete source document for reactive collection declarations.</summary>
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
        ReactiveCollectionFieldInfo[] properties,
        ReactiveUiIntegration integration)
    {
        // Get Parent class details from properties.ParentInfo
        var (parentClassDeclarationsString, closingBrackets) = TargetInfo.GenerateParentClassDeclarations(GetParentInfos(properties));

        var classes = GenerateClassWithProperties(containingTypeName, containingClassVisibility, containingType, properties);

        return $$"""
// <auto-generated/>
{{integration.UsingDirectives}}

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
        ReactiveCollectionFieldInfo[] properties)
    {
        // Includes 2 tabs from the property declarations so no need to add them here.
        var propertyDeclarations = GetPropertyDeclarations(properties);
        var collectionChangedDeclaration = GetCollectionChangedDeclaration();

        return
$$"""

    {{containingClassVisibility}} partial {{containingType}} {{containingTypeName}}
    {
        [global::System.CodeDom.Compiler.GeneratedCode("{{GeneratorName}}", "{{GeneratorVersion}}")]
{{propertyDeclarations}}

        [global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
{{collectionChangedDeclaration}}
    }
""";
    }

    /// <summary>Creates the collection-change helper while preserving its generated one-line format.</summary>
    /// <returns>The generated collection-change helper declaration.</returns>
    private static string GetCollectionChangedDeclaration()
    {
        var builder = new StringBuilder("        private static global::System.Collections.Specialized.");
        _ = builder.Append("NotifyCollectionChangedEventHandler CollectionChanged(IReactiveObject @this, ");
        _ = builder.Append("string propName)=> (_, _) =>  @this.RaisePropertyChanged(propName);");
        return builder.ToString();
    }

    /// <summary>Generates a property declaration for a reactive collection field.</summary>
    /// <param name="propertyInfo">The source field metadata.</param>
    /// <returns>The generated property declaration.</returns>
    private static string GetPropertySyntax(ReactiveCollectionFieldInfo propertyInfo)
    {
        var propertyAttributes = GetPropertyAttributes(propertyInfo);

        return $$"""
        /// <inheritdoc cref="{{propertyInfo.FieldName}}"/>
        {{propertyAttributes}}
        public {{propertyInfo.TypeNameWithNullabilityAnnotations}} {{propertyInfo.PropertyName}}
        {
            get => {{propertyInfo.FieldName}};
            set
            {
                if (value == null)
                {
                    {{propertyInfo.PropertyName}}.CollectionChanged -= CollectionChanged(this, nameof({{propertyInfo.PropertyName}}));
                }

                {{propertyInfo.FieldName}} = value;
                this.RaisePropertyChanged(nameof({{propertyInfo.PropertyName}}));

                if ({{propertyInfo.FieldName}} != null)
                {
                    // Remove the old handler if it exists
                    {{propertyInfo.PropertyName}}.CollectionChanged -= CollectionChanged(this, nameof({{propertyInfo.PropertyName}}));

                    {{propertyInfo.PropertyName}}.CollectionChanged += CollectionChanged(this, nameof({{propertyInfo.PropertyName}}));
                }
            }
        }
""";
    }

    /// <summary>Gets parent information for each generated property.</summary>
    /// <param name="properties">The generated property metadata.</param>
    /// <returns>The parent information for the generated properties.</returns>
    private static TargetInfo?[] GetParentInfos(ReactiveCollectionFieldInfo[] properties)
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
    private static string GetPropertyDeclarations(ReactiveCollectionFieldInfo[] properties)
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
    private static string GetPropertyAttributes(ReactiveCollectionFieldInfo propertyInfo)
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
